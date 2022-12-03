using System;
using System.Windows;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    public class CursorLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => false;

        public bool ClickDragActive { get; private set; } = false;
        public Point CanvasPoint { get; set; } = new Point(0, 0);
        private Point ClickDragPoint = new Point(0, 0);

        public SKPointI CursorPoint { get; set; } = new SKPointI(0, 0);

        private SKBitmap LastImage;

        public CursorLayer()
        {
        }

        /// <summary>
        /// Get the nearest grid point to the cursor's position. This function is used to convert a screen-related point to a grid-related point for the cursor, and its result is displayed in the status bar at the bottom of the screen.
        /// </summary>
        /// <returns>An SKPointI relative to the grid with a maximum of "PageData.Instance.SquaresWide" for X and "PageData.Instance.SquaresTall" for Y</returns>
        public SKPointI GetCursorGridPoints()
        {
            return PixelToHex(CursorPoint.X, CursorPoint.Y);
        }

        /// <summary>
        /// This method rounds a point on the main canvas to the main canvas grid such that the return value is equal to a multiple of "PageData.Instance.SquareSize + PageData.Instance.Margin"
        /// </summary>
        /// <param name="p">Point to round off</param>
        /// <returns></returns>
        public SKPointI RoundToNearestHex(Point p)
        {
            return RoundToNearestHex(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        private SKPointI RoundToNearestHex(int x, int y)
        {
            //Ignore it if we're out of bounds.
            if ((x - PageData.Instance.MarginX < 0) || (y - PageData.Instance.MarginY < 0) || (x + PageData.Instance.MarginX > PageData.Instance.GetTotalWidth()) || (y + PageData.Instance.MarginY > PageData.Instance.GetTotalHeight()))
            {
                return CursorPoint;
            }
            SKPointI gridPoint = PixelToHex(x, y);
            SKPoint pixelPoint = HexToPixel(gridPoint);
            return new SKPointI(Convert.ToInt32(pixelPoint.X), Convert.ToInt32(pixelPoint.Y));
        }

        public SKPointI RoundToNearestVertex(Point p)
        {
            return RoundToNearestVertex(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        public SKPointI RoundToNearestVertex(int x, int y)
        {
            SKPointI cursorPoint = new SKPointI(x, y);
            SKPointI gridPoint = PixelToHex(x, y);
            SKPoint hexCenter = HexToPixel(gridPoint);
            SKPoint[] vertices = new SKPoint[6];
            int hexRad = PageData.Instance.SquareSize / 2;
            float minDistance = hexRad * 2;
            SKPointI rVal = new SKPointI(0, 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                int angleDeg = 60 * i;
                double angleRad = Math.PI / 180 * angleDeg;
                float xVert = (float)(hexCenter.X + (hexRad * Math.Cos(angleRad)));
                float yVert = (float)(hexCenter.Y + (hexRad * Math.Sin(angleRad)));
                vertices[i] = new SKPoint(xVert, yVert);
                float distance = SKPoint.Distance(cursorPoint, vertices[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    rVal = new SKPointI(Convert.ToInt32(vertices[i].X), Convert.ToInt32(vertices[i].Y));
                }
            }
            return rVal;
        }

        public SKPointI PixelToHex(SKPoint p)
        {
            return PixelToHex(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        public SKPointI PixelToHex(int x, int y)
        {
            int gridX = x - PageData.Instance.MarginX;
            int gridY = y - PageData.Instance.MarginY;
            int q = Convert.ToInt32(Math.Round((2.0 / 3 * gridX) / (PageData.Instance.SquareSize / 2)));
            int r = Convert.ToInt32(Math.Round((-1.0 / 3 * gridX + Math.Sqrt(3) / 3 * gridY) / (PageData.Instance.SquareSize / 2)));

            return new SKPointI(q, r);
        }

        public SKPoint HexToPixel(SKPointI p)
        {
            return HexToPixel(p.X, p.Y);
        }

        public SKPoint HexToPixel(int q, int r)
        {
            int hexRad = PageData.Instance.SquareSize / 2;
            int maxMargin = Math.Max(PageData.Instance.MarginX, PageData.Instance.MarginY);
            int start = maxMargin / 2 + hexRad;

            float xInc = (float)(hexRad * 1.5);
            float yInc = (float)(hexRad * Math.Sqrt(3));

            //Q value
            float x = start + (xInc * q);

            //R value
            //x += (xInc * r) ;
            float y = start + (yInc * r) + (yInc * q / 2);
            return new SKPoint(x, y);
        }

        public void StartClickDrag()
        {
            if (!ClickDragActive)
            {
                ClickDragActive = true;
                ClickDragPoint = CanvasPoint;
            }
        }

        public SKRect StopClickDrag()
        {
            if (ClickDragActive)
            {
                ClickDragActive = false;

                float left = Convert.ToSingle(Math.Min(CanvasPoint.X, ClickDragPoint.X));
                float right = Convert.ToSingle(Math.Max(CanvasPoint.X, ClickDragPoint.X));
                float top = Convert.ToSingle(Math.Min(CanvasPoint.Y, ClickDragPoint.Y));
                float bottom = Convert.ToSingle(Math.Max(CanvasPoint.Y, ClickDragPoint.Y));

                SKRect rVal = new SKRect(left, top, right, bottom);
                return rVal;
            }
            return SKRect.Empty;
        }

        public SKPointI GetRenderPoint()
        {
            if (ClickDragActive)
            {
                int renderX = Convert.ToInt32(Math.Min(CanvasPoint.X, ClickDragPoint.X));
                int renderY = Convert.ToInt32(Math.Min(CanvasPoint.Y, ClickDragPoint.Y));
                return new SKPointI(renderX, renderY);
            }
            int radius = Math.Max(1, PageData.Instance.SquareSize / 6);
            return new SKPointI(CursorPoint.X - radius, CursorPoint.Y - radius);
        }

        public SKBitmap GenerateLayerBitmap()
        {
            //This code is commented out as a monument to my own stupidity. This canvas ABSOLUTELY DID NOT need to be this big.
            //SKBitmap replaceBitmap = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

            int radius = Math.Max(1, PageData.Instance.SquareSize / 6);
            int canvasWidth = radius * 2;
            int canvasHeight = radius * 2;

            if (ClickDragActive)
            {
                canvasWidth = Math.Max(1, Convert.ToInt32(Math.Abs(ClickDragPoint.X - CanvasPoint.X)));
                canvasHeight = Math.Max(1, Convert.ToInt32(Math.Abs(ClickDragPoint.Y - CanvasPoint.Y)));
            }
            //Disposables
            SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
            SKCanvas drawingSurface = new SKCanvas(image);
            SKPaint brush;

            if (ClickDragActive)
            {
                float strokeSize = Math.Max(radius / 2, 1);
                brush = new SKPaint { Style = SKPaintStyle.Stroke, Color = ConfigOptions.Instance.SelectionBoxColor, StrokeWidth = strokeSize };
                float right = Convert.ToSingle(Math.Abs(ClickDragPoint.X - CanvasPoint.X));
                float bottom = Convert.ToSingle(Math.Abs(ClickDragPoint.Y - CanvasPoint.Y));

                SKRect rect = new SKRect(strokeSize, strokeSize, right - Math.Max(radius / 2, 1), bottom - Math.Max(radius / 2, 1));
                drawingSurface.DrawRect(rect, brush);
            }
            else
            {
                brush = new SKPaint { Style = SKPaintStyle.Fill, Color = ConfigOptions.Instance.CursorColor };
                drawingSurface.DrawCircle(new SKPointI(radius, radius), radius, brush);
            }

            //Dispose of them.
            drawingSurface.Dispose();
            brush.Dispose();
            if (LastImage != null)
            {
                LastImage.Dispose();
            }
            LastImage = image;

            return LastImage;
        }

        public bool IsRedrawRequired()
        {
            return true;
        }

        public void ForceRedraw()
        {
        }
    }
}
