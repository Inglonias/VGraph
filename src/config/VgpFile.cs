using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.config
{
    internal class VgpFile
    {
        public int SquaresWide { get; set; }
        public int SquaresTall { get; set; }
        public int SquareSize { get; set; }
        public int Margin { get; set; }

        public List<LineSegment> Lines { get; set; }

        public VgpFile()
        {

        }
    }
}
