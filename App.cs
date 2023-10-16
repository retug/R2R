#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

#endregion

namespace rebarBenderMulti
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            RibbonPanel curPanel = a.CreateRibbonPanel("R2R - RAM <-> Revit");

            // line gets where this .dll is location, this gives .dll itself
            string curAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            // line below gets directory name of .dll
            string curAssemblyPath = System.IO.Path.GetDirectoryName(curAssembly);

            // #3
            PushButtonData pbd1 = new PushButtonData("R2R", "Begin transfer of data", curAssembly, "rebarBenderMulti.xferInfo");
            

            SplitButtonData splitBtnData = new SplitButtonData("SplitButton", "Split Button");
            SplitButton splitBtn = curPanel.AddItem(splitBtnData) as SplitButton;
            splitBtn.AddPushButton(pbd1);


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
