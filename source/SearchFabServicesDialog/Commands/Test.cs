using Autodesk.Revit.UI;
using System.Reflection;
#if (REVIT2025)
                        //2025
#else
#endif
namespace CODE.Free
{
    public class Free
    {
        public void CreateServiceFilters(View activeView)
        {
            Document doc = activeView.Document;
            FabricationConfiguration config = FabricationConfiguration.GetFabricationConfiguration(doc);
            if (config == null || config.GetAllLoadedServices().Count == 0)
            {
                TaskDialog.Show("Create Service Filters", "No services loaded in the project. Please load services and try again.");
                return;
            }
            ICollection<ElementId> categories = new List<ElementId>()
                {
                    new ElementId(BuiltInCategory.OST_FabricationContainment),
                    new ElementId(BuiltInCategory.OST_FabricationPipework),
                    new ElementId(BuiltInCategory.OST_FabricationPipeworkInsulation),
                    new ElementId(BuiltInCategory.OST_FabricationDuctwork),
                    new ElementId(BuiltInCategory.OST_FabricationDuctworkInsulation),
                    new ElementId(BuiltInCategory.OST_FabricationDuctworkLining),
                    new ElementId(BuiltInCategory.OST_FabricationHangers),
                };
            ElementId paramId = new ElementId(BuiltInParameter.FABRICATION_SERVICE_PARAM);
            ElementId patternId = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "<Solid fill>").Id;
            int index = 0;
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            foreach (PropertyInfo propInfo in typeof(System.Windows.Media.Colors).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                colors.Add((System.Windows.Media.Color)propInfo.GetValue(null));
            }
            int max = colors.Count;
            int count = 0;
            using (Transaction trans = new Transaction(doc, "Create Service Filters"))
            {
                trans.Start();
                FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(View));
                View template = fec.Cast<View>().FirstOrDefault(x => x.IsTemplate && x.Name == "Fabrication Services");
                if (template == null)
                {
                    template = activeView.CreateViewTemplate();
                    template.Name = "Fabrication Services";
                }
                foreach (FabricationService service in config.GetAllLoadedServices())
                {
                    string serviceName = service.Name.Replace(": ", " - ");
                    FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(paramId, service.ServiceId);
                    ElementFilter elementFilter = new ElementParameterFilter(rule);
                    try
                    {
                        ParameterFilterElement pfe = ParameterFilterElement.Create(doc, serviceName, categories, elementFilter);
                        OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                        ogs.SetSurfaceForegroundPatternId(patternId);
                        System.Windows.Media.Color sCol = colors[index];
                        Autodesk.Revit.DB.Color color = new Autodesk.Revit.DB.Color(sCol.R, sCol.G, sCol.B);
                        ogs.SetSurfaceForegroundPatternColor(color);
                        activeView.SetFilterOverrides(pfe.Id, ogs);
                        count++;
                        if (index < max)
                        {
                            index++;
                        }
                        else
                        {
                            index = 0;
                        }
                    }
                    catch (Exception ex) { }
                }
                activeView.ApplyViewTemplateParameters(template);
                trans.Commit();
            }
        }
    }

#if (REVIT2025)
#else
#endif
}
