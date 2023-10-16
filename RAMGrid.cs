using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rebarBenderMulti
{
    public class RAMGrid
    {
        public string Name { get; set; }
        public double Location { get; set; }

        //the double? allows you to test for the presence of null, thanks chatgpt
        public double? Min { get; set; }
        public double? Max { get; set; }
        public string U_V { get; set; }

    }
}
