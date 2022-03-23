using SkiaSharp;
using VGraph.src.objects;

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

        public LineSegment[] DrawWithToolOdd(SKPointI start, SKPointI end)
        {
            return DrawWithTool(start, end);
        }
    }
}