﻿using System;
using System.Windows;
using System.Windows.Forms;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    public class CursorLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => false;

        public bool ClickDragActive { get; private set; } = false;
        private Point _canvasPoint;
        public Point CanvasPoint { get { return _canvasPoint; } set { _canvasPoint = ScaleCursorPoint(value); } }
        private Point ClickDragPoint = new Point(0, 0);

        public SKPointI CursorPoint { get; set; } = new SKPointI(0, 0);

        private SKBitmap LastImage;

        public CursorLayer()
        {
            CanvasPoint = new Point(0, 0);
        }

        /// <summary>
        /// Get the nearest grid point to the cursor's position. This function is used to convert a screen-related point to a grid-related point for the cursor, and its result is displayed in the status bar at the bottom of the screen.
        /// </summary>
        /// <returns>An SKPointI relative to the grid with a maximum of "PageData.Instance.SquaresWide" for X and "PageData.Instance.SquaresTall" for Y</returns>
        public SKPointI GetCursorGridPoints()
        {
            //Subtract out the margin.
            int cursorX = CursorPoint.X - PageData.Instance.MarginX;
            int cursorY = CursorPoint.Y - PageData.Instance.MarginY;
            return new SKPointI(cursorX / PageData.Instance.SquareSize, cursorY / PageData.Instance.SquareSize);
        }

        /// <summary>
        /// This method rounds a point on the main canvas to the main canvas grid such that the return value is equal to a multiple of "PageData.Instance.SquareSize + PageData.Instance.Margin"
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
            if ((x - PageData.Instance.MarginX < 0) || (y - PageData.Instance.MarginY < 0) || (x + PageData.Instance.MarginX > PageData.Instance.GetTotalWidth()) || (y + PageData.Instance.MarginY > PageData.Instance.GetTotalHeight()))
            {
                return CursorPoint;
            }

            //Subtract the margin out.
            int mouseX = x - PageData.Instance.MarginX;
            int mouseY = y - PageData.Instance.MarginY;

            //Round to the nearest intersection.
            int targetX = ((mouseX % PageData.Instance.SquareSize) < (PageData.Instance.SquareSize / 2)) ?
                    (mouseX - (mouseX % PageData.Instance.SquareSize)) :
                    (mouseX + (PageData.Instance.SquareSize - (mouseX % PageData.Instance.SquareSize)));
            int targetY = ((mouseY % PageData.Instance.SquareSize) < (PageData.Instance.SquareSize / 2)) ?
                    (mouseY - (mouseY % PageData.Instance.SquareSize)) :
                    (mouseY + (PageData.Instance.SquareSize - (mouseY % PageData.Instance.SquareSize)));

            //Add the margin back in.
            targetX += PageData.Instance.MarginX;
            targetY += PageData.Instance.MarginY;

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

        //If screen DPI scaling is active, we need to account for that in the cursor position.
        private Point ScaleCursorPoint(Point p)
        {
            float scale = (float)(Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth );
            if (scale != 1.00f)
            {
                Point rVal = new Point(p.X * scale, p.Y * scale);
                return rVal;
            }
            return p;
        }
    }
}
