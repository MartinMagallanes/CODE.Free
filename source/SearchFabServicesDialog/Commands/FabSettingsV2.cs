using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Fabrication;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Windows;
using FabricationPartBrowser;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using UIFramework;
using Control = System.Windows.Controls.Control;


namespace CODE.Free
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class FabSettingsV2 : ExternalCommand
    {
        //RibbonItemControl GetControl(Control parent, string name)
        //{
        //    if (parent is Autodesk.Windows.RibbonItemControl rc && rc.DataContext is RibbonButton rb && rb.Id != null && rb.Id.Contains(name))
        //    {

        //        return rc;
        //    }
        //    foreach (Control c in parent.FindChildrenByType<Control>())
        //    {
        //        if (GetControl(c, name) is RibbonItemControl c2)
        //        {
        //            return c2;
        //        }
        //    }
        //    return null;
        //}
        public override void Execute()
        {
            CheckIn.Hello(this);
            RevitCommandId cmd3 = RevitCommandId.LookupCommandId("ID_EXPORT_FABRICATION_PCF");
            IDictionary<Guid, Delegate> dic = getBeforeCommandEventDelegate(cmd3.Id);
            UI.Popup($"dic1 null: {dic == null}");
            foreach (RibbonTab tab in UIFramework.RevitRibbonControl.RibbonControl.Tabs)
            {
                if (!tab.Title.Contains("Modify"))
                {
                    continue;
                }
                foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
                {
                    //UI.Popup(panel.Source.Title);
                    if (!panel.Source.Title.Contains("Export"))
                    {
                        continue;
                    }
                    foreach (Autodesk.Windows.RibbonItem item in panel.Source.Items)
                    {
                        if (!item.Id.Contains("ID_EXPORT_FABRICATION_PCF"))
                        {
                            continue;
                        }
                        item.IsVisibleBinding = null;
                        item.IsEnabledBinding = null;
                        item.IsVisible = true;
                        item.IsEnabled = true;
                        (item.Tag as ControlHelperExtension).HideIfDisabled = false;

                        RevitCommandId cmd = RevitCommandId.LookupCommandId("ID_EXPORT_FABRICATION_PCF");
                        if (cmd != null)
                        {
                            try
                            {
                                UiApplication.RemoveAddInCommandBinding(cmd);
                            }
                            catch (System.Exception ex)
                            {
                                UI.Popup(ex.Message);
                            }
                            AddInCommandBinding b = UiApplication.CreateAddInCommandBinding(cmd);
                            if (b != null)
                            {
                                b.CanExecute += new EventHandler<CanExecuteEventArgs>(B_CanExecute);
                                b.Executed += new EventHandler<ExecutedEventArgs>(MM_ex);
                            }
                            dic = getBeforeCommandEventDelegate(cmd.Id);
                            UI.Popup($"cmd.id:{cmd.Id}\ndic2 null: {dic == null}");
                            if (dic != null)
                            {
                                foreach (Guid key in dic.Keys)
                                {
                                    UI.Popup($"key: {key}, {dic[key].Method.Name}");
                                }
                            }
                        }
                    }
                }
            }
            //Type uia = typeof(UIApplication);
            //System.Reflection.MethodInfo method = uia.GetMethod("getBeforeCommandEventDelegate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //UI.Popup($"method:{method != null}, cmd3:{cmd3 != null},{cmd3.Id}, uia:{UiApplication != null}");
            ////System.Reflection.MethodInfo method = uia.GetMethod("getCommandEventDelegate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //IDictionary<Guid, Delegate> before = method.Invoke(UiApplication, [cmd3.Id]) as IDictionary<Guid, Delegate>;
            //UI.Popup($"before: {before != null}");
            ////Delegate del = method.Invoke(UiApplication, cmd.Id, cmd.id
        }

        internal IDictionary<Guid, Delegate> getBeforeCommandEventDelegate(uint revitCmdId)
        {
            Dictionary<uint, IDictionary<Guid, Delegate>> dictionary = typeof(UIApplication).GetField("sm_ExecutedHandlerDictionary", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as Dictionary<uint, IDictionary<Guid, Delegate>>;
            UI.Popup($"has key: {dictionary.ContainsKey(revitCmdId)}");
            if (dictionary.ContainsKey(revitCmdId))
            {
                IDictionary<Guid, Delegate> dictionary2 = dictionary[revitCmdId];
                if (dictionary2 == null)
                {
                    UI.Popup("dictionary2 is null");
                    return null;
                }

                return dictionary2;
            }

            return null;
        }
        void B_CanExecute(object sender, CanExecuteEventArgs avgs)
        {
            avgs.CanExecute = true;
            //if (!(obj is UIApplication uiApplication) ||
            //    uiApplication.ActiveUIDocument == null ||
            //    uiApplication.ActiveUIDocument.Selection.GetElementIds().Count == 0 ||
            //    avgs.ActiveDocument == null ||
            //    !avgs.ActiveDocument.Application.IsMechanicalAnalysisEnabled ||
            //    avgs.ActiveDocument.IsModifiable)
            //    avgs.CanExecute = false;
            //else
            //    avgs.CanExecute = true;
        }
        void MM_ex(object sender, ExecutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PCF Files (*.pcf)|*.pcf",
                DefaultExt = "pcf",
                AddExtension = true,
                FileName = "ExportedPCF.pcf"
            };
            if (dialog.ShowDialog() != true)
            {
                UI.Popup(2);
                return;
            }
            FabricationUtils.ExportToPCF(
                e.ActiveDocument,
                (sender as UIApplication).ActiveUIDocument.Selection.GetElementIds().ToList(),
                dialog.FileName);
            UI.Popup("Done");
        }
    }
}
