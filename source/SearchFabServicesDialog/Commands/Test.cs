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

                //DrawFace2();
                // DirectShape shape = DrawFace();
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
                    if (part.IsAStraight())
                    {
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
}
