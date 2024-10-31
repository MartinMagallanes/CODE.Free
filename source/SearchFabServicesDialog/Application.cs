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
using System.Net.Http;
using System.Reflection;
using System.Text;
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
using TextBox = System.Windows.Controls.TextBox;

namespace CODE.Free
{
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            CheckIn.Hello(this);
            //CreateRibbon();
            new FabSettingsV2().Update();
        }
        void CreateRibbon()
        {
            RevitCommandId cmd = RevitCommandId.LookupCommandId("ID_EXPORT_FABRICATION_PCF");
            if (cmd != null)
            {
                UI.Popup($"can bind:{cmd.CanHaveBinding},has bind:{cmd.HasBinding},id:{cmd.Id},name:{cmd.Name}");
                try
                {
                    UiApplication.RemoveAddInCommandBinding(cmd);
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {

                }
                AddInCommandBinding b = UiApplication.CreateAddInCommandBinding(cmd);
                if (b != null)
                {
                    //b.CanExecute += new EventHandler<CanExecuteEventArgs>(B_CanExecute);
                    //b.Executed += new EventHandler<ExecutedEventArgs>(B_Executed);
                }
            }

            //RibbonPanel panel = Application.CreatePanel(_panelName, _tabName);
            //var btn = panel.AddPushButton<FabSettingsV2>(_btnText)
            //    .SetImage(_img)
            //    .SetLargeImage(_largeImg);
            //btn.LongDescription = _description;

            //var panel = Application.CreatePanel("Commands", "Tag3D");

            //panel.AddPushButton<>("Execute")
            //    .SetImage("/Tag3D;component/Resources/Icons/RibbonIcon16.png")
            //    .SetLargeImage("/Tag3D;component/Resources/Icons/RibbonIcon32.png");
        }
        public override void OnShutdown()
        {
            CheckIn.Goodbye();
            base.OnShutdown();
        }
    }
    internal static class CheckIn
    {
        const string _hello = "https://licensing.contentorigin.dev/api/Hello";
        const string _goodbye = "https://licensing.contentorigin.dev/api/Hello/goodbye";
        const string _appJson = "application/json";
        public static List<string> Addins = new List<string>();
        static HttpClient _client = new HttpClient();
        public static void Hello(object type)
        {
            try
            {
                string addin = type.GetType().FullName;
                if (!Addins.Contains(addin))
                {
                    Addins.Add(addin);
                }
                _client.PostAsync(_hello, new StringContent(Serialize(Context.Application.Username, addin), Encoding.UTF8, _appJson));
            }
            catch { }
        }
        public static void Goodbye()
        {
            try
            {
                foreach (string addin in Addins)
                {
                    _client.PostAsync(_goodbye, new StringContent(Serialize(Context.Application.Username, addin), Encoding.UTF8, _appJson));
                }
            }
            catch { }
        }
        static string Serialize(string userName, string productId)
        {
            return
            $"{{\"AutodeskUsername\":\"{userName}\"," +
            $"\"ProductIds\":[\"{productId}\"]," +
            $"\"ActiveProductIds\":[\"{productId}\"]}}";
        }
    }
}