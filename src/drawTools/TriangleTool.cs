using SkiaSharp;
using System;
using VGraph.src.objects;

namespace VGraph.src.drawTools
{
    public class TriangleTool : IDrawTool
    {
        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            if (start.Equals(end))
            {
                return null;
            }
            LineSegment[] rVal = new LineSegment[3];

            SKPointI thirdCorner = new SKPointI(start.X, end.Y);

            rVal[0] = new LineSegment(start, end);
            rVal[1] = new LineSegment(end, thirdCorner);
            rVal[2] = new LineSegment(thirdCorner, start);

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

            return "X Len: " + xSize + "  Y Len: " + ySize + "  H Len: " + Math.Round(Math.Sqrt(Math.Pow(xSize, 2) + Math.Pow(ySize, 2)), 2);
        }
    }
}