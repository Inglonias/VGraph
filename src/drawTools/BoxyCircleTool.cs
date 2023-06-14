using System;
using System.Collections.Generic;
using SkiaSharp;
using VGraph.src.objects;

namespace VGraph.src.drawTools
{
    public class BoxyCircleTool : EllipseTool
    {
        public BoxyCircleTool()
        {
            FuzzRating = 0.175;
        }

        public BoxyCircleTool(double fuzz)
        {
            FuzzRating = fuzz;
        }

        public override LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            int radius = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));

            return DrawManhattanStyle(DrawEllipse(start, radius, radius));
        }

        public override LineSegment[] DrawWithToolOdd(SKPointI start, SKPointI end)
        {
            int radius = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
            return DrawManhattanStyle(DrawEllipseOdd(start, radius, radius));
        }

        public LineSegment[] DrawManhattanStyle(LineSegment[] input)
        {
            LineSegment[] output = new LineSegment[2 * input.Length];
            if (input.Length == 0)
            {
                return output;
            }
            for (int i = 0; i < output.Length; i += 2)
            {
                int xStart = input[i/2].StartPointGrid.X;
                int yStart = input[i/2].StartPointGrid.Y;
                int xEnd = input[i/2].EndPointGrid.X;
                int yEnd = input[i/2].EndPointGrid.Y;
                LineSegment yComp = new LineSegment(new SKPointI(xStart, yStart), new SKPointI(xStart, yEnd));
                LineSegment xComp = new LineSegment(new SKPointI(xStart, yEnd), new SKPointI(xEnd, yEnd));
                output[i] = yComp;
                output[i + 1] = xComp;
            }
            return output;
        }

        public override string GenerateStatusText(SKPointI start, SKPointI end)
        {
            int xSize = System.Math.Abs(start.X - end.X);
            int ySize = System.Math.Abs(start.Y - end.Y);

            return "Radius: " + Math.Max(xSize, ySize);
        }
    }
}