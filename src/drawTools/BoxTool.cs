using SkiaSharp;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.drawTools
{
    public class BoxTool : IDrawTool
    {
        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            LineSegment[] rVal = new LineSegment[4];

            SKPointI topRightCorner = new SKPointI(end.X, start.Y);
            SKPointI bottomLeftCorner = new SKPointI(start.X, end.Y);

            rVal[0] = new LineSegment(start, topRightCorner);
            rVal[1] = new LineSegment(topRightCorner, end);
            rVal[2] = new LineSegment(end, bottomLeftCorner);
            rVal[3] = new LineSegment(bottomLeftCorner, start);

            return rVal;
        }
    }
}