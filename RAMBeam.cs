using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rebarBenderMulti
{
    public class RAMBeam
    {
        public string Name { get; set; }
        public List<double> StartPoint { get; set; }
        public List<double> EndPoint { get; set; }
        public double BeamLength { get; set; }

        public RAMBeam(string Name, List<double> StartPoint, List<double> EndPoint, double BeamLength)
        {
            this.Name = Name;
            this.StartPoint = StartPoint;
            this.EndPoint = EndPoint;
            this.BeamLength = BeamLength;
        }

    }
}
