using System;
using System.Windows;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    internal class CursorLayer : IDataLayer
    {
        public bool ClickDragActive { get; private set; } = false;
        public Point CanvasPoint { get; set; } = new Point(0, 0);
        private Point ClickDragPoint = new Point(0, 0);

        public SKPointI CursorPoint { get; set; } = new SKPointI(0, 0);
        private SKBitmap Bitmap;

        private const int RADIUS = 4;

        public CursorLayer()
        {
        }

        /// <summary>
        /// This method rounds a point on the main canvas to the main canvas grid such that the return value is equal to a multiple of PageData.Instance.SquareSize + PageData.Instance.Margin
        /// </summary>
        /// <param name="p">Point to round off</param>
        /// <returns></returns>
        public SKPointI RoundToNearestIntersection(Point p)
        {
            return RoundToNearestIntersection(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        private SKPointI RoundToNearestIntersection(int x, int y)
        {
            //Ignore it if we're out of bounds.
            if ((x - PageData.Instance.Margin < 0) || (y - PageData.Instance.Margin < 0) || (x + PageData.Instance.Margin > PageData.Instance.GetTotalWidth()) || (y + PageData.Instance.Margin > PageData.Instance.GetTotalHeight()))
            {
                return CursorPoint;
            }

            //Subtract the margin out.
            int mouseX = x - PageData.Instance.Margin;
            int mouseY = y - PageData.Instance.Margin;

            //Round to the nearest intersection.
            int targetX = ((mouseX % PageData.Instance.SquareSize) < (PageData.Instance.SquareSize / 2)) ?
                    (mouseX - (mouseX % PageData.Instance.SquareSize)) :
                    (mouseX + (PageData.Instance.SquareSize - (mouseX % PageData.Instance.SquareSize)));
            int targetY = ((mouseY % PageData.Instance.SquareSize) < (PageData.Instance.SquareSize / 2)) ?
                    (mouseY - (mouseY % PageData.Instance.SquareSize)) :
                    (mouseY + (PageData.Instance.SquareSize - (mouseY % PageData.Instance.SquareSize)));

            //Add the margin back in.
            targetX += PageData.Instance.Margin;
            targetY += PageData.Instance.Margin;

            return new SKPointI(targetX, targetY);
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
            }
            return SKRect.Empty;
        }

        public SKPoint GetRenderPoint()
        {
            if (ClickDragActive)
            {
                float renderX = Convert.ToSingle(Math.Min(CanvasPoint.X, ClickDragPoint.X));
                float renderY = Convert.ToSingle(Math.Min(CanvasPoint.Y, ClickDragPoint.Y));
                return new SKPoint(renderX, renderY);
            }
            return new SKPointI(CursorPoint.X - RADIUS, CursorPoint.Y - RADIUS);
        }

        public SKBitmap GenerateLayerBitmap()
        {
            //This code is commented out as a monument to my own stupidity. This canvas ABSOLUTELY DID NOT need to be this big.
            //SKBitmap replaceBitmap = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

            int canvasWidth = RADIUS * 2;
            int canvasHeight = RADIUS * 2;

            if (ClickDragActive)
            {
                if (ClickDragPoint == null)
                {
                    ClickDragPoint = CanvasPoint;
                    Console.WriteLine("ClickDrag set!");
                }
                canvasWidth = Convert.ToInt32(Math.Abs(ClickDragPoint.X - CanvasPoint.X));
                canvasHeight = Convert.ToInt32(Math.Abs(ClickDragPoint.Y - CanvasPoint.Y));
            }

            Bitmap = new SKBitmap(canvasWidth, canvasHeight);

            //Disposables
            SKCanvas canvas = new SKCanvas(Bitmap);
            SKPaint brush;
            if (ClickDragActive)
            {
                float strokeSize = Math.Max(RADIUS / 2, 1);
                brush = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = strokeSize };
                float right  = Convert.ToSingle(Math.Abs(ClickDragPoint.X - CanvasPoint.X));
                float bottom = Convert.ToSingle(Math.Abs(ClickDragPoint.Y - CanvasPoint.Y));

                SKRect rect = new SKRect(strokeSize, strokeSize, right - Math.Max(RADIUS / 2, 1), bottom - Math.Max(RADIUS / 2, 1));
                canvas.DrawRect(rect, brush);
            }
            else
            {
               brush = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Red };
               canvas.DrawCircle(new SKPointI(RADIUS, RADIUS), RADIUS, brush);
            }

            //Dispose of them.
            canvas.Dispose();
            brush.Dispose();
            
            return Bitmap;
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