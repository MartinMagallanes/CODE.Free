using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;

namespace CODE.Free
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class FabSettingsV2 : ExternalCommand
    {
        public override void Execute()
        {
            //CODE.Free.Application.OverrideFabConfigDialog();
        }
    }
}
