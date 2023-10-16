#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace rebarBenderMulti
{
    /// <summary>
    /// Start calling
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class xferInfo : IExternalCommand
    {
        // here starts all the calling of functions and stuff
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            MainWindow window = new MainWindow(uidoc);

            window.ShowDialog();

            return Result.Succeeded;
 
        }
    }
}