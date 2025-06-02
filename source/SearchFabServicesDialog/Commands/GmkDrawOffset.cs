using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Fabrication;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CODE.Free.Utils;
using Nice3point.Revit.Toolkit.External;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UIFramework;
using Point = Autodesk.Revit.DB.Point;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using Transform = Autodesk.Revit.DB.Transform;
#if (REVIT2025)
                        //2025
#else
#endif
namespace CODE.Free
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class GmkDrawOffset : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                OffsetUpdater updater = new OffsetUpdater(UiApplication.ActiveAddInId);
                if (UpdaterRegistry.IsUpdaterRegistered(OffsetUpdater.UpdaterId))
                {
                    UpdaterRegistry.UnregisterUpdater(OffsetUpdater.UpdaterId);
                }
                UpdaterRegistry.RegisterUpdater(updater, this.Document);
                CreateRollingOffset(UiDocument);
            }
            catch (Exception ex)
            {
                UI.Popup(ex.Message);
            }
        }

        void CreateMidpoints(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View activeView = uidoc.ActiveGraphicalView;

            // Check if the current view is an assembly view
            if (activeView.AssociatedAssemblyInstanceId != ElementId.InvalidElementId)
            {
                // Get the assembly instance
                AssemblyInstance assembly = doc.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance;
                if (assembly == null) return;
                // Get all member IDs of the assembly
                List<ElementId> memberIds = assembly.GetMemberIds().ToList();

                // Find all DirectShape GenericModels within memberIds that have a Name property
                List<DirectShape> directShapes = new FilteredElementCollector(doc, memberIds)
                    .OfClass(typeof(DirectShape))
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .Cast<DirectShape>()
                    .Where(ds => !string.IsNullOrEmpty(ds.Name))
                    .ToList();
                using (Transaction tr = new Transaction(doc, "Create Midpoints"))
                {
                    tr.Start();
                    List<ElementId> midpoints = new List<ElementId>();
                    List<ElementId> fabParts = new List<ElementId>();
                    // create midpoint for all FabricationParts that are couplers or valves
                    foreach (ElementId id in memberIds)
                    {
                        Element elem = doc.GetElement(id);
                        if (elem is FabricationPart fabPart)
                        {
                            int patternNo = fabPart.ItemCustomId;
                            string typeName = fabPart.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)?.AsValueString() ?? "";
                            if (patternNo == 2522 || typeName.Contains("Valve"))
                            //if ((patternNo == 2522 && typeName.Contains("Coupling ~")) || typeName.Contains("Valve"))
                            {
                                // Get the connectors
                                var connectors = fabPart.ConnectorManager.Connectors.Cast<Connector>().ToList();
                                if (connectors.Count != 2)
                                    throw new InvalidOperationException("Part must have exactly two connectors.");

                                // Calculate midpoint
                                XYZ midpoint = (connectors[0].Origin + connectors[1].Origin) / 2;

                                // Find if a DirectShape with Name matching fabPart's ElementId already exists
                                DirectShape existingDirectShape = directShapes
                                    .FirstOrDefault(ds => ds.Name == fabPart.Id.ToLong().ToString());

                                if (existingDirectShape != null)
                                {
                                    // Optionally, skip creation or update the existing DirectShape as needed
                                    // Get the geometry of the DirectShape
                                    Options options = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true };
                                    GeometryElement geomElem = existingDirectShape.get_Geometry(options);

                                    // Try to find a Point in the geometry
                                    XYZ origin = existingDirectShape.GetPointReferences().FirstOrDefault()?.Coord;
                                    if (origin == null)
                                    {
                                        throw new InvalidOperationException("No origin found in existing DirectShape geometry.");
                                    }
                                    ElementTransformUtils.MoveElement(doc, existingDirectShape.Id, midpoint - origin);
                                    midpoints.Add(existingDirectShape.Id);
                                }
                                else
                                {
                                    // Create a DirectShape with the Point as its geometry
                                    DirectShape ds = doc.CreatePoint(midpoint, fabPart.Id.ToLong().ToString());
                                    midpoints.Add(ds.Id);
                                }
                                fabParts.Add(fabPart.Id);
                            }
                        }
                        else if (elem is DirectShape shape)
                        {
                            // If the element is a DirectShape, check if its Name corresponds to a FabricationPart's ElementId.
                            // If not, and the referenced FabricationPart does not exist, delete the DirectShape.
                            if (long.TryParse(shape.Name, out long num))
                            {
                                ElementId partId = new ElementId(num);
                                if (partId == ElementId.InvalidElementId || doc.GetElement(partId) == null)
                                {
                                    doc.Delete(shape.Id);
                                }
                            }
                        }
                    }

                    if (midpoints.Count > 0)
                    {
                        assembly.AddMemberIds(midpoints);
                    }
                    if (fabParts.Count > 0)
                    {
                        UpdaterRegistry.AddTrigger(OffsetUpdater.UpdaterId, doc, fabParts, Element.GetChangeTypeAny());
                    }
                    tr.Commit();
                }
            }
        }
        // Selection filter for DirectShape of GenericModel
        class DirectShapeSelectionFilter : ISelectionFilter
        {
            private readonly Document _doc;
            public DirectShapeSelectionFilter(Document doc) { _doc = doc; }
            public bool AllowElement(Element elem)
            {
                return elem is DirectShape && elem.Category != null && elem.Category.Id.ToLong() == (int)BuiltInCategory.OST_GenericModel;
            }
            public bool AllowReference(Reference reference, XYZ position) => true;
        }

        class OffsetUpdater : IUpdater
        {
            private static Guid _guid => new Guid("E17BD0D7-C0FD-4C01-8E01-89ECE5193299");
            private static AddInId _addInId;
            public static UpdaterId UpdaterId;
            public OffsetUpdater(AddInId addInId)
            {
                _addInId = addInId;
                UpdaterId = new UpdaterId(_addInId, _guid);
            }
            public void Execute(UpdaterData data)
            {
                /*
                dimensions will reference that midpoint-like directshape instead
                and the offset shape should be updated when the fabparts move without breaking existing travel dimensions
                */

                Document doc = data.GetDocument();
                foreach (ElementId movedId in data.GetModifiedElementIds())
                {
                    Element movedElement = doc.GetElement(movedId);
                    AssemblyInstance assemblyInstance = doc.GetElement(movedElement.AssemblyInstanceId) as AssemblyInstance;
                    if (assemblyInstance == null)
                    {
                        // If the moved element is not part of an assembly, skip it
                        continue;
                    }

                    // Get all member IDs of the assembly
                    ICollection<ElementId> memberIds = assemblyInstance.GetMemberIds();

                    #region 053125-updateoffsetboxortriangleorpoints
                    // Find all DirectShape GenericModels within memberIds that have a Name property that is a long integer
                    IEnumerable<Element> directShapes = new FilteredElementCollector(doc, memberIds)
                        .OfClass(typeof(DirectShape))
                        .OfCategory(BuiltInCategory.OST_GenericModel)
                        .Where(ds => !string.IsNullOrEmpty(ds.Name));
                    // .Where(ds => long.TryParse(ds.Name, out long _));
                    if (directShapes.Count() == 0) return;
                    // update all offset shapes
                    for (int i = 0; i < directShapes.Count(); i++)
                    {
                        DirectShape ds = directShapes.ElementAt(i) as DirectShape;
                        string[] twoIds = ds.Name.Split(',');
                        if (twoIds.Length == 2 &&
                            long.TryParse(twoIds[0], out long part1) &&
                            long.TryParse(twoIds[1], out long part2))
                        {
                            ElementId id1 = new ElementId(part1);
                            ElementId id2 = new ElementId(part2);
                            if (id1.Equals(ElementId.InvalidElementId) || id2.Equals(ElementId.InvalidElementId))
                            {
                                // If either part1 or part2 is invalid, delete this DirectShape
                                // doc.Delete(ds.Id);
                                continue;
                            }
                            FabricationPart fabpart1 = doc.GetElement(id1) as FabricationPart;
                            FabricationPart fabpart2 = doc.GetElement(id2) as FabricationPart;
                            if (fabpart1 == null || fabpart2 == null)
                            {
                                // If either FabricationPart does not exist, delete this DirectShape
                                // doc.Delete(ds.Id);
                                continue;
                            }
                            XYZ p1 = fabpart1.CalculateMidpoint();
                            XYZ p2 = fabpart2.CalculateMidpoint();
                            // this directshape is a rolling offset box, rebuild it
                            // use "OffsetBoxMethod()" here, then skip to next directshape
                            DrawRollingOffset(id1, id2, p1, p2, new UIDocument(doc), true);
                            doc.Delete(ds.Id); //MM, for testing only, delete later

                            continue;
                        }
                        //if directshape name is a single long integer, it is a midpoint-like directshape
                        long iden = long.Parse(ds.Name);
                        Point point = ds.GetPointReferences().FirstOrDefault();
                        if (point == null)
                        {
                            // If the DirectShape has no point references, skip it
                            continue;
                        }
                        ElementId id = new ElementId(iden);
                        FabricationPart part = doc.GetElement(id) as FabricationPart;
                        if (part == null)
                        {
                            // If the FabricationPart does not exist, delete the DirectShape
                            // doc.Delete(ds.Id);
                            continue;
                        }
                        XYZ vector = XYZ.Zero;
                        // Get the connectors
                        var connectors = part.ConnectorManager.Connectors.Cast<Connector>().ToList();

                        string typeName = part.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)?.AsValueString() ?? "";
                        if (part.ItemCustomId == 2522 || typeName.Contains("Valve")) //this should update the added midpoints only
                        {
                            // Calculate midpoint
                            XYZ midpoint = (connectors[0].Origin + connectors[1].Origin) / 2;
                            vector = midpoint - point.Coord;
                        }
                        else if (part.IsAStraight()) //this should update the projected point in a rolling offset
                        {
                            // get connectors and find the one that is closest to the point coordinate
                            Connector connector = connectors
                                .OrderBy(c => c.Origin.DistanceTo(ds.GetPointReferences().FirstOrDefault()?.Coord ?? XYZ.Zero))
                                .FirstOrDefault();
                            // get the location of connector and move point to that location
                            vector = connector.Origin - point.Coord;
                        }
                        else
                        {
                            vector = part.CalculateMidpoint() - point.Coord;
                        }
                        try
                        {
                            ElementTransformUtils.MoveElement(doc, ds.Id, vector);
                        }
                        catch { }
                    }
                    #endregion
                }

            }
            public string GetAdditionalInformation() => "Updates offset shapes and dimensions in the current assembly view.";
            public ChangePriority GetChangePriority() => ChangePriority.Annotations;
            public UpdaterId GetUpdaterId() => UpdaterId;
            public string GetUpdaterName() => "Offset Updater";
        }
        void CreateRollingOffset(UIDocument uidoc)
        {
            CreateMidpoints(uidoc);
            Document doc = uidoc.Document;
            Reference ref1 = uidoc.Selection.PickObject(ObjectType.PointOnElement, new PointSelectionFilter(doc));
            Reference ref2 = uidoc.Selection.PickObject(ObjectType.PointOnElement, new PointSelectionFilter(doc));
            //if the user has picked two ends of the same pipe, just make a dimension. we can check for this by looking at the element associated with ref1 and ref2
            //if the user clicked a "planar offset" draw a triangle instead. might have to look at the orientation of a connected fitting (because pipe can be randomly rotated about its long axis)
            //else continue to create rolling offset box
            double distance = ref1.GlobalPoint.DistanceTo(ref2.GlobalPoint);
            if (ref1.ElementId == ref2.ElementId)
            {
                using (Transaction tr = new Transaction(doc))
                {
                    tr.Start("Create Dimension");
                    //create dimension
                    Line line = Line.CreateBound(ref1.GlobalPoint, ref2.GlobalPoint);
                    XYZ cp = line.Direction.CrossProduct(XYZ.BasisZ); //MK, use this for the sketchplane instead
                    double factor = line.Origin.Add(XYZ.BasisZ).DistanceTo(uidoc.ActiveGraphicalView.Origin) > line.Origin.DistanceTo(uidoc.ActiveGraphicalView.Origin) ? -1 : 1; //MK, add the basisZ so dim goes up (my bad)
                    line = line.CreateTransformed(Transform.CreateTranslation(XYZ.BasisZ * distance * factor)) as Line; //MK, use basisZ instead of cp
                    ReferenceArray refArray = new ReferenceArray();
                    refArray.Append(ref1);
                    refArray.Append(ref2);
                    Plane plane = Plane.CreateByNormalAndOrigin(cp, ref1.GlobalPoint); //MK, create sketchplane
                    uidoc.ActiveGraphicalView.SketchPlane = SketchPlane.Create(doc, plane); //MK, create sketchplane
                    Dimension dim = doc.Create.NewDimension(uidoc.ActiveGraphicalView, line, refArray);
                    dim.Below = "E - E";
                    tr.Commit();
                    return;
                }
            }
            else
            {
                //when user clicks on two distinct parts, create a rolling offset box
                ElementId id1 = ref1.ElementId;
                ElementId id2 = ref2.ElementId;
                using (Transaction tr = new Transaction(doc))
                {
                    tr.Start("Create Rolling Offset");
                    DrawRollingOffset(id1, id2, ref1.GlobalPoint, ref2.GlobalPoint, uidoc, false);
                    tr.Commit();
                }
            }
        }
        static CurveLoop GetCurveLoop(XYZ p1, XYZ p2)
        {
            XYZ c0;
            XYZ c2;

            //calculate height for the box and the two botttom points of the rectangle
            if (p1.Z < p2.Z) //
            {
                c0 = p1;
                c2 = new XYZ(p2.X, p2.Y, p1.Z);
                // height = p2.Z - p1.Z;
            }
            else
            {
                c0 = p2;
                c2 = new XYZ(p1.X, p1.Y, p2.Z);
                // height = p1.Z - p2.Z;
            }
            XYZ c1 = new XYZ(c2.X, c0.Y, c0.Z);
            XYZ c3 = new XYZ(c0.X, c2.Y, c0.Z);
            //create bound line for each side of rectangle with above points
            Line l0 = Line.CreateBound(c0, c1);
            Line l1 = Line.CreateBound(c1, c2);
            Line l2 = Line.CreateBound(c2, c3);
            Line l3 = Line.CreateBound(c3, c0);
            //create curveloop with lines
            CurveLoop loop = new CurveLoop();
            loop.Append(l0);
            loop.Append(l1);
            loop.Append(l2);
            loop.Append(l3);
            return loop;
        }
        static void DrawRollingOffset(ElementId id1, ElementId id2, XYZ p1, XYZ p2, UIDocument uidoc, bool inUpdater)
        {
            Document doc = uidoc.Document;
            ElementId assemId = doc.GetElement(id1).AssemblyInstanceId;
            double distance = p1.DistanceTo(p2);
            try
            {
                //create extrusion
                Solid ext = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { GetCurveLoop(p1, p2) }, XYZ.BasisZ, Math.Abs(p1.Z - p2.Z));
                DirectShape shape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                shape.SetShape(new List<GeometryObject>() { ext });
                shape.Name = $"{id1.ToLong()},{id2.ToLong()}"; //MK, use both element ids in the name
                if (assemId != ElementId.InvalidElementId)
                {
                    AssemblyInstance spool = doc.GetElement(assemId) as AssemblyInstance;
                    spool.AddMemberIds(new List<ElementId>() { shape.Id });
                }
                if (!uidoc.ActiveGraphicalView.AssociatedAssemblyInstanceId.Equals(assemId))
                {
                    return;
                }
                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetSurfaceTransparency(75);
                LinePatternElement linePattern = LinePatternElement.GetLinePatternElementByName(doc, "Hidden");
                ogs.SetProjectionLinePatternId(linePattern.Id);
                FillPatternElement fillPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "<Solid fill>");
                ogs.SetSurfaceForegroundPatternId(fillPattern.Id);
                uidoc.ActiveGraphicalView.SetElementOverrides(shape.Id, ogs);
                try
                {
                    doc.Regenerate();
                }
                catch { }
                //get geometry from extrusion
                GeometryElement geoElem = shape.get_Geometry(new Options() { ComputeReferences = true });
                //get geometry instance from geometry element
                Solid solid = geoElem.First() as Solid;
                PlanarFace lowest = GetLowestFace(solid);
                PlanarFace furthest = GetFurthestFaceFromView(solid, uidoc.ActiveGraphicalView);
                List<Edge> edges = SortEdgesByNearestFromView(lowest, uidoc.ActiveGraphicalView);
                uidoc.ActiveGraphicalView.SketchPlane = SketchPlane.Create(doc, lowest.Reference);
                DimensionEdges(edges[0], lowest.FaceNormal, distance, uidoc.ActiveGraphicalView);
                DimensionEdges(edges[1], lowest.FaceNormal, distance, uidoc.ActiveGraphicalView);
                List<Edge> sideEdges = SortEdgesByNearestFromView(furthest, uidoc.ActiveGraphicalView);
                uidoc.ActiveGraphicalView.SketchPlane = SketchPlane.Create(doc, furthest.Reference);
                Edge sideEdge = sideEdges.FirstOrDefault(e => (e.AsCurve() as Line).Direction.IsVertical()) ?? sideEdges[0];
                DimensionEdges(sideEdge, furthest.FaceNormal, distance, uidoc.ActiveGraphicalView);
                //get faces
            }
            catch { }
        }
        
        static PlanarFace GetLowestFace(Solid solid)
        {
            PlanarFace lowest = null;
            foreach (Face f in solid.Faces)
            {
                if (f is PlanarFace pf)
                {
                    if (lowest == null)
                    {
                        lowest = pf;
                    }
                    else
                    {
                        if (pf.Origin.Z < lowest.Origin.Z)
                        {
                            lowest = pf;
                        }
                    }
                }
            }
            return lowest;
        }
        static PlanarFace GetFurthestFaceFromView(Solid solid, View view)
        {
            PlanarFace furthest = null;
            foreach (Face f in solid.Faces)
            {
                if (f is PlanarFace pf)
                {
                    if (pf.FaceNormal.IsVertical())
                    {
                        continue;
                    }
                    if (furthest == null)
                    {
                        furthest = pf;
                    }
                    else
                    {
                        if (pf.Origin.DistanceTo(view.Origin) > furthest.Origin.DistanceTo(view.Origin))
                        {
                            furthest = pf;
                        }
                    }
                }
            }
            return furthest;
        }
        static List<Edge> SortEdgesByNearestFromView(PlanarFace face, View view)
        {
            List<Edge> edges = new List<Edge>();
            foreach (EdgeArray edgeArray in face.EdgeLoops)
            {
                foreach (Edge e in edgeArray)
                {
                    edges.Add(e);
                }
            }
            edges.Sort((a, b) => a.Evaluate(0.5).DistanceTo(view.Origin).CompareTo(b.Evaluate(0.5).DistanceTo(view.Origin)));
            return edges;
        }
        static void DimensionEdges(Edge edge, XYZ normal, double offset, View view)
        {
            ReferenceArray referenceArray = new ReferenceArray();
            referenceArray.Append(edge.GetEndPointReference(0));
            referenceArray.Append(edge.GetEndPointReference(1));
            Line line3 = edge.AsCurve() as Line;
            XYZ cp = line3.Direction.CrossProduct(normal);
            double factor = line3.Origin.Add(cp).DistanceTo(view.Origin) > line3.Origin.DistanceTo(view.Origin) ? -1 : 1;
            line3 = line3.CreateTransformed(Transform.CreateTranslation(cp * offset * factor)) as Line;
            Dimension dim = view.Document.Create.NewDimension(view, line3, referenceArray);
            dim.Below = "C - C";
        }
        public class PointSelectionFilter : ISelectionFilter
        {
            public Document _doc { get; set; }
            public PointSelectionFilter(Document doc)
            {
                _doc = doc;
            }
            public bool AllowElement(Element elem)
            {
                return
                    elem is FabricationPart ||
                    (elem is DirectShape ds && long.TryParse(ds.Name, out long _)); //true if the directshape is named after elementid of a fabpart, meaning this is a midpoint for a coupler or valve
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                /* //select only points
                GeometryElement geo = _doc.GetElement(reference).get_Geometry(new Options() { ComputeReferences = true, IncludeNonVisibleObjects = true });
                GeometryInstance geoInstance = geo.First() as GeometryInstance;
                IEnumerable<Point> points = geoInstance.GetInstanceGeometry().Where(s => s is Point).Cast<Point>();
                return reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_NONE && points.Any(p => p.Coord.IsAlmostEqualTo(position));
                */

                //possibly more accurate than position
                //position = reference.GlobalPoint;

                //select only endpoints and midpoints of fittings
                try
                {
                    //remove anything that isn't a point reference type
                    if (reference.ElementReferenceType != ElementReferenceType.REFERENCE_TYPE_NONE)
                        return false;
                    Element elem = _doc.GetElement(reference);
                    if (elem is FabricationPart fabricationPart)
                    {
                        // Get the connector manager from the fabrication part.
                        ConnectorManager connectorManager = fabricationPart.ConnectorManager;
                        if (connectorManager != null)
                        {
                            //detect couplings and provide midpoint
                            /*//actually none of this works if there is no point reference available in the part
                            string typeName = elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)?.AsValueString();
                            bool isCoupling = typeName != null && typeName.Contains("Coupling");
                            bool isReducer = typeName != null && typeName.Contains("Reducer");
                            bool isElbow = typeName != null && (typeName.Contains("Elbow") || typeName.Contains("Bend"));

                            if (isCoupling || isReducer || isElbow)
                            {
                                var connectors = connectorManager.Connectors.Cast<Connector>().ToList();
                                if (connectors.Count == 2)
                                {
                                    XYZ midpoint = (connectors[0].Origin + connectors[1].Origin) / 2;
                                    if (midpoint.DistanceTo(position) < 0.05)
                                        return true;
                                }
                            }
                            //*/
                            //detect olets and provide midpoint



                            string typeName = elem.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)?.AsValueString();

                            //flange faces
                            bool isFlange = typeName != null && (typeName.Contains("Flange") || typeName.Contains("Stub End"));
                            if (isFlange)
                            {
                                var flangeConnectors = connectorManager.Connectors.Cast<Connector>().ToList();
                                if (flangeConnectors.Count == 2)
                                {
                                    //TODO: find connector that attaches to a gasket and that will be the face.
                                    foreach (var flangeConnector in flangeConnectors)
                                    {
                                        (FabricationPart, string) connectedType = GetSimpleConnectedPartTuple(_doc, flangeConnector);
                                        if (connectedType.Item2.Contains("Gasket"))//|| connectedType.Contains("O Ring"))
                                        {
                                            //check if mouse is next to flange endpoint shared with gasket
                                            FabricationPart gasket = connectedType.Item1;
                                            ConnectorManager gasketConnectorManager = gasket.ConnectorManager;
                                            foreach (Connector gasketConnector in gasketConnectorManager.Connectors)
                                            {
                                                //todo check to see if there's already a dimension on it... because we wouldn't be trying to dimension to that one anymore.
                                                // or how about... the closest to ref 1/ 2 
                                                if (gasketConnector.Origin.IsAlmostEqualTo(flangeConnector.Origin, 0.0026d) && flangeConnector.Origin.IsAlmostEqualTo(position))
                                                    return true;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }

                            //coupling endpoints
                            bool isCoupling = typeName != null && typeName.Contains("Coupling");
                            if (isCoupling)
                            {
                                var connectors = connectorManager.Connectors.Cast<Connector>().ToList();
                                if (connectors.Count == 2)
                                {
                                    if (connectors[0].Origin.IsAlmostEqualTo(position))
                                        return true;
                                    else if (connectors[1].Origin.IsAlmostEqualTo(position))
                                        return true;
                                    else
                                        return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            //olet endpoints
                            bool isOlet = typeName != null && (typeName.Contains("olet") || typeName.Contains("Olet"));
                            if (isOlet)
                            {
                                //if we were to check that the fabricationpart.origin is equal to the position, we would be able to select the olet's origin in the host pipe.
                                //but we probably need both, so just grab each connector no matter what and ignore the rest.
                                //todo: ask if we need to filter out the unused connector.
                                var connectors = connectorManager.Connectors.Cast<Connector>().ToList();
                                if (connectors.Count == 2)
                                {
                                    if (connectors[0].Origin.IsAlmostEqualTo(position))
                                        return true;
                                    else if (connectors[1].Origin.IsAlmostEqualTo(position))
                                        return true;
                                    else
                                        return false;
                                }
                                else
                                {
                                    return false; //i'm not even sure it's possible to have an olet with more or less connectors
                                }
                            }

                            //elbow midpoint
                            bool isElbow = typeName != null && (typeName.Contains("Elbow") || typeName.Contains("Bend"));
                            // bool elbow = false;
                            if (isElbow)
                            {
                                try
                                {
                                    if (!fabricationPart.IsAStraight() && fabricationPart.CalculateMidpoint().IsAlmostEqualTo(position))
                                    //if (!fabricationPart.IsAStraight() && fabricationPart.CalculateMidpoint().DistanceTo(position) < 0.05)
                                    {
                                        // elbow = true;
                                        return true;
                                    }
                                }
                                catch //(Exception e)
                                {
                                    //TaskDialog.Show("AllowReference compare to midpoint Error", e.Message + "\n" + e.StackTrace);
                                }
                            }


                            //Endpoints at the end of spools not connecting to other spool
                            var unusedConnectors = connectorManager.UnusedConnectors;
                            foreach (Connector unusedConnector in unusedConnectors)
                            {
                                // Check if the clicked position is almost equal to the unused connector's origin.
                                //if (unusedConnector.Origin.IsAlmostEqualTo(position))
                                if (unusedConnector.Origin.IsAlmostEqualTo(position))
                                {
                                    return true;
                                }
                            }

                            //Endpoints connected to a different spool while rejecting all interior endpoint connections except for welds and pipe to pipe.
                            //todo: get smarter about the weld dimensioning... perhaps based on what kind of pipe sytem we are hovering over.
                            foreach (Connector connector in connectorManager.Connectors)
                            {
                                //check if the connector has another part connected to it from a different assembly
                                //for each connector, find all refs (connectors attached to it), check their owner assembly name against this connector's owner assembly name
                                //gather info about this connector, like the spool name
                                var thisOwnerId = connector.Owner.Id;
                                var thisSpoolId = _doc.GetElement(thisOwnerId).AssemblyInstanceId;
                                //find the connected parts
                                var connectedRefs = connector.AllRefs;
                                var connectorCount = connectedRefs.Size;
                                foreach (Connector otherConnector in connectedRefs)
                                {
                                    //to support pipe welded to pipe, we need to check if the other part is a pipe or weld in the same assembly, and the part we are hovering over should be a pipe or a weld already if it's not, continue.
                                    //find other part type

                                    var otherOwnerId = otherConnector.Owner.Id;
                                    var otherPartType = _doc.GetElement(otherOwnerId).get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)?.AsValueString();
                                    var otherSpoolId = _doc.GetElement(otherOwnerId).AssemblyInstanceId;
                                    bool isPipe = typeName != null && typeName.Contains("Pipe ~");
                                    bool isWeld = typeName != null && typeName.Contains("Weld:");
                                    if ((isPipe || isWeld) && (otherPartType.Contains("Pipe ~") || otherPartType.Contains("Weld:")) && otherSpoolId == thisSpoolId)
                                    {
                                        if (connector.Origin.IsAlmostEqualTo(position))
                                            return true;
                                    }
                                    else
                                    {

                                        //if the connected part is from a different assembly, accept it
                                        if (thisSpoolId != otherSpoolId /* ||  unused connector */)
                                        {
                                            // Check if the clicked position is almost equal to the connector's origin.
                                            if (connector.Origin.IsAlmostEqualTo(position))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }//end foreach otherConnector
                            }//end foreach connector
                        }//end if connector manager is not null
                    }//end if fabrication part
                    else if (elem is DirectShape ds)
                    {
                        //check if the directshape is named after elementid of a fabpart, meaning this is a midpoint for a coupler or valve
                        if (long.TryParse(ds.Name, out long _))
                        {
                            // Check if the clicked position is almost equal to the direct shape's origin.
                            if (ds.Location is LocationPoint locationPoint && locationPoint.Point.IsAlmostEqualTo(position))
                            {
                                return true;
                            }
                            Point pt = ds.GetPointReferences().First();
                            if (pt != null && pt.Coord.IsAlmostEqualTo(position))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskDialog.Show("AllowReference Error", e.Message + "\n" + e.StackTrace);
                }
                return false;
            }//end AllowReference
            private static (FabricationPart, string) GetSimpleConnectedPartTuple(Document doc, Connector connector)
            {
                string partType = string.Empty;
                FabricationPart fabp = null;

                // Get the referenced connectors from the given connector
                ConnectorSet referencedConnectors = connector.AllRefs;

                if (referencedConnectors != null && referencedConnectors.Size > 0)
                {
                    foreach (Connector referencedConnector in referencedConnectors)
                    {
                        // Get the element connected to this referenced connector
                        Element referencedElement = doc.GetElement(referencedConnector.Owner.Id);

                        fabp = referencedElement as FabricationPart;

                        // Get the family and type parameter value of the referenced element
                        partType = referencedElement.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)?.AsValueString();

                        // If partType is found, return it
                        if (fabp != null)
                        {
                            break;
                        }
                    }
                }

                return (fabp, partType);
            }
        }//end PointSelectionFilter
        bool IsEndOfSpool(FabricationPart fp)
        {
            foreach (Connector con in fp.ConnectorManager.Connectors)
            {
                if (con.AllRefs.Size == 0)
                {
                    return true;
                }
            }
            return false;
        }
        
    }
    public static class OffsetUpdaterExtensions
    {
        public static XYZ CalculateMidpoint(this FabricationPart fabp)
        {
            var cm = fabp.ConnectorManager;
            var cs = cm.Connectors;
            var csi = cs.ForwardIterator();
            var basisZ1 = new XYZ();
            var basisZ2 = new XYZ();
            List<XYZ> endpoints = new List<XYZ>();

            while (csi.MoveNext())
            {
                var connector = csi.Current as Connector;
                if (connector != null)
                {
                    endpoints.Add(connector.Origin);
                    var transform = connector.CoordinateSystem as Transform;
                    if (endpoints.Count == 1)
                        basisZ1 = transform.BasisZ;
                    else if (endpoints.Count == 2)
                        basisZ2 = transform.BasisZ;
                }
            }

            if (endpoints.Count < 2)
            {
                Trace.WriteLine("Error\n" + "Elbow does not have two valid connectors.");
                return XYZ.Zero;
            }

            XYZ redVector = new XYZ(endpoints[1].X - endpoints[0].X,
                                          endpoints[1].Y - endpoints[0].Y,
                                          endpoints[1].Z - endpoints[0].Z);

            XYZ whiteVector = new XYZ(basisZ1.X, basisZ1.Y, basisZ1.Z).Normalize().Flip();
            XYZ yellowVector = new XYZ(basisZ2.X, basisZ2.Y, basisZ2.Z).Normalize().Flip();

            XYZ numerator = yellowVector.CrossProduct(redVector);
            XYZ denominator = yellowVector.CrossProduct(whiteVector);
            double ratio = numerator.GetLength() / denominator.GetLength(); //was magnitude
            XYZ bye = whiteVector.Multiply(ratio);
            double dotProduct = numerator.DotProduct(denominator);

            XYZ midpoint = dotProduct >= 0
                ? new XYZ(endpoints[0].X + bye.X, endpoints[0].Y + bye.Y, endpoints[0].Z + bye.Z)
                : new XYZ(endpoints[0].X - bye.X, endpoints[0].Y - bye.Y, endpoints[0].Z - bye.Z);

            return midpoint;
        }
        public static XYZ Flip(this XYZ vector)
        {
            return new XYZ(-vector.X, -vector.Y, -vector.Z);
        }
    }
}
