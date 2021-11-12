using System;
using System.Collections.Generic;
using SkiaSharp;
using VGraph.src.objects;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.drawTools
{
    public class CircleTool : IDrawTool
    {
        public double FuzzRating { get; set; }

        public CircleTool()
        {
            FuzzRating = 0.3;
        }

        public CircleTool(double fuzz)
        {
            FuzzRating = fuzz;
        }

        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            int radius = Math.Max(Math.Abs(start.X - end.X),Math.Abs(start.Y - end.Y));
            List<LineSegment> lines = new List<LineSegment>();
            if (radius < 1) {
                return null;
            }
            //Radius of 1 needs four vertices to be sensible.
            int numVertices = 64 * radius;

            List<SKPointI> vertices = new List<SKPointI>();
            double angleSpacing = (Math.PI * 2) / numVertices;
            for (int i = 0; i < numVertices; i++) {
                double angle = i * angleSpacing;
                double rawX = radius * Math.Cos(angle);
                double rawY = radius * Math.Sin(angle);
                if (Math.Max(Math.Abs(rawX - Math.Round(rawX)), Math.Abs(rawY - Math.Round(rawY))) < FuzzRating )
                {
                    vertices.Add(new SKPointI(Convert.ToInt32(rawX), Convert.ToInt32(rawY)));
                }
            }
            //Add the first element to the back of the list to ensure the circle closes.
            vertices.Add(new SKPointI(vertices[0].X, vertices[0].Y));
            for (int i = 1; i < vertices.Count; i++) {
                SKPointI offsetStart = new SKPointI(vertices[i - 1].X + start.X, vertices[i - 1].Y + start.Y);
                SKPointI offsetEnd = new SKPointI(vertices[i].X + start.X, vertices[i].Y + start.Y);
                LineSegment l = new LineSegment(offsetStart, offsetEnd);
                lines.Add(l);
            }
            return lines.ToArray();
        }
    }
}