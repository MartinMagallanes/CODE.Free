using Autodesk.Revit.UI;
using CODE.Free.Utils;
using Nice3point.Revit.Toolkit.External;
using System.Net.Http;
using System.Text;
#if (REVIT2025)
//2025
#else
#endif

namespace CODE.Free
{
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        static string Image => "/CODE.Free;component/Resources/Icons/RibbonIcon16.png";
        static string ImageCreateSurfacesFilters16 => "/CODE.Free;component/Resources/Icons/CreateSurfacesFilters16.png";
        static string ImageCreateSurfacesFilters32 => "/CODE.Free;component/Resources/Icons/CreateSurfacesFilters32.png";
        static string ImageCreateLinesFilters16 => "/CODE.Free;component/Resources/Icons/CreateLinesFilters16.png";
        static string ImageCreateLinesFilters32 => "/CODE.Free;component/Resources/Icons/CreateLinesFilters32.png";
        static string RibbonIcon32 => "/CODE.Free;component/Resources/Icons/RibbonIcon32.png";
        static string TabName => "CODE";
        public override void OnStartup()
        {
            CreateRibbon();
            new FabSettingsV2().Update();
        }
        void CreateRibbon()
        {
            //RevitCommandId cmd = RevitCommandId.LookupCommandId("ID_EXPORT_FABRICATION_PCF");
            //if (cmd != null)
            //{
            //    UI.Popup($"can bind:{cmd.CanHaveBinding},has bind:{cmd.HasBinding},id:{cmd.Id},name:{cmd.Name}");
            //    try
            //    {
            //        UiApplication.RemoveAddInCommandBinding(cmd);
            //    }
            //    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            //    {

            //    }
            //    AddInCommandBinding b = UiApplication.CreateAddInCommandBinding(cmd);
            //    if (b != null)
            //    {
            //        b.CanExecute += new EventHandler<CanExecuteEventArgs>(B_CanExecute);
            //        b.Executed += new EventHandler<ExecutedEventArgs>(B_Executed);
            //    }
            //}

            //RibbonPanel panel = Application.CreatePanel("Free", TabName);
            //RibbonButton btn = panel.AddPushButton<Halloween>("Halloween\n\rTheme")
            //    .SetLargeImage(LargeImage);
            //btn.LongDescription = "Update theme";
            //RibbonPanel panel = Application.CreatePanel("Free", TabName);
            //RibbonButton btn = panel.AddPushButton<AutoHidePanes>("Auto-hide\n\r")
            //    .SetLargeImage(LargeImage);
            //btn.LongDescription = "Toggle visibility of the auto-hide button for dockable windows like the Properties and Project Browser panes.";

            RibbonPanel fabPanel = Application.CreatePanel("Fabrication", TabName);
            SplitButton sb = fabPanel.AddSplitButton("createfilters", "Create Filters");
            sb.IsSynchronizedWithCurrentItem = true;
            RibbonButton servicesBtn = sb.AddPushButton<CreateFiltersWithLines>("Create Filters\r\nWith Lines")
                .SetImage(ImageCreateLinesFilters16)
                .SetLargeImage(ImageCreateLinesFilters32);
            sb.CurrentButton = servicesBtn as PushButton;
            servicesBtn.ToolTip = "Create filters overriding line colors for Fabrication services";
            servicesBtn.LongDescription = "Create filter rules and line color overrides for the Fabrication services loaded in the current project. Optionally create insulation filters with 50% transparency.";
            RibbonButton servicesBtn2 = sb.AddPushButton<CreateFiltersWithSurfaces>("Create Filters\r\nWith Surfaces")
                .SetImage(ImageCreateSurfacesFilters16)
                .SetLargeImage(ImageCreateSurfacesFilters32);
            servicesBtn2.ToolTip = "Create filters overriding surface colors for Fabrication services";
            servicesBtn2.LongDescription = "Create filter rules and surface color overrides for the Fabrication services loaded in the current project. Optionally create insulation filters with 50% transparency.";

        }
        public override void OnShutdown()
        {
            this.Goodbye();
            base.OnShutdown();
        }
    }
}