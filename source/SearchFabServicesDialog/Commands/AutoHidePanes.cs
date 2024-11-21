using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.WPFFramework;
using Autodesk.UI.Windows.Controls;
using FabricationPartBrowser;
using FabricationPartBrowser.Modules;
using FabricationPartBrowser.ViewModels;
using FabSettings;
using Nice3point.Revit.Toolkit.External;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#if (REVIT2025)
                        //2025
#else
using System.Windows.Interactivity;
#endif
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Binding = System.Windows.Data.Binding;
using Grid = System.Windows.Controls.Grid;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using TextBox = System.Windows.Controls.TextBox;
namespace CODE.Free
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class AutoHidePanes : ExternalCommand
    {
        const string _search = "Search";
        const string _toolTip = "Enter text to filter services list";
        const string _isConnected = "IsConfigConnectedToSource";
        public static bool AllowAutoHide = false;
        public override void Execute()
        {
            CheckIn.Hello(this);
            //AllowAutoHide = CODE.Free.Properties.Settings.Default.AllowAutoHidePanes;
            CODE.Free.Properties.Settings.Default.AllowAutoHidePanes = true;
            CODE.Free.Properties.Settings.Default.Save();
            try
            {
                List<TabControl> panes = UIFramework.MainWindow.getMainWnd().FindChildrenByType<TabControl>();
                foreach (TabControl pane in panes)
                {
                    List<ContentPresenter> p = pane.FindChildrenByType<ContentPresenter>();
                    foreach (ContentPresenter cp in p)
                    {
                        if (cp.DataContext == null) continue;
                        PropertyInfo pi = cp.DataContext.GetType().GetProperty("CanAutoHide");
                        if (pi != null)
                        {
                            pi.SetValue(cp.DataContext, !(bool)pi.GetValue(cp.DataContext));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
            }
        }
    }

#if (REVIT2025)
#else
#endif
}
