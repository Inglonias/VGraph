using SkiaSharp;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.drawTools
{
    public class LineTool : IDrawTool
    {
        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            LineSegment[] rVal = new LineSegment[1];
            if (start.Equals(end))
            {
                return null;
            }
            rVal[0] = new LineSegment(start, end);

            return rVal;
        }
    }
}