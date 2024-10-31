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
    public class SetParameterByFilter : ExternalCommand
    {
        public override void Execute()
        {
            UI.StartTimer();
            //CheckIn.Hello(this);
            //filter by current view
            Document doc = UiApplication.ActiveUIDocument.Document;
            ElementId viewId = UiApplication.ActiveUIDocument.ActiveGraphicalView.Id;

            //filter by elevation, 2.0 meters or higher from instance level
            ElementId parameterId = new ElementId(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
            double height = UnitUtils.ConvertToInternalUnits(2.0, UnitTypeId.Meters);
            FilterRule filter = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(parameterId, height, 0.0);
            ElementParameterFilter elementParameterFilter = new ElementParameterFilter(filter);

            //collect filtered elements
            FilteredElementCollector collector =
                new FilteredElementCollector(doc, viewId)
                .OfClass(typeof(FamilyInstance))
                .WherePasses(elementParameterFilter);

            //use transaction to modify the document
            using (Transaction transaction = new Transaction(doc, "Set Parameter By Filter"))
            {
                transaction.Start();
                foreach (FamilyInstance familyInstance in collector)
                {
                    //set the parameter value
                    familyInstance.LookupParameter("H-Terminal")?.Set("Terminal");
                }
                transaction.Commit();
            }
            UI.Test($"{collector.GetElementCount()} elements updated in {UI.StopTimer()}");
        }
    }
}
