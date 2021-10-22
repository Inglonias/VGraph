using System;

using SkiaSharp;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.drawTools
{
    public class CircleTool : IDrawTool
    {
        private const int SEGMENTS_PER_QUAD = 12;

        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            int radius = Math.Max(Math.Abs(start.X - end.X),Math.Abs(start.Y - end.Y));
            SKPointI north = new SKPointI(start.X, start.Y - radius);
            SKPointI south = new SKPointI(start.X, start.Y + radius);
            SKPointI east = new SKPointI(start.X + radius, start.Y);
            SKPointI west = new SKPointI(start.X - radius, start.Y);
            //Special case for itty-bitty circles.
            if (radius == 0) {
                return null;
            }

            List<SKPointI> vertices = new List<SKPointI>();
            double angleSpacing = (Math.Pi / 2) / SEGMENTS_PER_QUAD;
            for (int i = 0; i < SEGMENTS_PER_QUAD; i++) {
                double angle = i * angleSpacing;
                double rawX = radius * Math.Cos(angle);
                double rawY = radius * Math.Sin(angle);
                SKPointI roundedCoord = new SKPointI(Convert.ToInt32(rawX), Convert.ToInt32(rawY));
                if (!vertices.Contains(roundedCoord))
                {
                    vertices.Add(roundedCoord);
                }
            }
            List<LineSegment> lines = new List<LineSegment>();
            for (int i = 1; i < vertices.Count; i++) {
                LineSegment l = new LineSegment(vertices[i - 1], vertices[i]);
                lines.Add(l);
            }

            return lines.ToArray();
        }
    }
}