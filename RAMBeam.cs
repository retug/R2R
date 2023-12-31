using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace rebarBenderMulti
{
    public class RAMBeam
    {
        public string Name { get; set; }
        public List<double> StartPoint { get; set; }
        public List<double> EndPoint { get; set; }
        public double BeamLength { get; set; }

        public System.Windows.Shapes.Line CustomLine { get; private set; }
        public bool Mapped { get; set; }
        public string ElementTypeName { get; private set; }
        public double centerX { get; private set; }
        public double centerY { get; private set; }

        public TextBlock beamName { get; private set; }

        public RAMBeam(string Name, List<double> StartPoint, List<double> EndPoint, double BeamLength)
        {
            this.Name = Name;
            this.StartPoint = StartPoint;
            this.EndPoint = EndPoint;
            this.BeamLength = BeamLength;
        }

        public void ConvertToGlobal(List<double> StartPoint, List<Double> EndPoint, GlobalCoordinateSystem gcs)
        {
            //Initalize a point that represents the start point of the RAM Beam in the local coordinate system, 0 in W direction, no verticality
            r2rPoint startRAMPoint = new r2rPoint(StartPoint[0], StartPoint[1], 0, false);
            startRAMPoint.Convert_To_Global(gcs);

            //Initalize a point that represents the end point of the RAM Beam in the local coordinate system
            r2rPoint endRAMPoint = new r2rPoint(EndPoint[0], EndPoint[1], 0, false);
            endRAMPoint.Convert_To_Global(gcs);

            //These are the points that have been converted into the global Revit coordinate system
            Point startPoint = new Point(startRAMPoint.X/12, startRAMPoint.Y/12);
            //These are the points that have been converted into the global Revit coordinate system
            Point endPoint= new Point(endRAMPoint.X/12, endRAMPoint.Y/12);

            // Calculate the center point of the line
            centerX = (startPoint.X + endPoint.X) / 2;
            centerY = (startPoint.Y + endPoint.Y) / 2;

            // Initialize or set up your System.Windows.Shapes.Line as needed
            CustomLine = new System.Windows.Shapes.Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                Opacity = 0.5
            };

            // Create TextBlock for ElementType name
            beamName = new TextBlock
            {
                Text = Name,
                Foreground = Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.5 
            };
        }
    }
}
