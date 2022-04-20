using System;
using System.Text.Json.Serialization;
using System.Windows;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.objects
{
    public class LineSegment
    {
        public static readonly int START = 0;
        public static readonly int END = 1;
        public static readonly SKColor DEFAULT_COLOR = ConfigOptions.Instance.DefaultLineColor;

        [JsonIgnore]
        public readonly static double SELECT_RADIUS = 5;
        public SKPointI StartPointGrid { get; set; }
        public SKPointI EndPointGrid { get; set; }
        public string LineColor { get; set; } //Stored as #AARRGGBB due to serialization issues with SKColor
        [JsonIgnore]
        public bool IsSelected { get; set; } = false;

        public LineSegment()
        {
            LineColor = DEFAULT_COLOR.ToString();
        }

        public LineSegment(SKPointI startPoint, SKPointI endPoint)
        {
            StartPointGrid = startPoint;
            EndPointGrid = endPoint;
            LineColor = PageData.Instance.CurrentLineColor.ToString();
        }

        public LineSegment(SKPointI startPoint, SKPointI endPoint, string lineColor)
        {
            StartPointGrid = startPoint;
            EndPointGrid = endPoint;
            LineColor = lineColor;
        }

        /// <summary>
        /// Converts the grid-relative coordinates of the line segment to canvas-relative coordinates for rendering.
        /// </summary>
        /// <returns>An array containing two "SKPointI"s representing the start and end points of the line segment.</returns>
        public SKPointI[] GetCanvasPoints()
        {
            SKPointI[] rVal = new SKPointI[2];

            int startX = (StartPointGrid.X * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
            int startY = (StartPointGrid.Y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;
            int endX = (EndPointGrid.X * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
            int endY = (EndPointGrid.Y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;

            rVal[START] = new SKPointI(startX, startY);
            rVal[END] = new SKPointI(endX, endY);

            return rVal;
        }

        /// <summary>
        /// Uses Heron's formula to determine the height of a triangle with the line segment as the base. This function is mainly used to determine if a line was clicked on.
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
        
        /// <summary>
        /// Calculates the Euclidian length of the line segment.
        /// </summary>
        /// <returns>A "double" containing the line length.</returns>
        public double GetLineLength()
        {
            return Math.Sqrt(Math.Pow(StartPointGrid.X - EndPointGrid.X, 2) + Math.Pow(StartPointGrid.Y - EndPointGrid.Y, 2));
        }
        
        /// <summary>
        /// Determines whether "clickPoint" was intended to select a line.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool WasLineSelected(SKRect boundingBox)
        {
            SKPointI[] endpoints = GetCanvasPoints();
            return (boundingBox.Contains(endpoints[START]) && boundingBox.Contains(endpoints[END]));
        }
        
        /// <summary>
        /// Attempts to merge this line segment with "target". A merge is only successful if the two lines are touching at an endpoint and have the same slope.
        /// </summary>
        /// <param name="target">The other line segment to attempt to merge with.</param>
        /// <returns>The merged line segment if the merge was successful. Null otherwise.</returns>
        public LineSegment MergeLines(LineSegment target)
        {
            SKPointI endpointA;
            SKPointI endpointB;
            if (!LineColor.Equals(target.LineColor)) {
                return null;
            }
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
            double slopeA;
            double slopeB;

            double riseA = this.EndPointGrid.Y - this.StartPointGrid.Y;
            double runA = this.EndPointGrid.X - this.StartPointGrid.X;
            slopeA = riseA / runA;

            double riseB = target.EndPointGrid.Y - target.StartPointGrid.Y;
            double runB = target.EndPointGrid.X - target.StartPointGrid.X;
            slopeB = riseB / runB;

            if (double.IsInfinity(slopeA) ^ double.IsInfinity(slopeB))
            {
                return null;
            }
            if ((double.IsInfinity(slopeA) && double.IsInfinity(slopeB)) || (slopeA == slopeB))
            {
                return new LineSegment(endpointA, endpointB, LineColor);
            }

            return null;
        }

        public LineSegment MirrorLineSegment(int? xCrease, int? yCrease)
        {
            if (xCrease == null && yCrease == null)
            {
                throw new FormatException("Lines cannot be mirrored across nothing");
            }
            SKPointI mirrorStart = new SKPointI(StartPointGrid.X, StartPointGrid.Y);
            SKPointI mirrorEnd = new SKPointI(EndPointGrid.X, EndPointGrid.Y);

            if (xCrease.HasValue)
            {
                int startDistance = xCrease.Value - mirrorEnd.X;
                int endDistance = xCrease.Value - mirrorStart.X;
                mirrorStart.X = endDistance + xCrease.Value;
                mirrorEnd.X = startDistance + xCrease.Value;
            }

            if (yCrease.HasValue)
            {
                int startDistance = yCrease.Value - mirrorEnd.Y;
                int endDistance = yCrease.Value - mirrorStart.Y;
                mirrorStart.Y = endDistance + yCrease.Value;
                mirrorEnd.Y = startDistance + yCrease.Value;
            }

            return new LineSegment(mirrorStart, mirrorEnd, LineColor);
        }

        //public SKColor GetInvertedLineColor()
        //{
        //    byte red = Convert.ToByte(255 - LineColor.Red);
        //    byte grn = Convert.ToByte(255 - LineColor.Green);
        //    byte blu = Convert.ToByte(255 - LineColor.Blue);
        //    return new SKColor(red, grn, blu);
        //}

        public override bool Equals(object o)
        {
            if ((o == null) || !this.GetType().Equals(o.GetType()))
            {
                return false;
            }
            LineSegment other = (LineSegment)o;
            return (this.StartPointGrid.Equals(other.StartPointGrid) && this.EndPointGrid.Equals(other.EndPointGrid)) ||
                   (this.StartPointGrid.Equals(other.EndPointGrid) && this.EndPointGrid.Equals(other.StartPointGrid));
        }
    }
}
