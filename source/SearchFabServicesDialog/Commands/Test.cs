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
    public class Test : ExternalCommand
    {
        static int loop = 0;
        public override void Execute()
        {
            try
            {
                //LoadCadmepServices();
                //TryCallExportPCF();
                //CreateRollingOffset();
                //ShowReferenceType();
                //AddIconToTab();
                //CustomizeTabColor();
                //UpdateDimension();
                //CreateRollingOffset();
                //AddItm();

                //ParsePcf();

                //GetPartDictionary();
                //AddItms();
                //GetPipeItmPaths();

                //GetMaps();

                //CODE.Free.ViewModels.PcfImportTableViewModel vm = new CODE.Free.ViewModels.PcfImportTableViewModel();
                //CODE.Free.Views.PcfImportTableView view = new CODE.Free.Views.PcfImportTableView(vm);
                //view.Show();

                //TFSCreateRollingOffset(UiDocument);
                //DirectShape shape = DrawFace();
                //if (shape != null)
                //{
                //    DimensionDirectShape(shape);
                //}

            }
            catch (Exception ex)
            {
                UI.Popup(ex.Message);
            }
        }
        void AddEvent()
        {
            UIApplication uiApp = new UIApplication(Document.Application);
            uiApp.Application.DocumentSynchronizedWithCentral += OnSynchronizedWithCentral;
        }

        private void OnSynchronizedWithCentral(object sender, Autodesk.Revit.DB.Events.DocumentSynchronizedWithCentralEventArgs e)
        {
            if (e.Status == RevitAPIEventStatus.Succeeded)
            {
                ReloadRevitLinks();
            }
        }

        private void ReloadRevitLinks()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Document);
            ICollection<Element> revitLinks = collector.OfClass(typeof(RevitLinkType)).ToElements();

            using (Transaction tr = new Transaction(Document, "Reload Revit Links"))
            {
                tr.Start();
                foreach (RevitLinkType link in revitLinks)
                {
                    link.Reload();
                }
                tr.Commit();
            }
        }
        void DimensionDirectShape(DirectShape shape)
        {
            try
            {
                using (Transaction tr = new Transaction(Document, "Dimension DirectShape"))
                {
                    tr.Start();
                    GeometryElement geoElem = shape.get_Geometry(new Options() { ComputeReferences = true });
                    Solid solid = geoElem.First() as Solid;
                    PlanarFace lowest = GetLowestFace(solid);
                    PlanarFace furthest = GetFurthestFaceFromView(solid, UiDocument.ActiveGraphicalView);
                    List<Edge> edges = SortEdgesByNearestFromView(lowest, UiDocument.ActiveGraphicalView);
                    UiDocument.ActiveGraphicalView.SketchPlane = SketchPlane.Create(Document, lowest.Reference);
                    DimensionEdges(edges[0], lowest.FaceNormal, edges[0].ApproximateLength, UiDocument.ActiveGraphicalView);
                    DimensionEdges(edges[1], lowest.FaceNormal, edges[1].ApproximateLength, UiDocument.ActiveGraphicalView);
                    List<Edge> sideEdges = SortEdgesByNearestFromView(furthest, UiDocument.ActiveGraphicalView);
                    UiDocument.ActiveGraphicalView.SketchPlane = SketchPlane.Create(Document, furthest.Reference);
                    Edge sideEdge = sideEdges.FirstOrDefault(e => (e.AsCurve() as Line).Direction.IsVertical()) ?? sideEdges[0];
                    DimensionEdges(sideEdge, furthest.FaceNormal, sideEdge.ApproximateLength, UiDocument.ActiveGraphicalView);
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }
        DirectShape DrawFace()
        {
            DirectShape shape = null;
            try
            {
                // Prompt user to select two points
                Reference ref1 = UiDocument.Selection.PickObject(ObjectType.PointOnElement, "Select first point");
                Reference ref2 = UiDocument.Selection.PickObject(ObjectType.PointOnElement, "Select second point");

                // Get the points from the references
                XYZ point1 = ref1.GlobalPoint;
                XYZ point2 = ref2.GlobalPoint;

                // Calculate the third point to form a triangle
                XYZ point3 = new XYZ(point2.X, point1.Y, point1.Z);

                // Create lines to form the triangle
                //Line line1 = Line.CreateBound(point1, point2);
                //Line line2 = Line.CreateBound(point2, point3);
                //Line line3 = Line.CreateBound(point3, point1);

                //// Create a curve loop with the lines
                //CurveLoop curveLoop = new CurveLoop();
                //curveLoop.Append(line1);
                //curveLoop.Append(line2);
                //curveLoop.Append(line3);

                // Create a solid extrusion from the curve loop
                using (Transaction tr = new Transaction(Document, "Create Triangle Face"))
                {
                    tr.Start();
                    TessellatedShapeBuilder builder = new TessellatedShapeBuilder
                    {
                        Target = TessellatedShapeBuilderTarget.AnyGeometry,
                        Fallback = TessellatedShapeBuilderFallback.Mesh
                    };
                    builder.OpenConnectedFaceSet(false);
                    UI.Test(1);
                    ElementId matId = new FilteredElementCollector(Document)
                        .OfClass(typeof(Material))
                        .FirstElementId();
                    UI.Test(2);
                    TessellatedFace face = new TessellatedFace(new List<XYZ>() { point1, point2, point3}, matId);
                    builder.AddFace(face);
                    UI.Test(3);
                    builder.CloseConnectedFaceSet();
                    builder.Build();
                    UI.Test(4);
                    TessellatedShapeBuilderResult res = builder.GetBuildResult();
                    UI.Test($"res null:{res == null}");
                    shape = DirectShape.CreateElement(Document, new ElementId(BuiltInCategory.OST_GenericModel));
                    UI.Test($"shape null:{shape == null}");
                    shape.SetShape(res.GetGeometricalObjects());
                    UI.Test(7);
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
            return shape;
        }
        void TFSCreateRollingOffset(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            Reference ref1 = uidoc.Selection.PickObject(ObjectType.PointOnElement, new TfsPointSelectionFilter(doc));
            Reference ref2 = uidoc.Selection.PickObject(ObjectType.PointOnElement, new TfsPointSelectionFilter(doc));
            ElementId assemId = doc.GetElement(ref1.ElementId).AssemblyInstanceId;
            XYZ c0 = null;
            XYZ c2 = null;
            double height = 0;
            double distance = ref1.GlobalPoint.DistanceTo(ref2.GlobalPoint);
            //if the user has picked two ends of the same pipe, just make a dimension. we can check for this by looking at the element associated with ref1 and ref2
            //if the user clicked a "planar offset" draw a triangle instead. might have to look at the orientation of a connected fitting (because pipe can be randomly rotated about its long axis)
            //else continue to create rolling offset box

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


            //calculate height for the box and the two botttom points of the rectangle
            if (ref1.GlobalPoint.Z < ref2.GlobalPoint.Z) //
            {
                c0 = ref1.GlobalPoint;
                c2 = new XYZ(ref2.GlobalPoint.X, ref2.GlobalPoint.Y, ref1.GlobalPoint.Z);
                height = ref2.GlobalPoint.Z - ref1.GlobalPoint.Z;
            }
            else
            {
                c0 = ref2.GlobalPoint;
                c2 = new XYZ(ref1.GlobalPoint.X, ref1.GlobalPoint.Y, ref2.GlobalPoint.Z);
                height = ref1.GlobalPoint.Z - ref2.GlobalPoint.Z;
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
            //create extrusion
            try
            {
                using (Transaction tr = new Transaction(doc))
                {
                    tr.Start("Rolling Offset");
                    //create extrusion
                    Solid ext = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { loop }, XYZ.BasisZ, height);
                    DirectShape shape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                    shape.SetShape(new List<GeometryObject>() { ext });
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetSurfaceTransparency(75);
                    LinePatternElement linePattern = LinePatternElement.GetLinePatternElementByName(doc, "Hidden 1/8\"");
                    ogs.SetProjectionLinePatternId(linePattern.Id);
                    FillPatternElement fillPattern = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "<Solid fill>");
                    ogs.SetSurfaceForegroundPatternId(fillPattern.Id);
                    uidoc.ActiveGraphicalView.SetElementOverrides(shape.Id, ogs);
                    doc.Regenerate();
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
                    if (assemId != ElementId.InvalidElementId)
                    {
                        AssemblyInstance spool = doc.GetElement(assemId) as AssemblyInstance;
                        spool.AddMemberIds(new List<ElementId>() { shape.Id });
                    }
                    //get faces
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return;
            }
        }
        void GetMaps()
        {
            FabricationNetworkChangeService fncs = new FabricationNetworkChangeService(Document);
            foreach (FabricationPartSizeMap map in fncs.GetMapOfAllSizesForStraights())
            {
                UI.Test($"map:{map.Depth},isprod:{map.IsMappedProductList}");
            }
        }
        static List<FabricationItemFile> fabricationItemFiles = new List<FabricationItemFile>();
        void AddItm()
        {
            UI.StartTimer();
            using (Transaction tr = new Transaction(Document))
            {
                tr.Start("Add Item");
                FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(Document);
                fc.ReloadConfiguration();
                List<FabricationItemFolder> folders = GetAllSubFolders(fc);
                int i = 0;
                FilteredElementCollector fec = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType();
                Level level = fec.FirstElement() as Level;
                foreach (FabricationItemFolder folder in folders)
                {
                    IEnumerable<FabricationItemFile> items = folder.GetItemFiles().Take(20); // Get the first 10 items
                    IList<FabricationItemFile> fail = fc.LoadItemFiles(items.ToList());
                    fabricationItemFiles.AddRange(items);
                    //foreach (FabricationItemFile item in items)
                    //{
                    //    try
                    //    {
                    //        FabricationPartType
                    //        //FabricationPart part = FabricationPart.Create(Document, item, level.Id);
                    //    }
                    //    catch
                    //    {
                    //    }
                    //}
                    i++;
                    if (i > 20)
                    {
                        break;
                    }
                }
                tr.Commit();
            }
            UI.Test($"time:{UI.StopTimer()}");
        }
        void AddItms()
        {
            UI.StartTimer();
            using (Transaction tr = new Transaction(Document))
            {
                tr.Start("Add Items");
                FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(Document);
                IList<FabricationItemFile> files = fc.GetAllLoadedItemFiles();
                FilteredElementCollector fec = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType();
                Level level = fec.FirstElement() as Level;
                foreach (string path in partDictionary.Values)
                {
                    try
                    {
                        FabricationItemFile item = files.FirstOrDefault(f => f.Identifier == path);
                        FabricationPart part = FabricationPart.Create(Document, item, level.Id);
                        UI.Test($"part:{part.GetTypeId()},{item.Identifier}");
                    }
                    catch
                    {
                    }
                }
                tr.Commit();
            }
            UI.Test($"time:{UI.StopTimer()}");
        }
        void GetPipeItmPaths()
        {
            FilteredElementCollector fec = new FilteredElementCollector(Document).OfClass(typeof(FabricationPart));
            foreach (FabricationPart part in fec)
            {
                if (partDictionary.TryGetValue(part.GetTypeId(), out string path))
                {
                    UI.Test(2);
                    if (part.IsAStraight())
                    {
                        UI.Test(3);
                        using (Transaction tr = new Transaction(Document))
                        {
                            tr.Start("Add Item");
                            FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(Document);
                            var files = fc.GetAllLoadedItemFiles();
                            FabricationItemFile item = files.FirstOrDefault(f => f.Identifier == path);
                            FabricationPart newPart = FabricationPart.Create(Document, item, part.LevelId);
                            newPart.SetPositionByEnd(newPart.GetConnectors().First(), new XYZ(10, 0, 0));
                            tr.Commit();
                        }
                        break;
                    }
                }
            }
        }
        static Dictionary<ElementId, string> partDictionary = new Dictionary<ElementId, string>();
        void GetPartDictionary()
        {
            int i = 0;
            using (Transaction tr = new Transaction(Document, "Add Item"))
            {
                tr.Start("reload");
                FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(Document);
                fc.ReloadConfiguration();
                List<FabricationItemFolder> folders = GetAllSubFolders(fc);
                //UI.Test(10);
                foreach (FabricationItemFolder folder in folders)
                {
                    //UI.Test(11);
                    foreach (FabricationItemFile file in folder.GetItemFiles())
                    {
                        //UI.Test(12);
                        IList<FabricationItemFile> fail = fc.LoadItemFiles([file]);
                        if (fail.Count > 0)
                        {
                            UI.Test($"fail:{fail.Count}");
                        }
                        FilteredElementCollector fec = new FilteredElementCollector(Document).OfClass(typeof(FabricationPartType));
                        //get fec element that has the newest elementid
                        FabricationPartType type = fec.Cast<FabricationPartType>().OrderByDescending(e => e.Id.Value).First();
                        //UI.Test($"type:{type.Id},name:{type.FamilyName}");
                        if (!partDictionary.ContainsKey(type.Id))
                        {
                            partDictionary.Add(type.Id, file.Identifier);
                        }
                    }
                    i++;
                    if (i > 20)
                    {
                        break;
                    }
                }
                tr.Commit();
            }
        }
        List<FabricationItemFolder> GetAllSubFolders(FabricationConfiguration fc)
        {
            return fc.GetItemFolders().SelectMany(f => GetAllSubFolders(f)).ToList();
        }
        List<FabricationItemFolder> GetAllSubFolders(FabricationItemFolder parent)
        {
            List<FabricationItemFolder> folders = new List<FabricationItemFolder>();
            foreach (FabricationItemFolder folder in parent.GetSubFolders())
            {
                folders.Add(folder);
                folders.AddRange(GetAllSubFolders(folder));
            }
            return folders;
        }
        void UpdateDimension()
        {
            Reference r = UiDocument.Selection.PickObject(ObjectType.Element, new FabricationPartSelectionFilter());
            FabricationPart part = Document.GetElement(r.ElementId) as FabricationPart;
            using (Transaction tr = new Transaction(Document))
            {
                tr.Start("Update Dimension");
                FabricationHostedInfo info = part.GetHostedInfo();
                if (info != null)
                {
                    info.DisconnectFromHost();
                }
                FabricationRodInfo rodInfo = part.GetRodInfo();
                if (rodInfo != null)
                {
                    rodInfo.CanRodsBeHosted = false;
                }
                Line center = info.GetBearerCenterline();
                //get midpoint between two xyz
                XYZ mid = center.GetEndPoint(0).Add(center.GetEndPoint(1)).Multiply(0.5);
                FabricationDimensionDefinition dim = part.GetDimensions().First(dim => dim.Name == "Width");
                UI.Test($"dim:{part.GetDimensionValue(dim).ToInches()}");
                part.SetDimensionValue(dim, 1.0);
                UI.Test($"dim2:{part.GetDimensionValue(dim).ToInches()}");
                foreach (FabricationDimensionDefinition d in part.GetDimensions())
                {
                    UI.Test($"{d.Name}:{part.GetDimensionValue(d).ToInches()}");
                }
                tr.Commit();
            }

        }
        void AddIconToTab()
        {
            Button tab = MainWindow.getMainWnd().FindChildrenByType<Button>()
                .FirstOrDefault(b => b.Name == "mRibbonTabButton" && AutomationProperties.GetName(b) == "CODE");
            tab.Content = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/CODE.Free;component/Resources/Icons/CODE32.png")),
                Width = 16,
                Height = 16
            };
        }
        void CustomizeTabColor()
        {
            Style style = new Style();
            style.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Colors.Red)));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            Button tab = MainWindow.getMainWnd().FindChildrenByType<Button>()
                .FirstOrDefault(b => b.Name == "mRibbonTabButton" && AutomationProperties.GetName(b) == "CODE");
            tab.Style = style;
        }
        void AddCheckBoxToTab()
        {
            Button tab = MainWindow.getMainWnd().FindChildrenByType<Button>()
                .FirstOrDefault(b => b.Name == "mRibbonTabButton" && AutomationProperties.GetName(b) == "CODE");
            tab.Content = new CheckBox()
            {
                Content = new System.Windows.Controls.TextBox()
                {
                    Text = "CODE",
                    Foreground = new SolidColorBrush(Colors.Orange),
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                },
            };
        }
        void ShowReferenceType()
        {
            bool result = false;
            do
            {
                try
                {
                    Reference ref1 = UiDocument.Selection.PickObject(ObjectType.PointOnElement, new FabricationPartSelectionFilter());
                    result = ref1 != null;
                    UI.Test($"Reference type:{ref1?.ElementReferenceType}");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    break;
                }
            } while (result);
        }
        void TryCallExportPCF()
        {
            UIFrameworkServices.CommandHandlerService.invokeCommandHandler("ID_EXPORT_FABRICATION_PCF");
        }
        void LoadCadmepServices()
        {
            //doesn't work, didn't load fabapi in first attempt
            Assembly assem = Assembly.LoadFrom(@"C:\Program Files\Autodesk\Fabrication 2021\CADmep\FabricationAPI.dll");
            UI.Popup($"assem null:{assem == null}");
            if (assem != null)
            {
                UI.Popup($"services: {Autodesk.Fabrication.DB.Database.Services?.Count}");
            }
        }
        void CreateRollingOffset()
        {
            Reference ref1 = UiDocument.Selection.PickObject(ObjectType.PointOnElement, new PointSelectionFilter(Document));
            Reference ref2 = UiDocument.Selection.PickObject(ObjectType.PointOnElement, new PointSelectionFilter(Document));
            ElementId assemId = Document.GetElement(ref1.ElementId).AssemblyInstanceId;
            XYZ c0 = null;
            XYZ c2 = null;
            double height = 0;
            double distance = ref1.GlobalPoint.DistanceTo(ref2.GlobalPoint);
            if (ref1.GlobalPoint.Z < ref2.GlobalPoint.Z)
            {
                c0 = ref1.GlobalPoint;
                c2 = new XYZ(ref2.GlobalPoint.X, ref2.GlobalPoint.Y, ref1.GlobalPoint.Z);
                height = ref2.GlobalPoint.Z - ref1.GlobalPoint.Z;
            }
            else
            {
                c0 = ref2.GlobalPoint;
                c2 = new XYZ(ref1.GlobalPoint.X, ref1.GlobalPoint.Y, ref2.GlobalPoint.Z);
                height = ref1.GlobalPoint.Z - ref2.GlobalPoint.Z;
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
            //create extrusion
            try
            {
                using (Transaction tr = new Transaction(Document))
                {
                    tr.Start("Rolling Offset");
                    //create extrusion
                    Solid ext = GeometryCreationUtilities.CreateExtrusionGeometry([loop], XYZ.BasisZ, height);
                    DirectShape shape = DirectShape.CreateElement(Document, new ElementId(BuiltInCategory.OST_GenericModel));
                    shape.SetShape([ext]);
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetSurfaceTransparency(75);
                    LinePatternElement linePattern = LinePatternElement.GetLinePatternElementByName(Document, "Hidden 1/8\"");
                    ogs.SetProjectionLinePatternId(linePattern.Id);
                    FillPatternElement fillPattern = FillPatternElement.GetFillPatternElementByName(Document, FillPatternTarget.Drafting, "<Solid fill>");
                    ogs.SetSurfaceForegroundPatternId(fillPattern.Id);
                    UiDocument.ActiveGraphicalView.SetElementOverrides(shape.Id, ogs);
                    Document.Regenerate();
                    //get geometry from extrusion
                    GeometryElement geoElem = shape.get_Geometry(new Options() { ComputeReferences = true });
                    //get geometry instance from geometry element
                    Solid solid = geoElem.First() as Solid;
                    PlanarFace lowest = GetLowestFace(solid);
                    PlanarFace furthest = GetFurthestFaceFromView(solid, UiDocument.ActiveGraphicalView);
                    List<Edge> edges = SortEdgesByNearestFromView(lowest, UiDocument.ActiveGraphicalView);
                    UiDocument.ActiveGraphicalView.SketchPlane = SketchPlane.Create(Document, lowest.Reference);
                    DimensionEdges(edges[0], lowest.FaceNormal, distance, UiDocument.ActiveGraphicalView);
                    DimensionEdges(edges[1], lowest.FaceNormal, distance, UiDocument.ActiveGraphicalView);
                    List<Edge> sideEdges = SortEdgesByNearestFromView(furthest, UiDocument.ActiveGraphicalView);
                    UiDocument.ActiveGraphicalView.SketchPlane = SketchPlane.Create(Document, furthest.Reference);
                    Edge sideEdge = sideEdges.FirstOrDefault(e => (e.AsCurve() as Line).Direction.IsVertical()) ?? sideEdges[0];
                    DimensionEdges(sideEdge, furthest.FaceNormal, distance, UiDocument.ActiveGraphicalView);
                    if (assemId != ElementId.InvalidElementId)
                    {
                        AssemblyInstance spool = Document.GetElement(assemId) as AssemblyInstance;
                        spool.AddMemberIds(new List<ElementId>() { shape.Id });
                    }
                    //get faces
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
                return;
            }
        }
        //remember to get lowest face for xy dims
        //get furthest vertical face for z dim
        PlanarFace GetLowestFace(Solid solid)
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
        PlanarFace GetFurthestFaceFromView(Solid solid, View view)
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
        List<Edge> SortEdgesByNearestFromView(PlanarFace face, View view)
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
        void DimensionEdges(Edge edge, XYZ normal, double offset, View view)
        {
            ReferenceArray referenceArray = new ReferenceArray();
            referenceArray.Append(edge.GetEndPointReference(0));
            referenceArray.Append(edge.GetEndPointReference(1));
            Line line3 = edge.AsCurve() as Line;
            XYZ cp = line3.Direction.CrossProduct(normal);
            double factor = line3.Origin.Add(cp).DistanceTo(view.Origin) > line3.Origin.DistanceTo(view.Origin) ? -1 : 1;
            line3 = line3.CreateTransformed(Transform.CreateTranslation(cp * offset * factor)) as Line;
            Dimension dim = Document.Create.NewDimension(view, line3, referenceArray);
            dim.Below = "C - C";
        }
        public class TfsPointSelectionFilter : ISelectionFilter
        {
            public Document Document { get; set; }
            public TfsPointSelectionFilter(Document doc)
            {
                Document = doc;
            }
            public bool AllowElement(Element elem)
            {
                return
                    elem is FabricationPart fp &&
                    fp.ServiceType != 57;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                /* //select only points
                GeometryElement geo = Document.GetElement(reference).get_Geometry(new Options() { ComputeReferences = true, IncludeNonVisibleObjects = true });
                GeometryInstance geoInstance = geo.First() as GeometryInstance;
                IEnumerable<Point> points = geoInstance.GetInstanceGeometry().Where(s => s is Point).Cast<Point>();
                return reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_NONE && points.Any(p => p.Coord.IsAlmostEqualTo(position));
                */

                //select only endpoints and midpoints of fittings
                Element elem = Document.GetElement(reference);
                if (elem is FabricationPart fabricationPart)
                {
                    // Get the connector manager from the fabrication part.
                    ConnectorManager connectorManager = fabricationPart.ConnectorManager;
                    if (connectorManager != null)
                    {
                        //compare to midpoint if it's an elbow
                        if (!fabricationPart.IsAStraight() && fabricationPart.CalculateMidpoint().IsAlmostEqualTo(position))
                        {
                            return true;
                        }
                        else
                        {
                            //find unconnected connectors in the mananger
                            var unusedConnectors = connectorManager.UnusedConnectors;
                            foreach (Connector unusedConnector in unusedConnectors)
                            {
                                // Check if the clicked position is almost equal to the unused connector's origin.
                                if (unusedConnector.Origin.IsAlmostEqualTo(position))
                                {
                                    return true;
                                }
                            }
                            // Loop through each connector in the manager.
                            foreach (Connector connector in connectorManager.Connectors)
                            {
                                //check if the connector has another part connected to it from a different assembly
                                //for each connector, find all refs (connectors attached to it), check their owner assembly name against this connector's owner assembly name
                                //gather info about this connector, like the spool name
                                var thisOwnerId = connector.Owner.Id;
                                var thisSpoolId = Document.GetElement(thisOwnerId).AssemblyInstanceId;
                                //find the connected parts
                                var connectedRefs = connector.AllRefs;
                                var connectorCount = connectedRefs.Size;
                                foreach (Connector otherConnector in connectedRefs)
                                {
                                    var otherOwnerId = otherConnector.Owner.Id;
                                    FabricationPart otherPart = Document.GetElement(otherOwnerId) as FabricationPart;

                                    var otherSpoolId = otherPart.AssemblyInstanceId;
                                    //if the connected part is from a different assembly, accept it
                                    if (thisSpoolId != otherSpoolId || otherPart.ServiceType == 57)
                                    {
                                        if (connector.AllRefs.Size > 0 && connector.Origin.IsAlmostEqualTo(position))
                                        {
                                            return true;
                                        }
                                    }
                                }


                            }
                        }


                    }
                }
                return false;
            }
        }
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
        public class PointSelectionFilter : ISelectionFilter
        {
            public Document Document { get; set; }
            public PointSelectionFilter(Document doc)
            {
                Document = doc;
            }
            public bool AllowElement(Element elem)
            {
                return true;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                GeometryElement geo = Document.GetElement(reference).get_Geometry(new Options() { ComputeReferences = true, IncludeNonVisibleObjects = true });
                GeometryInstance geoInstance = geo.First() as GeometryInstance;
                IEnumerable<Point> points = geoInstance.GetInstanceGeometry().Where(s => s is Point).Cast<Point>();
                return reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_NONE && points.Any(p => p.Coord.IsAlmostEqualTo(position));
            }
        }
        void CreateDim2()
        {
            //while (PickObject(ObjectType.PointOnElement _pickPrompt, ref r))
            //{

            //}
            //get user to click two references
            var refs = UiDocument.Selection.PickObjects(ObjectType.PointOnElement);
            //create a plane from the two references
            try
            {

                //create a sketch plane from the plane
                //TaskDialog.Show("x", "1");
                using (Transaction tr = new Transaction(Document))
                {
                    tr.Start("dim");
                    //create new dimension
                    ReferenceArray referenceArray = new ReferenceArray();
                    foreach (Reference r in refs)
                    {
                        referenceArray.Append(r);
                    }
                    Line line = Line.CreateBound(refs[0].GlobalPoint, refs[1].GlobalPoint);
                    Document.Create.NewDimension(UiDocument.ActiveGraphicalView, line, referenceArray);
                    tr.Commit();
                }
                //wow github, great job
            }
            catch (Exception ex)
            {
                TaskDialog.Show("x", ex.Message);
                return;
            }
        }
        void CreateDim3()
        {
            //get user to click two references
            //linear for centerline, none for points, surface for connector face
            Reference ref1 = UiDocument.Selection.PickObject(ObjectType.PointOnElement);
            Reference ref2 = UiDocument.Selection.PickObject(ObjectType.PointOnElement);
            TaskDialog.Show("x", $"ref1:{ref1.ElementReferenceType}, ref2:{ref2.ElementReferenceType}");
            //create a plane from the two references
            try
            {

                Plane plane = Plane.CreateByThreePoints(ref1.GlobalPoint, ref2.GlobalPoint, ref1.GlobalPoint + XYZ.BasisZ);
                //create a sketch plane from the plane
                using (Transaction tr = new Transaction(Document))
                {
                    tr.Start("dim");
                    SketchPlane sp = SketchPlane.Create(Document, plane);
                    //set the active sketch plane to the sketch plane
                    UiDocument.ActiveGraphicalView.SketchPlane = sp;
                    //postcommand dimension
                    tr.Commit();
                }
                UiApplication.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.AlignedDimension));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("x", ex.Message);
                return;
            }
        }

        void ParsePcf()
        {
            var parser = new PcfParser();
            parser.Parse($@"B:\01-CO\02-Addins\01-CODE\02-Published\CODE.Free\source\SearchFabServicesDialog\Resources\AC-CIPR-DS38U01501-02.pcf");

            FilteredElementCollector fec = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType();
            Level level = fec.FirstElement() as Level;
            foreach (var component in parser.Components)
            {
                //UI.Test(component.ToString());
                //try to find an item in the loaded services/items that matches the component details and create it
            }
        }
        public class PcfParser
        {
            public HashSet<Component> Components { get; private set; } = new HashSet<Component>();

            public void Parse(string filePath)
            {
                var lines = File.ReadAllLines(filePath);
                Component currentComponent = null;

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                        continue;

                    if (IsComponentStart(parts[0]))
                    {
                        if (currentComponent != null)
                        {
                            Components.Add(currentComponent);
                        }
                        currentComponent = new Component { Type = parts[0] };
                    }
                    else if (currentComponent != null)
                    {
                        ParseComponentAttribute(currentComponent, line.Trim());
                    }
                }

                if (currentComponent != null)
                {
                    Components.Add(currentComponent);
                }
            }

            private void ParseComponentAttribute(Component component, string attribute)
            {
                var parts = attribute.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return;

                switch (parts[0])
                {
                    case "COMPONENT-IDENTIFIER":
                        component.ComponentIdentifier = int.Parse(parts[1]);
                        break;
                    case "SKEY":
                        component.Skey = parts[1];
                        break;
                    case "MATERIAL-IDENTIFIER":
                        component.MaterialIdentifier = int.Parse(parts[1]);
                        break;
                    case "ANGLE":
                        component.Angle = double.Parse(parts[1]);
                        break;
                    case "INSULATION-SPEC":
                        component.InsulationSpec = parts[1];
                        break;
                    case "INSULATION":
                        component.InsulationOn = parts[1] == "ON";
                        break;
                    case "PIPING-SPEC":
                        component.PipingSpec = parts[1];
                        break;
                    case "FABRICATION-ITEM":
                        component.FabricationItem = true;
                        break;
                    case "WELD-ATTRIBUTE1":
                        component.WeldAttribute1 = parts[1];
                        break;
                    case "WELD-ATTRIBUTE2":
                        component.WeldAttribute2 = double.Parse(parts[1]);
                        break;
                    case "WELD-ATTRIBUTE3":
                        component.WeldAttribute3 = parts[1];
                        break;
                    case "WELD-ATTRIBUTE4":
                        component.WeldAttribute4 = parts[1];
                        break;
                    case "WEIGHT":
                        component.Weight = double.Parse(parts[1]);
                        break;
                    case "CUT-PIECE-LENGTH":
                        component.CutPieceLength = double.Parse(parts[1]);
                        break;
                    //add cases for end-point and centre-point
                    case "END-POINT":
                        XYZ point = GetPoint(parts);
                        if (point != null)
                        {
                            component.EndPoints.Add(point);
                        }
                        break;
                    case "CENTRE-POINT":
                        XYZ point2 = GetPoint(parts);
                        if (point2 != null)
                        {
                            component.CentrePoints.Add(point2);
                        }
                        break;
                    case "BRANCH1-POINT":
                        XYZ point3 = GetPoint(parts);
                        if (point3 != null)
                        {
                            component.Branch1Points.Add(point3);
                        }
                        break;
                    default:
                        break;
                }
            }

            private bool IsComponentStart(string part)
            {
                return part == "ELBOW" || part == "WELD" || part == "PIPE" || part == "SUPPORT" || part == "INSTRUMENT-3WAY" || part == "FLOW-ARROW" || part == "REFERENCE-DIMENSION" || part == "END-CONNECTION-PIPELINE" || part == "MATERIALS";
            }

            private XYZ GetPoint(string[] parts)
            {
                if (parts.Length >= 4 && double.TryParse(parts[1], out double x) && double.TryParse(parts[2], out double y) && double.TryParse(parts[3], out double z))
                {
                    return new XYZ(x, y, z);
                }
                return null;
            }
        }
        public class Component
        {
            public string Type { get; set; } = string.Empty;
            public int ComponentIdentifier { get; set; } = 0;
            public string Skey { get; set; } = string.Empty;
            public int MaterialIdentifier { get; set; } = 0;
            public double Angle { get; set; } = 0.0;
            public string InsulationSpec { get; set; } = string.Empty;
            public bool InsulationOn { get; set; } = false;
            public string PipingSpec { get; set; } = string.Empty;
            public bool FabricationItem { get; set; } = false;
            public string WeldAttribute1 { get; set; } = string.Empty;
            public double WeldAttribute2 { get; set; } = 0.0;
            public string WeldAttribute3 { get; set; } = string.Empty;
            public string WeldAttribute4 { get; set; } = string.Empty;
            public double Weight { get; set; } = 0.0;
            public double CutPieceLength { get; set; } = 0.0;
            public List<XYZ> EndPoints { get; set; } = new List<XYZ>();
            public List<XYZ> CentrePoints { get; set; } = new List<XYZ>();
            public List<XYZ> Branch1Points { get; set; } = new List<XYZ>();

            //override ToString() output the component properties and value types
            public override string ToString()
            {
                return
                    $"Type (string): {Type}\n" +
                    $"ComponentIdentifier (int): {ComponentIdentifier}\n" +
                    $"Skey (string): {Skey}\n" +
                    $"MaterialIdentifier (int): {MaterialIdentifier}\n" +
                    $"Angle (double): {Angle}\n" +
                    $"InsulationSpec (string): {InsulationSpec}\n" +
                    $"InsulationOn (bool): {InsulationOn}\n" +
                    $"PipingSpec (string): {PipingSpec}\n" +
                    $"FabricationItem (bool): {FabricationItem}\n" +
                    $"WeldAttribute1 (string): {WeldAttribute1}\n" +
                    $"WeldAttribute2 (int): {WeldAttribute2}\n" +
                    $"WeldAttribute3 (string): {WeldAttribute3}\n" +
                    $"WeldAttribute4 (string): {WeldAttribute4}\n" +
                    $"Weight (double): {Weight}\n" +
                    $"CutPieceLength (double): {CutPieceLength}\n" +
                    $"EndPoints (List<XYZ>):\n{string.Join("\n", EndPoints)}" +
                    $"CentrePoints (List<XYZ>):\n{string.Join("\n", CentrePoints)}" +
                    $"Branch1Points (List<XYZ>):\n{string.Join("\n", Branch1Points)}";
            }
        }
    }
    //selection filter for fabrication parts
    public class FabricationPartSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FabricationPart;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
    //static class Store
    //{
    //    public class Revit_Addins
    //    {
    //        /// <summary>
    //        /// The Iso Dimension command for Revit 2021-2025.
    //        /// Try it free for 7 days. Just download the installer.
    //        /// Buy it for $25.00 per user.
    //        /// </summary>
    //        public class IsoDimension
    //        {
    //            public static string About()
    //            {
    //                return
    //                    "This command allows the user to pick from multiple planes along pipes and fittings to create isometric dimensions in 3D views.";
    //            }
    //            private static IsoDimension DownloadInstaller();
    //            private static IsoDimension Buy();
    //        }

    //        /// <summary>
    //        /// The Export NWC command for Revit 2021-2025.
    //        /// Try it free for 7 days. Just download the installer.
    //        /// Buy it for $60.00 per user.
    //        /// </summary>
    //        public class ExportNWC
    //        {
    //            public static string About()
    //            {
    //                return
    //                    "This command allows the user to export colors of Fabrication parts to Navisworks.";
    //            }
    //            private static ExportNWC DownloadInstaller();
    //            private static ExportNWC Buy();
    //        }
    //    }
    //    public class AutoCAD_Addins
    //    {
    //        /// <summary>
    //        /// The Size Up and Size Down commands for Fabrication CADmep 2021-2025.
    //        /// Free forever. Just download the installer.
    //        /// </summary>
    //        public class SizeUpDown
    //        {
    //            public static string About()
    //            {
    //                return
    //                    "This pair of commands allows users to quickly adjust sizing of selected ITMs.";
    //            }
    //            private static SizeUpDown DownloadInstaller();
    //        }
    //        /// <summary>
    //        /// The Database Manager command for Fabrication CADmep 2021-2025.
    //        /// Try it free for 7 days. Just download the installer.
    //        /// Buy it for $1,750.00 per user.
    //        /// Or hire our team to update your database for $500.00 per request.
    //        /// </summary>
    //        public class DatabaseManager
    //        {
    //            public static string About()
    //            {
    //                return
    //                    "This command opens a dialog with many controls for easy editing of the database settings and ITMs.";
    //            }
    //            private static DatabaseManager DownloadInstaller();
    //            private static DatabaseManager Buy();
    //        }
    //    }
    //}
    public static class TestExtensions
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
