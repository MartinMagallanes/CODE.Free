using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Fabrication;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Windows;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External;
using UIFramework;
namespace CODE.Free
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class UpdateExportPcf : ExternalCommand
    {
        public override void Execute()
        {
            CheckIn.Hello(this);
            RevitCommandId cmd3 = RevitCommandId.LookupCommandId("ID_EXPORT_FABRICATION_PCF");
            IDictionary<Guid, Delegate> dic = getBeforeCommandEventDelegate(cmd3.Id);
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
                                b.Executed += new EventHandler<ExecutedEventArgs>(B_Executed);
                            }
                            dic = getBeforeCommandEventDelegate(cmd.Id);
                            if (dic != null)
                            {
                                foreach (Guid key in dic.Keys)
                                {
                                    //UI.Popup($"key: {key}, {dic[key].Method.Name}");
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
            //avgs.CanExecute = true;
            if (!(sender is UIApplication uiApplication) ||
                uiApplication.ActiveUIDocument == null ||
                uiApplication.ActiveUIDocument.Selection.GetElementIds().Count == 0 ||
                avgs.ActiveDocument == null ||
                !avgs.ActiveDocument.Application.IsMechanicalAnalysisEnabled ||
                avgs.ActiveDocument.IsModifiable)
                avgs.CanExecute = false;
            else
                avgs.CanExecute = true;
        }
        void B_Executed(object sender, ExecutedEventArgs e)
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
