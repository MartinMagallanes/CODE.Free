using Autodesk.Revit.Attributes;
using Autodesk.Windows;
using CODE.Free.Utils;
using Nice3point.Revit.Toolkit.External;
using System.ComponentModel;
using System.Windows;
using UIFramework;
namespace CODE.Free
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class DisableCutIn : ExternalCommand
    {
        static bool userClicked = false;
        static int loop = 0;
        static bool _allowCutIn = true;
        public static bool AllowCutIn
        {
            get
            {
                return _allowCutIn;
            }
            private set
            {
                _allowCutIn = value;
                //Properties.Settings.Default.AllowCutIn = value;
                //Properties.Settings.Default.Save();
            }
        }
        static RibbonToggleButton tb;
        static RibbonTab rt;
        static ToggleButtonControl tbc;
        public override void Execute()
        {
            //AllowCutIn = Properties.Settings.Default.AllowCutIn;
            RibbonControl ribbon = RevitRibbonControl.RibbonControl;
            rt = ribbon.FindTab("Modify");
            rt.PropertyChanged -= Ribbon_PropertyChanged;
            rt.PropertyChanged += Ribbon_PropertyChanged;
        }

        private void Ribbon_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            loop = 0;
            if (rt.IsContextualTab)
            {
                string cmdId = UIFrameworkServices.CommandHandlerService.getActiveCommandId();
                if (cmdId == "ID_PLACE_FABRICATION_PART" || cmdId == "ID_CREATE_FABRICATIONPART")
                {
                    tb = rt.FindItem("ID_FABRICATION_PART_INSERT") as RibbonToggleButton;
                    if (tb != null)
                    {
                        rt.PropertyChanged -= Ribbon_PropertyChanged;
                        tb.PropertyChanged -= tb_PropertyChanged;
                        tb.PropertyChanged += tb_PropertyChanged;
                    }
                }
            }
        }
        private void tb_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //UI.Test($"prop:{e.PropertyName},ischecked:{tb.IsChecked}");
            if (e.PropertyName != "IsChecked")
            {
                return;
            }
            loop++;
            if (loop > 10)
            {
                tb.PropertyChanged -= tb_PropertyChanged;
            }
            if (tbc == null)
            {
                tbc = UIFramework.ContextualTabHelper.ModifyTab.RibbonControl.FindChildrenByType<ToggleButtonControl>().FirstOrDefault(b => b.Text.Contains("Insert"));
                if (tbc != null)
                {
                    tbc.Click -= Tbc_Click;
                    tbc.Click += Tbc_Click;
                }
            }
            if (tbc == null)
            {
                return;
            }
            //UI.Test($"1. tbc.ischecked:{tbc.IsChecked},tb.ischecked:{tb.IsChecked},allowcutin:{AllowCutIn}");
            if (userClicked)
            {
                userClicked = false;
                AllowCutIn = tb.IsChecked;
            }
            else if (tb.IsChecked != AllowCutIn)
            {
                try
                {
                    UIFrameworkServices.CommandHandlerService.invokeCommandHandler("ID_PLACE_FABRICATION_PART");
                    UIFrameworkServices.CommandHandlerService.invokeCommandHandler("ID_FABRICATION_PART_INSERT");
                    //tb.IsChecked = AllowCutIn;
                    //UI.Test($"2. tbc.ischecked:{tbc.IsChecked},tb.ischecked:{tb.IsChecked},allowcutin:{AllowCutIn}");
                }
                catch (Exception ex)
                {
                    UI.Test($"ex:{ex.Message}");
                }
            }
        }
        private void Tbc_Click(object sender, RoutedEventArgs e)
        {
            userClicked = true;
        }
    }
}
