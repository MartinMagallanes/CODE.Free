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
    public class CreateFiltersWithLines : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            View activeView = UiDocument.ActiveGraphicalView;
            CreateServiceFiltersExtensions.CreateFilters(activeView, 0);
        }
    }
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CreateFiltersWithSurfaces : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            View activeView = UiDocument.ActiveGraphicalView;
            CreateServiceFiltersExtensions.CreateFilters(activeView, 1);
        }
    }
    static class CreateServiceFiltersExtensions
    {
        static string _TemplateName = "CODE - Fabrication Services";
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
        static ElementId _ServiceParamId = new ElementId(BuiltInParameter.FABRICATION_SERVICE_PARAM);
        public static void CreateFilters(View activeView, int mode)
        {
            string cmd = mode == 0 ? "Create Filters With Lines" : "Create Filters With Surfaces";
            if (activeView is not View3D &&
                activeView is not ViewSection &&
                activeView is not ViewPlan)
            {
                UI.Popup(cmd, "Please open a model view before running this command.");
                return;
            }
            TaskDialog taskDialog = new TaskDialog(cmd);
            taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            taskDialog.MainInstruction = "Do you want to create separate filters for insulation/lining?";
            TaskDialogResult result = taskDialog.Show();
            bool separateInsul = result == TaskDialogResult.Yes;
            Document doc = activeView.Document;
            FabricationConfiguration fc = FabricationConfiguration.GetFabricationConfiguration(doc);
            if (fc == null || fc.GetAllLoadedServices().Count == 0)
            {
                UI.Popup(cmd, "No services loaded in the project. Please load services and try again.");
                return;
            }
            UI.StartTimer();
            int i = 0;
            int cnt = 0;
            ElementId patternId = ElementId.InvalidElementId;
            if (mode == 0)
            {
                patternId = LinePatternElement.GetSolidPatternId();
            }
            else
            {
                if (doc.GetSolidSurfacePattern() is FillPatternElement fpe)
                {
                    patternId = fpe.Id;
                }
                else
                {
                    UI.Popup(cmd, "Solid fill pattern not found in the project. Please load the pattern and try again.");
                    return;
                }
            }
            if (patternId == ElementId.InvalidElementId)
            {
                return;
            }
            List<System.Drawing.Color> colors = new List<System.Drawing.Color>();
            foreach (PropertyInfo propInfo in typeof(System.Drawing.Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                colors.Add((System.Drawing.Color)propInfo.GetValue(null));
            }
            colors.RemoveAll(color => color.IsSystemColor || color.IsColorTooLightOrDark());
            int max = colors.Count;
            using (Transaction tr = new Transaction(doc, cmd))
            {
                tr.Start();
                FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(View));
                View template = fec.Cast<View>().FirstOrDefault(x => x.IsTemplate && x.Name == _TemplateName);
                if (template == null)
                {
                    template = activeView.CreateViewTemplate();
                    template.Name = _TemplateName;
                }
                foreach (FabricationService service in fc.GetAllLoadedServices())
                {
                    string serviceName = service.Name.Replace(": ", " - ").LegalizeString();
                    FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(_ServiceParamId, service.ServiceId);
                    ElementFilter elementFilter = new ElementParameterFilter(rule);
                    try
                    {
                        System.Drawing.Color sCol = colors[i];
                        Color color = new Color(sCol.R, sCol.G, sCol.B);
                        ParameterFilterElement pfe =
                            ParameterFilterElement.Create(
                                doc,
                                serviceName,
                                separateInsul ?
                                    _FabCategories :
                                    _FabCategories.Concat(_FabInsulCategories).ToList(),
                                elementFilter);
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        if (mode == 0)
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
                        if (i < max)
                        {
                            i++;
                        }
                        else
                        {
                            i = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        UI.Popup(cmd, $"Error creating filter for service {service.Name}: {ex.Message}");
                    }
                }
                activeView.ApplyViewTemplateParameters(template);
                tr.Commit();
            }
            UI.Popup(cmd, $"{cnt} filters and graphics overrides created for the loaded services. Time elapsed: {UI.StopTimer()}");
        }
    }
}
