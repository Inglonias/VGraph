using SkiaSharp;
using VGraph.src.objects;

namespace VGraph.src.drawTools
{
    public class BoxTool : IDrawTool
    {
        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            if (start.Equals(end))
            {
                return null;
            }
            LineSegment[] rVal = new LineSegment[4];

            SKPointI topRightCorner = new SKPointI(end.X, start.Y);
            SKPointI bottomLeftCorner = new SKPointI(start.X, end.Y);

            rVal[0] = new LineSegment(start, topRightCorner);
            rVal[1] = new LineSegment(topRightCorner, end);
            rVal[2] = new LineSegment(end, bottomLeftCorner);
            rVal[3] = new LineSegment(bottomLeftCorner, start);

            return rVal;
        }

        public LineSegment[] DrawWithToolOdd(SKPointI start, SKPointI end)
        {
            return DrawWithTool(start, end);
        }

        public string GenerateStatusText(SKPointI start, SKPointI end)
        {
            int xSize = System.Math.Abs(start.X - end.X);
            int ySize = System.Math.Abs(start.Y - end.Y);

            return "Box Size: " + xSize + " x " + ySize;
        }
    }
}