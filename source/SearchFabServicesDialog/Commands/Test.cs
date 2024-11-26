using Autodesk.Revit.Attributes;
using CODE.Free.Utils;
using Nice3point.Revit.Toolkit.External;
using System.ComponentModel;
using Autodesk.Windows;
using System.Reflection;
using UIFramework;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System;
using System.Windows;
using FabricationPartBrowser;
using UIFrameworkServices;
using Autodesk.Revit.UI;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
#if (REVIT2025)
                        //2025
#else
#endif
namespace CODE.Free
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Test : ExternalCommand
    {
        static int loop = 0;
        public override void Execute()
        {
            //LoadCadmepServices();
        }
        void LoadCadmepServices()
        {
            //doesn't work, didn't load fabapi in first attempt
            Assembly assem = Assembly.LoadFrom(@"C:\Program Files\Autodesk\Fabrication 2021\CADmep\FabricationAPI.dll");
            UI.Popup($"assem null:{assem == null}");
            if (assem != null)
            {
                UI.Popup($"services: {Autodesk.Fabrication.DB.Database.Services?.Count}");
            }
        }
    }
}
