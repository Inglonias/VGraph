using System.Collections.Generic;
using VGraph.src.objects;

namespace VGraph.src.config
{
    //Class used to serialize a canvas for saving and loading files.
    internal class VgpFile
    {
        public int SquaresWide { get; set; }
        public int SquaresTall { get; set; }
        public int SquareSize { get; set; }
        public int MarginX { get; set; }
        public int MarginY { get; set; }

        public List<LineSegment> Lines { get; set; }

        public VgpFile()
        {

        }
    }
}
