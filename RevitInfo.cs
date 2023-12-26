using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Grid = Autodesk.Revit.DB.Grid;


namespace rebarBenderMulti
{
    public class RevitInfo
    {
        public void GatherRevitGrids(object sender, RoutedEventArgs e, Document doc, ref List<CustomLine> RevitGrids)
        {
            //UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //Document doc = uidoc.Document;

            // Create a filtered element collector to get all grid lines
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> gridLines = collector.OfClass(typeof(Grid)).ToElements();

            

            if (gridLines.Count == 0)
            {
                TaskDialog.Show("No Grid Lines", "There are no grid lines in the current project.");
            }


            // Iterate through the grid lines and access their properties
            foreach (Element gridLine in gridLines)
            {
                if (gridLine is Grid)
                {
                    Grid grid = gridLine as Grid;

                    // Access grid properties
                    string gridName = grid.Name;

                    //this assumes no curved grids
                    Line gridLineGeometry = grid.Curve as Line;
                    XYZ startGrid = gridLineGeometry.GetEndPoint(0);
                    XYZ endGrid = gridLineGeometry.GetEndPoint(1);

                    System.Windows.Point startPoint = new System.Windows.Point(startGrid.X, startGrid.Y);
                    System.Windows.Point endPoint = new System.Windows.Point(endGrid.X, endGrid.Y);
                    double startGridX = startGrid.X;
                    double startGridY = startGrid.Y;
                    double endGridX = endGrid.X;
                    double endGridY = endGrid.Y;
                    RevitGrids.Add(new CustomLine(startPoint, endPoint, gridName));

                }
            }
        }

        public void GatherRevitBeams(object sender, RoutedEventArgs e, Document doc, UIDocument uidoc, ref List<CustomLine> RevitBeams)
        {

            //we will pass in doc and uidoc becuase these are already being accessed in the mainwindow xaml code
            IList<Reference> pickedObjs = uidoc.Selection.PickObjects(ObjectType.Element, "Select Elements");

            // Create a filter for structural framing category
            ElementCategoryFilter categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            // Use the filter to collect elements
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> structuralFramingElements = collector.WherePasses(categoryFilter).OfCategory(BuiltInCategory.OST_StructuralFraming).ToElements();

            if (structuralFramingElements.Count == 0)
            {
                TaskDialog.Show("No Structural Framing Elements", "There are no structural framing elements in the selected elements.");
            }

            // Iterate through the structural framing elements and access their properties
            foreach (Element structuralFramingElement in structuralFramingElements)
            {
                if (structuralFramingElement is FamilyInstance framingInstance)
                {
                    // Access framing instance properties
                    LocationCurve locationCurve = framingInstance.Location as LocationCurve;
                    Curve curve = locationCurve.Curve;

                    // Access curve properties
                    XYZ startFraming = curve.GetEndPoint(0);
                    XYZ endFraming = curve.GetEndPoint(1);

                    System.Windows.Point startPoint = new System.Windows.Point(startFraming.X, startFraming.Y);
                    System.Windows.Point endPoint = new System.Windows.Point(endFraming.X, endFraming.Y);
                    string framingName = framingInstance.Symbol.Family.Name;

                    RevitBeams.Add(new CustomLine(startPoint, endPoint, framingName));
                }
            }
        }
    }
}
