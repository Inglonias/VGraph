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

        public virtual LineSegment[] DrawWithTool(SKPointI start, SKPointI end)
        {
            SKPointI center = new SKPointI(start.X, end.Y);
            int radiusA = Convert.ToInt32(new LineSegment(start, center).GetLineLength());
            int radiusB = Convert.ToInt32(new LineSegment(end, center).GetLineLength());

            return DrawEllipse(start, radiusA, radiusB);
        }

        public virtual LineSegment[] DrawWithToolOdd(SKPointI start, SKPointI end)
        {
            SKPointI center = new SKPointI(start.X, end.Y);
            int radiusA = Convert.ToInt32(new LineSegment(start, center).GetLineLength());
            int radiusB = Convert.ToInt32(new LineSegment(end, center).GetLineLength());

            return DrawEllipseOdd(start, radiusA, radiusB);
        }

        protected LineSegment[] DrawEllipse(SKPointI start, int radiusA, int radiusB)
        {
            int numVertices = 3600 * ((Math.Max(radiusA, radiusB) / 25) + 1);

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
                if (!lines.Contains(l))
                {
                    lines.Add(l);
                }
            }
            return lines.ToArray();
        }

        protected LineSegment[] DrawEllipseOdd(SKPointI start, int radiusA, int radiusB)
        {
            SKPointI[] radiusStart = { new SKPointI(start.X + 1, start.Y    ),
                                       new SKPointI(start.X + 1, start.Y + 1),
                                       new SKPointI(start.X    , start.Y + 1),
                                       new SKPointI(start.X    , start.Y    )};

            int[] xMod = { 1, 1, 0, 0 };
            int[] yMod = { 0, 1, 1, 0 };

            List<SKPointI> vertices = new List<SKPointI>();
            List<LineSegment> lines = new List<LineSegment>();

            for (int i = 0; i < radiusStart.Length; i++)
            {
                //This method draws by quadrant to ensure symmetry when the ellipse radius is not an integer.
                int numVertices = 900 * ((Math.Max(radiusA, radiusB) / 25) + 1);

                double angleSpacing = (Math.PI / 2) / numVertices;
                for (int j = 0; j < numVertices; j++)
                {
                    double angle = j * angleSpacing;
                    angle += (Math.PI / 2) * (i - 1);
                    double rawX = radiusB * Math.Cos(angle) + xMod[i];
                    double rawY = radiusA * Math.Sin(angle) + yMod[i];
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
            }
            //Add the first element to the back of the list to ensure the circle closes.
            if (vertices.Count > 0)
            {
                vertices.Add(new SKPointI(vertices[0].X, vertices[0].Y));
            }
            for (int i = 1; i < vertices.Count; i++)
            {
                SKPointI offsetStart = new SKPointI(vertices[i - 1].X + start.X, vertices[i - 1].Y + start.Y);
                SKPointI offsetEnd = new SKPointI(vertices[i].X + start.X, vertices[i].Y + start.Y);
                LineSegment l = new LineSegment(offsetStart, offsetEnd);
                if (!lines.Contains(l))
                {
                    lines.Add(l);
                }
            }
            return lines.ToArray();
        }
    }
}