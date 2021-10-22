using System;
using System.Collections.Generic;
using System.Windows;
using SkiaSharp;

using VGraph.src.config;
using System.Text.Json.Serialization;

namespace VGraph.src.dataLayers
{
    public class LineLayer : IDataLayer
    {
        public List<LineSegment> LineList { get; } = new List<LineSegment>();
        private SKBitmap LastBitmap;
        private bool RedrawRequired;
        public bool PreviewPointActive = false;

        private const int START = 0;
        private const int END = 1;

        private SKPointI PreviewPoint;

        public class LineSegment
        {
            [JsonIgnore]
            public readonly static double SELECT_RADIUS = 5;
            public SKPointI StartPointGrid { get; set; }
            public SKPointI EndPointGrid { get; set; }
            [JsonIgnore]
            public bool IsSelected { get; set; } = false;

            public LineSegment()
            {

            }

            public LineSegment(int x1, int y1, int x2, int y2)
            {
                GenerateGridPoints(new SKPointI(x1, y1), new SKPointI(x2, y2));
            }

            public LineSegment(SKPointI startPoint, SKPointI endPoint, bool needToConvert)
            {
                if (needToConvert)
                {
                    GenerateGridPoints(startPoint, endPoint);
                }
                else
                {
                    StartPointGrid = startPoint;
                    EndPointGrid = endPoint;
                }
            }

                //In case I want to add the ability to zoom in and out, line coordinates will be stored as grid points instead of canvas coordinates.
                //That way, zooming in and out can be accomplished by simply changing the PageData's SquareSize value.
            private void GenerateGridPoints(SKPointI start, SKPointI end)
            {
                //Subtract out the margin.
                int startX = start.X - PageData.Instance.Margin;
                int startY = start.Y - PageData.Instance.Margin;
                int endX = end.X - PageData.Instance.Margin;
                int endY = end.Y - PageData.Instance.Margin;

                StartPointGrid = new SKPointI(startX / PageData.Instance.SquareSize, startY / PageData.Instance.SquareSize);
                EndPointGrid = new SKPointI(endX / PageData.Instance.SquareSize, endY / PageData.Instance.SquareSize);

                Console.WriteLine("Line created. Grid points are " + PrintCoords(StartPointGrid) + " and " + PrintCoords(EndPointGrid));
            }

            public SKPointI[] GetCanvasPoints()
            {
                SKPointI[] rVal = new SKPointI[2];

                int startX = (StartPointGrid.X * PageData.Instance.SquareSize) + PageData.Instance.Margin;
                int startY = (StartPointGrid.Y * PageData.Instance.SquareSize) + PageData.Instance.Margin;
                int endX = (EndPointGrid.X * PageData.Instance.SquareSize) + PageData.Instance.Margin;
                int endY = (EndPointGrid.Y * PageData.Instance.SquareSize) + PageData.Instance.Margin;

                rVal[START] = new SKPointI(startX, startY);
                rVal[END] = new SKPointI(endX, endY);

                return rVal;
            }

            /// <summary>
            /// Uses Heron's formula to determine minimum Euclidian distance from the provided point to the line segment
            /// </summary>
            /// <param name="pointC">Canvas point used to measure the distance to the line segment</param>
            /// <returns>Minimum Euclidian distance to the line segment</returns>
            public double LinePointDistance(Point pointC)
            {
                SKPointI[] canvasPoints = GetCanvasPoints();        

                Point pointA = new Point(canvasPoints[START].X, canvasPoints[START].Y);
                Point pointB = new Point(canvasPoints[END].X, canvasPoints[END].Y);

                //Heron's formula. This is one I had not heard of before!
                double abX = Math.Abs(pointB.X - pointA.X);
                double abY = Math.Abs(pointB.Y - pointA.Y);
                double abLength = Math.Sqrt(Math.Pow(abX, 2) + Math.Pow(abY, 2));

                double acX = Math.Abs(pointC.X - pointA.X);
                double acY = Math.Abs(pointC.Y - pointA.Y);
                double acLength = Math.Sqrt(Math.Pow(acX, 2) + Math.Pow(acY, 2));

                double bcX = Math.Abs(pointC.X - pointB.X);
                double bcY = Math.Abs(pointC.Y - pointB.Y);
                double bcLength = Math.Sqrt(Math.Pow(bcX, 2) + Math.Pow(bcY, 2));

                double semiPerim = (abLength + acLength + bcLength) / 2;

                //Apparently this derivation is unstable if we click too close to the line. Darn.
                double area = Math.Sqrt(semiPerim * (semiPerim - abLength) * (semiPerim - acLength) * (semiPerim - bcLength));

                double distance = 2 * area / abLength;

                return distance;
            }

            public bool WasLineSelected(double dist, Point clickPoint)
            {
                //Sanity check for co-linear clicks.
                SKPointI[] endpoints = GetCanvasPoints();
                double minX = Math.Min(endpoints[START].X, endpoints[END].X) - SELECT_RADIUS;
                double maxX = Math.Max(endpoints[START].X, endpoints[END].X) + SELECT_RADIUS;
                double minY = Math.Min(endpoints[START].Y, endpoints[END].Y) - SELECT_RADIUS;
                double maxY = Math.Max(endpoints[START].Y, endpoints[END].Y) + SELECT_RADIUS;

                bool rVal;
                if ((clickPoint.X < minX) || (clickPoint.Y < minY) || (clickPoint.X > maxX) || (clickPoint.Y > maxY))
                {
                    rVal = false;
                    return rVal;
                }
                rVal = dist < SELECT_RADIUS;
                return rVal;
            }

            public bool WasLineSelected(SKRect boundingBox)
            {
                SKPointI[] endpoints = GetCanvasPoints();
                return (boundingBox.Contains(endpoints[START]) && boundingBox.Contains(endpoints[END]));
            }

            private string PrintCoords(SKPointI p)
            {
                return "(" + p.X + ", " + p.Y + ")";
            }
        }

        public LineLayer()
        {
        }

        public void AddNewLine(SKPointI start, SKPointI end)
        {
            AddNewLine(start, end, true);
        }

        public void AddNewLine(SKPointI start, SKPointI end, bool needToConvert)
        {
            if (start.Equals(end))
            {
                return;
            }
            LineList.Add(new LineSegment(start, end, needToConvert));
            ForceRedraw();
        }

        public void ClearAllLines()
        {
            LineList.Clear();
            ForceRedraw();
        }

        public void DeselectLines()
        {
            foreach (LineSegment l in LineList)
            {
                l.IsSelected = false;
            }
            ForceRedraw();
        }

        public void DeleteSelectedLines()
        {
            for (int i = LineList.Count - 1; i >= 0; i--)
            {
                if (LineList[i].IsSelected)
                {
                    LineList.RemoveAt(i);
                    ForceRedraw();
                }
            }
        }

        public void MoveSelectedLines(int x, int y)
        {
            foreach (LineSegment l in LineList)
            {
                if (l.IsSelected)
                {
                    int targetStartX = l.StartPointGrid.X + x;
                    int targetStartY = l.StartPointGrid.Y + y;
                    int targetEndX = l.EndPointGrid.X + x;
                    int targetEndY = l.EndPointGrid.Y + y;

                    bool moveValid = (Math.Min(targetStartX, targetEndX) >= 0) &&
                                (Math.Min(targetStartY, targetEndY) >= 0) &&
                                (Math.Max(targetStartX, targetEndX) <= PageData.Instance.SquaresWide) &&
                                (Math.Max(targetStartY, targetEndY) <= PageData.Instance.SquaresTall);

                    if (moveValid)
                    {
                        l.StartPointGrid = new SKPointI(l.StartPointGrid.X + x, l.StartPointGrid.Y + y);
                        l.EndPointGrid = new SKPointI(l.EndPointGrid.X + x, l.EndPointGrid.Y + y);
                    }
                    
                }
            }
            ForceRedraw();
        }

        public void HandleSelectionClick(Point point)
        {
            DeselectLines();
            ForceRedraw();
            for (int i = 0; i < LineList.Count; i++)
            {
                double dist = LineList[i].LinePointDistance(point);
                if (dist < LineSegment.SELECT_RADIUS)
                {
                    if (LineList[i].WasLineSelected(dist, point))
                    {
                        LineList[i].IsSelected = true;
                        return;
                    }
                }
            }
        }

        public void HandleBoxSelect(SKRect boundingBox)
        {
            DeselectLines();
            ForceRedraw();
            foreach (LineSegment l in LineList)
            {
                l.IsSelected = false;
                if (l.WasLineSelected(boundingBox))
                {
                    l.IsSelected = true;
                }
            }
        }

        public void HandleCreationClick(SKPointI point)
        {
            if (PreviewPointActive)
            {
                AddNewLine(PreviewPoint, point);
                PreviewPointActive = false;
            }
            else
            {
                PreviewPointActive = true;
                PreviewPoint = point;
            }
            ForceRedraw();
        }

        public SKBitmap GenerateLayerBitmap()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            if (LastBitmap == null || RedrawRequired)
            {
                RedrawRequired = false;
                SKBitmap bitmap = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

                //Disposables
                SKCanvas canvas = new SKCanvas(bitmap);

                SKPaint previewBrush = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Green };
                SKPaint selectedBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Red, IsAntialias = true };
                SKPaint standardBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Blue, IsAntialias = true };

                foreach (LineSegment line in LineList)
                {
                    SKPointI[] canvasPoints = line.GetCanvasPoints();
                    if (line.IsSelected)
                    {
                        canvas.DrawLine(canvasPoints[START], canvasPoints[END], selectedBrush);
                    }
                    else
                    {
                        canvas.DrawLine(canvasPoints[START], canvasPoints[END], standardBrush);
                    }
                }

                if (PreviewPointActive)
                {
                    int previewRadius = Math.Max(1, drawRadius);
                    canvas.DrawCircle(PreviewPoint, previewRadius, previewBrush);
                }

                //Dispose of them.
                canvas.Dispose();
                previewBrush.Dispose();
                selectedBrush.Dispose();
                standardBrush.Dispose();

                if (LastBitmap != null)
                {
                    LastBitmap.Dispose();
                }
                RedrawRequired = false;
                LastBitmap = bitmap;
            }

            return LastBitmap;
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }

        public SKPoint GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }
    }
}
