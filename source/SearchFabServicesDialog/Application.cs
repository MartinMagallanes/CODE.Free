using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using CODE.Free.Utils;
using Nice3point.Revit.Toolkit.External;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using UIFramework;
using RibbonButton = Autodesk.Revit.UI.RibbonButton;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;
#if (REVIT2025)
//2025
#else
#endif

namespace CODE.Free
{
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        static string RibbonIcon16 => "/CODE.Free;component/Resources/Icons/RibbonIcon16.png";
        static string RibbonIcon32 => "/CODE.Free;component/Resources/Icons/RibbonIcon32.png";
        static string ImageCreateSurfacesFilters16 => "/CODE.Free;component/Resources/Icons/CreateSurfacesFilters16.png";
        static string ImageCreateSurfacesFilters32 => "/CODE.Free;component/Resources/Icons/CreateSurfacesFilters32.png";
        static string ImageCreateLinesFilters16 => "/CODE.Free;component/Resources/Icons/CreateLinesFilters16.png";
        static string ImageCreateLinesFilters32 => "/CODE.Free;component/Resources/Icons/CreateLinesFilters32.png";
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
            RibbonButton servicesBtn = sb.AddPushButton<CreateFabFiltersWithLines>("Create Filters\r\nWith Lines")
                .SetImage(ImageCreateLinesFilters16)
                .SetLargeImage(ImageCreateLinesFilters32);
            servicesBtn.ToolTip = "Create filters overriding line colors for Fabrication services";
            servicesBtn.LongDescription = "Create filter rules and line color overrides for the Fabrication services loaded in the current project. Optionally create insulation filters with 50% transparency.";
            ((PushButton)servicesBtn).AvailabilityClassName = typeof(CreateFiltersAvailability).FullName;
            sb.CurrentButton = servicesBtn as PushButton;

            RibbonButton servicesBtn2 = sb.AddPushButton<CreateFabFiltersWithSurfaces>("Create Filters\r\nWith Surfaces")
                .SetImage(ImageCreateSurfacesFilters16)
                .SetLargeImage(ImageCreateSurfacesFilters32);
            servicesBtn2.ToolTip = "Create filters overriding surface colors for Fabrication services";
            servicesBtn2.LongDescription = "Create filter rules and surface color overrides for the Fabrication services loaded in the current project. Optionally create insulation filters with 50% transparency.";
            ((PushButton)servicesBtn2).AvailabilityClassName = typeof(CreateFiltersAvailability).FullName;

            RibbonPanel mepPanel = Application.CreatePanel("MEP", TabName);
            SplitButton sb2 = mepPanel.AddSplitButton("createfilters", "Create Filters");
            sb2.IsSynchronizedWithCurrentItem = true;
            RibbonButton systemsBtn = sb2.AddPushButton<CreateFabFiltersWithLines>("Create Filters\r\nWith Lines")
                .SetImage(RibbonIcon16)
                .SetLargeImage(RibbonIcon32);
            systemsBtn.ToolTip = "Create filters overriding line colors for MEP systems";
            systemsBtn.LongDescription = "Create filter rules and line color overrides for the MEP systems in the current project. Optionally create insulation filters with 50% transparency.";
            ((PushButton)systemsBtn).AvailabilityClassName = typeof(CreateFiltersAvailability).FullName;
            sb2.CurrentButton = systemsBtn as PushButton;

            RibbonButton systemsBtn2 = sb2.AddPushButton<CreateFabFiltersWithSurfaces>("Create Filters\r\nWith Surfaces")
                .SetImage(RibbonIcon16)
                .SetLargeImage(RibbonIcon32);
            systemsBtn2.ToolTip = "Create filters overriding surface colors for MEP systems";
            systemsBtn2.LongDescription = "Create filter rules and surface color overrides for the MEP systems in the current project. Optionally create insulation filters with 50% transparency.";
            ((PushButton)systemsBtn2).AvailabilityClassName = typeof(CreateFiltersAvailability).FullName;

            //RibbonPanel testPanel = Application.CreatePanel("Test", TabName);
            //RibbonButton testBtn = testPanel.AddPushButton<Test>("Test")
            //    .SetImage(RibbonIcon16)
            //    .SetLargeImage(RibbonIcon32);
            RibbonPanel settings = Application.CreatePanel("Settings", TabName);
            RibbonButton cutInBtn = settings.AddPushButton<DisableCutIn>("No Cut-In")
                .SetImage(RibbonIcon16)
                .SetLargeImage(RibbonIcon32);
        }
        public override void OnShutdown()
        {
            this.Goodbye();
            base.OnShutdown();
        }
    }
}