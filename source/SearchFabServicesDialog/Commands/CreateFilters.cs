using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using CODE.Free.Utils;
using Nice3point.Revit.Toolkit.External;
using System.Reflection;
#if (REVIT2025)
                        //2025
#else
#endif
namespace CODE.Free
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CreateFabFiltersWithLines : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            if (CreateFiltersExtensions.IsValidView(UiDocument.ActiveGraphicalView, "Create Service Filters With Lines") &&
                CreateFiltersExtensions.CanCreateFabFilters(Document) &&
                CreateFiltersExtensions.CanGetLinePatternId(Document))
            {
                bool separateInsul = CreateFiltersExtensions.PromptSeparateInsulation();
                CreateFiltersExtensions.CreateFabFilters(UiDocument.ActiveGraphicalView, true, separateInsul);
            }
        }
    }
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CreateFabFiltersWithSurfaces : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            if (CreateFiltersExtensions.IsValidView(UiDocument.ActiveGraphicalView, "Create Service Filters With Surfaces") &&
                CreateFiltersExtensions.CanCreateFabFilters(Document) &&
                CreateFiltersExtensions.CanGetFillPatternId(Document))
            {
                bool separateInsul = CreateFiltersExtensions.PromptSeparateInsulation();
                CreateFiltersExtensions.CreateFabFilters(UiDocument.ActiveGraphicalView, false, separateInsul);
            }
        }
    }
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CreateSystemFiltersWithLines : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            if (CreateFiltersExtensions.IsValidView(UiDocument.ActiveGraphicalView, "Create System Filters With Lines") &&
                CreateFiltersExtensions.CanCreateSystemFilters(Document) &&
                CreateFiltersExtensions.CanGetLinePatternId(Document))
            {
                bool separateInsul = CreateFiltersExtensions.PromptSeparateInsulation();
                CreateFiltersExtensions.CreateSystemFilters(UiDocument.ActiveGraphicalView, true, separateInsul);
            }
        }
    }
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CreateSystemFiltersWithSurfaces : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            if (CreateFiltersExtensions.IsValidView(UiDocument.ActiveGraphicalView, "Create System Filters With Surfaces") &&
                CreateFiltersExtensions.CanCreateSystemFilters(Document) &&
                CreateFiltersExtensions.CanGetFillPatternId(Document))
            {
                bool separateInsul = CreateFiltersExtensions.PromptSeparateInsulation();
                CreateFiltersExtensions.CreateSystemFilters(UiDocument.ActiveGraphicalView, false, separateInsul);
            }
        }
    }
    public class CreateFiltersAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication app, CategorySet selectedCategories)
        {
            return !app.ActiveUIDocument.Document.IsFamilyDocument;
        }
    }
    static class CreateFiltersExtensions
    {
        static string _TemplateName = "CODE - Fabrication Services";
        static string _Title = "Create Filters";
        static ElementId _linePatternId = ElementId.InvalidElementId;
        static ElementId _fillPatternId = ElementId.InvalidElementId;
        static ICollection<ElementId> _FabCategories
        {
            get
            {
                return new List<ElementId>()
                {
                    new ElementId(BuiltInCategory.OST_FabricationContainment),
                    new ElementId(BuiltInCategory.OST_FabricationPipework),
                    new ElementId(BuiltInCategory.OST_FabricationDuctwork),
                    new ElementId(BuiltInCategory.OST_FabricationHangers),
                };
            }
        }
        static ICollection<ElementId> _FabInsulCategories
        {
            get
            {
                return new List<ElementId>()
                {
                    new ElementId(BuiltInCategory.OST_FabricationPipeworkInsulation),
                    new ElementId(BuiltInCategory.OST_FabricationDuctworkInsulation),
                    new ElementId(BuiltInCategory.OST_FabricationDuctworkLining),
                };
            }
        }
        static ICollection<ElementId> _RevitMEPCategories
        {
            get
            {
                return new List<ElementId>()
                {
                    //equipment
                    //new ElementId(BuiltInCategory.OST_MechanicalEquipment),
                    //new ElementId(BuiltInCategory.OST_PlumbingFixtures),
                    //new ElementId(BuiltInCategory.OST_LightingFixtures),
                    //new ElementId(BuiltInCategory.OST_ElectricalEquipment),
                    //new ElementId(BuiltInCategory.OST_PlumbingEquipment),
                    //new ElementId(BuiltInCategory.OST_SpecialityEquipment),

                    //duct
                    new ElementId(BuiltInCategory.OST_DuctAccessory),
                    new ElementId(BuiltInCategory.OST_DuctCurves),
                    new ElementId(BuiltInCategory.OST_DuctFitting),
                    //new ElementId(BuiltInCategory.OST_DuctTerminal),

                    //pipe
                    new ElementId(BuiltInCategory.OST_PipeAccessory),
                    new ElementId(BuiltInCategory.OST_PipeCurves),
                    new ElementId(BuiltInCategory.OST_PipeFitting),

                    //electrical
                    //new ElementId(BuiltInCategory.OST_ElectricalEquipment),
                    //new ElementId(BuiltInCategory.OST_ElectricalFixtures),
                    //new ElementId(BuiltInCategory.OST_CableTray),
                    //new ElementId(BuiltInCategory.OST_CableTrayFitting),
                    //new ElementId(BuiltInCategory.OST_CableTrayRun),
                };
            }
        }
        static ICollection<ElementId> _RevitMEPInsulCategories
        {
            get
            {
                return new List<ElementId>()
                {
                    //duct
                    //new ElementId(BuiltInCategory.OST_DuctCurvesInsulation),
                    //new ElementId(BuiltInCategory.OST_DuctCurvesLining),
                    //new ElementId(BuiltInCategory.OST_DuctFittingInsulation),
                    //new ElementId(BuiltInCategory.OST_DuctFittingLining),
                    new ElementId(BuiltInCategory.OST_DuctInsulations),
                    //new ElementId(BuiltInCategory.OST_DuctLinings),

                    //pipe
                    //new ElementId(BuiltInCategory.OST_PipeCurvesInsulation),
                    //new ElementId(BuiltInCategory.OST_PipeFittingInsulation),
                    new ElementId(BuiltInCategory.OST_PipeInsulations),
                };
            }
        }
        static List<System.Drawing.Color> _Colors = new List<System.Drawing.Color>();
        static ElementId _ServiceParamId = new ElementId(BuiltInParameter.FABRICATION_SERVICE_PARAM);
        static ElementId _SystemNameParamId = new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
        //static ElementId _SystemClassParamId = new ElementId(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
        static void GetColors()
        {
            if (_Colors == null || _Colors.Count == 0)
            {
                _Colors = new List<System.Drawing.Color>();
                foreach (PropertyInfo propInfo in typeof(System.Drawing.Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
                {
                    _Colors.Add((System.Drawing.Color)propInfo.GetValue(null));
                }
                _Colors.RemoveAll(color => color.IsSystemColor || color.IsColorTooLightOrDark());
            }
        }
        public static bool PromptSeparateInsulation()
        {
            TaskDialog taskDialog = new TaskDialog(_Title);
            taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            taskDialog.MainInstruction = "Do you want to create separate filters for insulation/lining?";
            TaskDialogResult result = taskDialog.Show();
            return result == TaskDialogResult.Yes;
        }
        public static bool IsValidView(View activeView, string title)
        {
            _Title = title;
            if (activeView is not View3D &&
                activeView is not ViewSection &&
                activeView is not ViewPlan)
            {
                UI.Popup(_Title, "Please open a model view before running this command.");
                return false;
            }
            return true;
        }
        public static bool CanCreateFabFilters(Document doc)
        {
            FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(doc);
            if (fc == null || fc.GetAllLoadedServices().Count == 0)
            {
                UI.Popup(_Title, "No services loaded in the project. Please load services and try again.");
                return false;
            }
            return true;
        }
        public static bool CanCreateSystemFilters(Document doc)
        {
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(MEPSystem)).WhereElementIsNotElementType();
            if (fec.GetElementCount() == 0)
            {
                UI.Popup(_Title, "No MEP systems found in the project. Please create MEP systems and try again.");
                return false;
            }
            return true;
        }
        public static bool CanGetLinePatternId(Document doc)
        {
            _linePatternId = LinePatternElement.GetSolidPatternId();
            if (_linePatternId == ElementId.InvalidElementId)
            {
                UI.Popup(_Title, "Solid line pattern not found in the project. Please load the pattern and try again.");
                return false;
            }
            return true;
        }
        public static bool CanGetFillPatternId(Document doc)
        {
            _fillPatternId = doc.GetSolidFillPattern();
            if (_fillPatternId == ElementId.InvalidElementId)
            {
                UI.Popup(_Title, "Solid fill pattern not found in the project. Please load the pattern and try again.");
                return false;
            }
            return true;
        }
        public static void CreateFabFilters(View activeView, bool createLineFilters, bool separateInsul)
        {
            UI.StartTimer();
            GetColors();
            int i = 0;
            int cnt = 0;
            Document doc = activeView.Document;
            ElementId patternId = createLineFilters ? _linePatternId : _fillPatternId;
            using (Transaction tr = new Transaction(doc, _Title))
            {
                tr.Start();
                FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(View));
                View template = fec.Cast<View>().FirstOrDefault(x => x.IsTemplate && x.Name == _TemplateName);
                if (template == null)
                {
                    template = activeView.CreateViewTemplate();
                    template.Name = _TemplateName;
                }
                FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(doc);
                foreach (FabricationService service in fc.GetAllLoadedServices())
                {
                    string serviceName = service.Name.Replace(": ", " - ").LegalizeString();
                    FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(_ServiceParamId, service.ServiceId);
                    ElementFilter elementFilter = new ElementParameterFilter(rule);
                    try
                    {
                        System.Drawing.Color sCol = _Colors[i];
                        Color color = new Color(sCol.R, sCol.G, sCol.B);
                        ParameterFilterElement pfe = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).FirstOrDefault(x => x.Name == serviceName) as ParameterFilterElement;
                        if (pfe == null)
                        {
                            pfe = ParameterFilterElement.Create(
                                    doc,
                                    serviceName,
                                    separateInsul ?
                                    _FabCategories :
                                    _FabCategories.Concat(_FabInsulCategories).ToList(),
                                    elementFilter);
                            UI.Test(12);
                        }
                        else
                        {
                            UI.Test(13);
                            pfe.SetCategories(
                                separateInsul ?
                                _FabCategories :
                                _FabCategories.Concat(_FabInsulCategories).ToList());
                            pfe.SetElementFilter(elementFilter);
                            UI.Test(414);
                        }
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        if (createLineFilters)
                        {
                            ogs.SetProjectionLinePatternId(patternId);
                            ogs.SetProjectionLineColor(color);
                        }
                        else
                        {
                            ogs.SetSurfaceForegroundPatternId(patternId);
                            ogs.SetSurfaceForegroundPatternColor(color);
                        }
                        template.SetFilterOverrides(pfe.Id, ogs);
                        cnt++;
                        if (separateInsul)
                        {
                            ParameterFilterElement pfeInsul = ParameterFilterElement.Create(doc, $"{serviceName} - Insulation", _FabInsulCategories, elementFilter);
                            OverrideGraphicSettings ogsInsul = new OverrideGraphicSettings(ogs);
                            ogsInsul.SetSurfaceTransparency(50);
                            template.SetFilterOverrides(pfeInsul.Id, ogsInsul);
                            cnt++;
                        }
                        i++;
                        if (i >= _Colors.Count)
                        {
                            i = 0;
                        }
                    }
                    catch { }
                }
                activeView.ApplyViewTemplateParameters(template);
                tr.Commit();
            }
            UI.Popup(_Title, $"{cnt} filters and graphics overrides created for the loaded services. Time elapsed: {UI.StopTimer()}");
        }
        public static void CreateSystemFilters(View activeView, bool createLineFilters, bool separateInsul)
        {
            UI.StartTimer();
            GetColors();
            int i = 0;
            int cnt = 0;
            Document doc = activeView.Document;
            ElementId patternId = createLineFilters ? _linePatternId : _fillPatternId;
            using (Transaction tr = new Transaction(doc, _Title))
            {
                tr.Start();
                FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(View));
                View template = fec.Cast<View>().FirstOrDefault(x => x.IsTemplate && x.Name == _TemplateName);
                if (template == null)
                {
                    template = activeView.CreateViewTemplate();
                    template.Name = _TemplateName;
                }
                FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(MEPSystem)).WhereElementIsNotElementType();
                List<Element> systems = col.OrderBy(x => x.Name).ToList();
                foreach (Element elem in systems)
                {
                    MEPSystem sys = elem as MEPSystem;
                    string sysName = sys.Name.LegalizeString();
                    FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(_SystemNameParamId, sys.Name);
                    ElementFilter elementFilter = new ElementParameterFilter(rule);
                    System.Drawing.Color sCol = _Colors[i];
                    Color color = new Color(sCol.R, sCol.G, sCol.B);
                    ParameterFilterElement pfe = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).FirstOrDefault(x => x.Name == sysName) as ParameterFilterElement;
                    try
                    {
                        if (pfe == null)
                        {
                            pfe = ParameterFilterElement.Create(
                                    doc,
                                    sysName,
                                    separateInsul ?
                                    _RevitMEPCategories :
                                    _RevitMEPCategories.Concat(_RevitMEPInsulCategories).ToList(),
                                    elementFilter);
                        }
                        else
                        {
                            pfe.SetCategories(
                                separateInsul ?
                                _RevitMEPCategories :
                                _RevitMEPCategories.Concat(_RevitMEPInsulCategories).ToList());
                            pfe.SetElementFilter(elementFilter);
                        }
                    }
                    catch (Exception ex)
                    {
                        UI.Test(ex.Message);
                    }
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    if (createLineFilters)
                    {
                        ogs.SetProjectionLinePatternId(patternId);
                        ogs.SetProjectionLineColor(color);
                    }
                    else
                    {
                        ogs.SetSurfaceForegroundPatternId(patternId);
                        ogs.SetSurfaceForegroundPatternColor(color);
                    }
                    template.SetFilterOverrides(pfe.Id, ogs);
                    cnt++;
                    if (separateInsul)
                    {
                        ParameterFilterElement pfeInsul = ParameterFilterElement.Create(doc, $"{sysName} - Insulation", _RevitMEPInsulCategories, elementFilter);
                        OverrideGraphicSettings ogsInsul = new OverrideGraphicSettings(ogs);
                        ogsInsul.SetSurfaceTransparency(50);
                        template.SetFilterOverrides(pfeInsul.Id, ogsInsul);
                        cnt++;
                    }
                    i++;
                    if (i >= _Colors.Count)
                    {
                        i = 0;
                    }
                }
                activeView.ApplyViewTemplateParameters(template);
                tr.Commit();
            }
            UI.Popup(_Title, $"{cnt} filters and graphics overrides created for the modeled systems. Time elapsed: {UI.StopTimer()}");
        }
    }
}
