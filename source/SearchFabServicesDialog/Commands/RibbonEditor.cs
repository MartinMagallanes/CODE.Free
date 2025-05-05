using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using CODE.Free.Models;
using CODE.Free.Utils;
using CODE.Free.Views;
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
    public class RibbonEditor : ExternalCommand
    {
        public override void Execute()
        {
            this.Hello();
            RibbonEditorViewModel viewModel = new RibbonEditorViewModel();
            RibbonEditorView view = new RibbonEditorView(viewModel);
            view.Owner = UIFramework.MainWindow.getMainWnd();
            view.ShowDialog();
        }
    }
    public class RibbonEditorAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication app, CategorySet selectedCategories)
        {
            // Check if there is an active document open
            return app.ActiveUIDocument != null;
        }
    }
}
