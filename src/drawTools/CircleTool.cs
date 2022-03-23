using System;
using System.Collections.Generic;
using SkiaSharp;
using VGraph.src.objects;

namespace VGraph.src.drawTools
{
    public class CircleTool : EllipseTool
    {
        public CircleTool()
        {
            FuzzRating = 0.175;
        }

        public CircleTool(double fuzz)
        {
            FuzzRating = fuzz;
        }

        public override LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            int radius = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
            return DrawEllipse(start, end, radius, radius);
        }

        public override LineSegment[] DrawWithToolOdd(SKPointI start, SKPointI end)
        {
            int radius = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
            return DrawEllipseOdd(start, end, radius, radius);
        }
    }
}