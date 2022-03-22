using System;
using System.Collections.Generic;
using SkiaSharp;
using VGraph.src.objects;

namespace VGraph.src.drawTools
{
    public class EllipseTool : IDrawTool
    {
        public double FuzzRating { get; set; }

        public EllipseTool()
        {
            FuzzRating = 0.175;
        }

        public EllipseTool(double fuzz)
        {
            FuzzRating = fuzz;
        }

        public LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            SKPointI pointA = start;
            SKPointI pointB = end;
            SKPointI center = new SKPointI(pointA.X, pointB.Y);

            int radiusA = Convert.ToInt32(new LineSegment(pointA, center).GetLineLength());
            int radiusB = Convert.ToInt32(new LineSegment(pointB, center).GetLineLength());

            int numVertices = 3600;

            List<SKPointI> vertices = new List<SKPointI>();
            double angleSpacing = (Math.PI * 2) / numVertices;
            for (int i = 0; i < numVertices; i++)
            {
                double angle = i * angleSpacing;
                double rawX = radiusB * Math.Cos(angle);
                double rawY = radiusA * Math.Sin(angle);

                int intX = Convert.ToInt32(rawX);
                int intY = Convert.ToInt32(rawY);

                if (Math.Max(Math.Abs(rawX - intX), Math.Abs(rawY - intY)) < FuzzRating)
                {
                    SKPointI candidate = new SKPointI(intX, intY);
                    if (vertices.Count > 0)
                    {
                        SKPointI lastVertex = vertices[vertices.Count - 1];
                        if (!candidate.Equals(lastVertex))
                        {
                            vertices.Add(candidate);
                        }
                    }
                    else
                    {
                        vertices.Add(candidate);
                    }
                }
            }
            //Add the first element to the back of the list to ensure the circle closes.
            vertices.Add(new SKPointI(vertices[0].X, vertices[0].Y));
            List<LineSegment> lines = new List<LineSegment>();
            for (int i = 1; i < vertices.Count; i++)
            {
                SKPointI offsetStart = new SKPointI(vertices[i - 1].X + start.X, vertices[i - 1].Y + start.Y);
                SKPointI offsetEnd = new SKPointI(vertices[i].X + start.X, vertices[i].Y + start.Y);
                LineSegment l = new LineSegment(offsetStart, offsetEnd);
                lines.Add(l);
            }
            return lines.ToArray();
        }
    }
}