using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Security.Cryptography.X509Certificates;
using RAMDATAACCESSLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextBox = System.Windows.Controls.TextBox;
using System.Globalization;
using Microsoft.Win32;
using Point = System.Windows.Point;
using System.Windows.Shapes;
using Line = System.Windows.Shapes.Line;


namespace rebarBenderMulti
{
    //A Custom Class that contains the revit structural framing member, the line that will be plotted on the mapping window and boolean that represents if the beam was mapped from RAM to Revit
    public class RevitBeam
    {
        public Element RevitElement { get; private set; }
        public System.Windows.Shapes.Line CustomLine { get; private set; }
        public bool Mapped { get; set; }
        public string ElementTypeName { get; private set; }
        public double centerX { get; private set; }
        public double centerY { get; private set; }

        public RevitBeam(Element elem)
        {
            if (elem == null)
            {
                throw new ArgumentNullException(nameof(elem));
            }

            RevitElement = elem;
            Mapped = false;


            // Get the start and end points of the structural framing element
            LocationCurve locationCurve = (RevitElement.Location as LocationCurve);
            // Get the start and end points of the structural framing element
            XYZ startPoint = locationCurve.Curve.GetEndPoint(0);
            XYZ endPoint = locationCurve.Curve.GetEndPoint(1);

            // Get the type name of the RevitElement, Usually Beam Size
            ElementTypeName = RevitElement.Name;

            // Convert Revit coordinates to WPF coordinates (assuming 1:1 mapping)
            Point startWpfPoint = new Point(startPoint.X, -startPoint.Y);
            Point endWpfPoint = new Point(endPoint.X, -endPoint.Y);

            // Calculate the center point of the line
            centerX = (startWpfPoint.X + endWpfPoint.X) / 2;
            centerY = (startWpfPoint.Y + endWpfPoint.Y) / 2;


            // Initialize or set up your System.Windows.Shapes.Line as needed
            // For this example, let's assume you want to create a line with start (0, 0) and end (100, 100)
            CustomLine = new System.Windows.Shapes.Line
            {
                X1 = startWpfPoint.X,
                Y1 = startWpfPoint.Y,
                X2 = endWpfPoint.X,
                Y2 = endWpfPoint.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Opacity = 0.5
            };
        }
    }
}
