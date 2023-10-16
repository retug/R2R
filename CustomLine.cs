using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; // Import the System.Windows namespace for Point

namespace rebarBenderMulti
{
    public class CustomLine
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public string Name { get; set; }

        public CustomLine(Point startPoint, Point endPoint, string name)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Name = name;
        }
    }
}
