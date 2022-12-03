using SkiaSharp;
using System;
using System.Collections.Generic;
using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    public class HexBackgroundLayer : BackgroundLayer
    {
        bool IDataLayer.DrawInExport => true;

        private bool RedrawRequired = true;

        public bool DrawCenterLines { get; set; } = false;
        public bool DrawGridLines { get; set; } = true;

        public bool DrawBackgroundImage { get; set; } = true;

        private SKBitmap OriginalBackgroundImage = null;
        private List<SKPoint[]> linesDrawn = new List<SKPoint[]>();
        public SKImageInfo BackgroundImageOriginalInfo { get; private set; }
        private SKBitmap LastImage = null;

        public HexBackgroundLayer()
        {
        }

        public bool SetBackgroundImage(string path)
        {
            if (path == null || path.Length == 0)
            {
                OriginalBackgroundImage = null;
                return true;
            }
            SKFileStream imageStream = new SKFileStream(path);
            if (!imageStream.IsValid)
            {
                return false;
            }

            OriginalBackgroundImage = SKBitmap.Decode(imageStream);
            BackgroundImageOriginalInfo = OriginalBackgroundImage.Info;
            if (OriginalBackgroundImage == null)
            {
                return false;
            }
            return true;
        }

        public bool ToggleCenterLines()
        {
            DrawCenterLines = !DrawCenterLines;
            ForceRedraw();
            return DrawCenterLines;
        }
        public bool ToggleGridLines()
        {
            DrawGridLines = !DrawGridLines;
            ForceRedraw();
            return DrawGridLines;
        }

        public bool ToggleBackgroundImage()
        {
            DrawBackgroundImage = !DrawBackgroundImage;
            ForceRedraw();
            return DrawBackgroundImage;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            if (!RedrawRequired)
            {
                return LastImage;
            }
            int canvasWidth = PageData.Instance.GetTotalWidth();
            int canvasHeight = PageData.Instance.GetTotalHeight();
            //Disposables

            SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
            SKCanvas drawingSurface = new SKCanvas(image);

            drawingSurface.Clear(ConfigOptions.Instance.BackgroundPaperColor);
            SKPaint gridBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = ConfigOptions.Instance.GridLinesColor, BlendMode = SKBlendMode.Src };
            gridBrush.IsAntialias = false;

            //Draw the background image within the border.
            if (OriginalBackgroundImage != null && DrawBackgroundImage)
            {
                SKImageInfo gridSize = new SKImageInfo(PageData.Instance.SquaresWide * PageData.Instance.SquareSize,
                                                       PageData.Instance.SquaresTall * PageData.Instance.SquareSize);
                SKBitmap backgroundImage = OriginalBackgroundImage.Resize(gridSize, SKFilterQuality.None);
                SKPaint alphaPaint = new SKPaint();
                alphaPaint.Color = alphaPaint.Color.WithAlpha(PageData.Instance.BackgroundImageAlpha);
                drawingSurface.DrawBitmap(backgroundImage, new SKPointI(PageData.Instance.MarginX, PageData.Instance.MarginY), alphaPaint);
                backgroundImage.Dispose();
            }

            int hexRad = PageData.Instance.SquareSize / 2;
            int quarterMarginX = PageData.Instance.MarginX / 4;
            int quarterMarginY = PageData.Instance.MarginY / 4;
            int maxMargin = Math.Max(PageData.Instance.MarginX, PageData.Instance.MarginY);
            int start = maxMargin / 2 + hexRad;

            float xInc = (float)(hexRad * 1.5);
            float yInc = (float)(hexRad * Math.Sqrt(3));
            if (DrawGridLines)
            {
                for (float y = start; IsOnPage(new SKPoint(start, y)); y += yInc)
                {
                    SKPoint centerPoint = new SKPoint(start, y);
                    while (IsOnPage(centerPoint))
                    {
                        DrawHex(centerPoint, drawingSurface, gridBrush);
                        centerPoint.X += xInc;
                        centerPoint.Y += yInc / 2;
                    }
                }
                for (float x = start; IsOnPage(new SKPoint(x, start)); x += xInc * 2)
                {
                    SKPoint centerPoint = new SKPoint(x, start);
                    while (IsOnPage(centerPoint))
                    {
                        DrawHex(centerPoint, drawingSurface, gridBrush);
                        centerPoint.X += xInc;
                        centerPoint.Y += yInc / 2;
                    }
                }
            }
            SKPaint borderBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = ConfigOptions.Instance.BorderLinesColor };
            SKRectI borderSquare = new SKRectI(quarterMarginX, quarterMarginY, PageData.Instance.GetTotalWidth() - quarterMarginX, PageData.Instance.GetTotalHeight() - quarterMarginY);
            drawingSurface.DrawRect(borderSquare, borderBrush);

            if (DrawCenterLines)
            {
                using (SKPaint centerBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = ConfigOptions.Instance.CenterLinesColor })
                {
                    int halfX = PageData.Instance.GetTotalWidth() / 2;
                    int halfY = PageData.Instance.GetTotalHeight() / 2;

                    drawingSurface.DrawLine(halfX, quarterMarginY, halfX, PageData.Instance.GetTotalHeight() - quarterMarginY, centerBrush);
                    drawingSurface.DrawLine(quarterMarginX, halfY, PageData.Instance.GetTotalWidth() - quarterMarginX, halfY, centerBrush);
                }
            }
            //Dispose of them.
            drawingSurface.Dispose();
            gridBrush.Dispose();
            borderBrush.Dispose();

            if (LastImage != null)
            {
                LastImage.Dispose();
            }
            LastImage = image;
            RedrawRequired = false;
            return LastImage;
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }

        public SKPointI GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }

        private void DrawHex(SKPoint centerPoint, SKCanvas drawingSurface, SKPaint brush)
        {
            SKPoint[] vertices = new SKPoint[6];
            int hexRad = PageData.Instance.SquareSize / 2;
            for (int i = 0; i < vertices.Length; i++)
            {
                int angleDeg = 60 * i;
                double angleRad = Math.PI / 180 * angleDeg;
                float xVert = (float)(centerPoint.X + (hexRad * Math.Cos(angleRad)));
                float yVert = (float)(centerPoint.Y + (hexRad * Math.Sin(angleRad)));
                vertices[i] = new SKPoint(xVert, yVert);
            }
            for (int i = 0; i < vertices.Length - 1; i++)
            {
                drawingSurface.DrawLine(vertices[i], vertices[i + 1], brush);
            }
            drawingSurface.DrawLine(vertices[5], vertices[0], brush);
        }

        private bool IsOnPage (SKPoint point)
        {
            int halfMarginX = PageData.Instance.MarginX / 2;
            int halfMarginY = PageData.Instance.MarginY / 2;
            if (point.X < halfMarginX || point.Y < halfMarginY)
            {
                return false;
            }
            if (point.X > (PageData.Instance.GetTotalWidth() - halfMarginX) || point.Y > (PageData.Instance.GetTotalHeight() - halfMarginY))
            {
                return false;
            }
            return true;
        }
    }
}
