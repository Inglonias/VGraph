using System;
using System.Text.Json.Serialization;
using System.Windows;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.objects
{
    public class LineSegment
    {
        public static int START = 0;
        public static int END = 1;

        [JsonIgnore]
        public readonly static double SELECT_RADIUS = 5;
        public SKPointI StartPointGrid { get; set; }
        public SKPointI EndPointGrid { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; } = false;

        public LineSegment()
        {

        }

        //public LineSegment(int x1, int y1, int x2, int y2)
        //{
        //    GenerateGridPoints(new SKPointI(x1, y1), new SKPointI(x2, y2));
        //}

        //public LineSegment(SKPointI startPoint, SKPointI endPoint, bool needToConvert)
        //{
        //    if (needToConvert)
        //    {
        //        GenerateGridPoints(startPoint, endPoint);
        //    }
        //    else
        //    {
        //        StartPointGrid = startPoint;
        //        EndPointGrid = endPoint;
        //    }
        //}

        public LineSegment(SKPointI startPoint, SKPointI endPoint)
        {
            StartPointGrid = startPoint;
            EndPointGrid = endPoint;
        }

        //In case I want to add the ability to zoom in and out, line coordinates will be stored as grid points instead of canvas coordinates.
        //That way, zooming in and out can be accomplished by simply changing the PageData's SquareSize value.
        //private void GenerateGridPoints(SKPointI start, SKPointI end)
        //{
        //    //Subtract out the margin.
        //    int startX = start.X - PageData.Instance.Margin;
        //    int startY = start.Y - PageData.Instance.Margin;
        //    int endX = end.X - PageData.Instance.Margin;
        //    int endY = end.Y - PageData.Instance.Margin;

        //    StartPointGrid = new SKPointI(startX / PageData.Instance.SquareSize, startY / PageData.Instance.SquareSize);
        //    EndPointGrid = new SKPointI(endX / PageData.Instance.SquareSize, endY / PageData.Instance.SquareSize);

        //    Console.WriteLine("Line created. Grid points are " + PrintCoords(StartPointGrid) + " and " + PrintCoords(EndPointGrid));
        //}

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

        public double GetLineLength()
        {
            return Math.Sqrt(Math.Pow(StartPointGrid.X - EndPointGrid.X, 2) + Math.Pow(StartPointGrid.Y - EndPointGrid.Y, 2));
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

        public LineSegment MergeLines(LineSegment target)
        {
            SKPointI endpointA;
            SKPointI endpointB;
            if (this.StartPointGrid == target.StartPointGrid)
            {
                endpointA = this.EndPointGrid;
                endpointB = target.EndPointGrid;
            }
            else if (this.EndPointGrid == target.StartPointGrid)
            {
                endpointA = this.StartPointGrid;
                endpointB = target.EndPointGrid;
            }
            else if (this.StartPointGrid == target.EndPointGrid)
            {
                endpointA = this.EndPointGrid;
                endpointB = target.StartPointGrid;
            }
            else if (this.EndPointGrid == target.EndPointGrid)
            {
                endpointA = this.StartPointGrid;
                endpointB = target.StartPointGrid;
            }
            else
            {
                return null;
            }
            double? slopeA = null;
            double? slopeB = null;

            try
            {
                double riseA = this.EndPointGrid.Y - this.StartPointGrid.Y;
                double runA = this.EndPointGrid.X - this.StartPointGrid.X;
                slopeA = riseA / runA;
            }
            catch (DivideByZeroException) { }
            try
            {
                double riseB = target.EndPointGrid.Y - target.StartPointGrid.Y;
                double runB = target.EndPointGrid.X - target.StartPointGrid.X;
                slopeB = riseB / runB;
            }
            catch (DivideByZeroException) { }

            if (slopeA == null ^ slopeB == null)
            {
                return null;
            }

            if ((slopeA == null && slopeB == null) || (slopeA == slopeB))
            {
                return new LineSegment(endpointA, endpointB);
            }

            return null;
        }

        public override bool Equals(object o)
        {
            if ((o == null) || !this.GetType().Equals(o.GetType()))
            {
                return false;
            }
            LineSegment other = (LineSegment)o;
            return this.StartPointGrid.Equals(other.StartPointGrid) && this.EndPointGrid.Equals(other.EndPointGrid);
        }

        //private string PrintCoords(SKPointI p)
        //{
        //    return "(" + p.X + ", " + p.Y + ")";
        //}
    }
}
