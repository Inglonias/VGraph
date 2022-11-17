using SkiaSharp;
using System.Collections.Generic;
using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    public class HexBackgroundLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => true;

        private bool RedrawRequired = true;

        public bool DrawCenterLines { get; set; } = false;
        public bool DrawGridLines { get; set; } = true;

        public bool DrawBackgroundImage { get; set; } = true;

        private SKBitmap OriginalBackgroundImage = null;

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

        private SKPath CreateLinePath(SKPoint a, SKPoint b)
        {
            SKPath rVal = new SKPath();
            rVal.MoveTo(a);
            rVal.LineTo(b);
            return rVal;
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
            SKPaint gridBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = ConfigOptions.Instance.GridLinesColor };
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

            int triSideLen = PageData.Instance.SquareSize;
            float triHeight = (float)(triSideLen * System.Math.Sqrt(3) / 2);
            float xFactor = (float)(triHeight * System.Math.Cos(System.Math.PI / 3));
            if (DrawGridLines)
            {
                float xStart = PageData.Instance.MarginX / 2;
                float yStartHigh = PageData.Instance.MarginY / 2;
                float yStartLow = (PageData.Instance.MarginY / 2) + triHeight;
                float gridMaxX = PageData.Instance.GetTotalWidth() - xStart;
                float gridMaxY = PageData.Instance.GetTotalHeight() - yStartHigh;
                float xInc = xFactor + triSideLen;
                float yInc = triHeight * 2;
                //Draw in the shape below for a hex grid without overlap.

                /*        __/
                 *          \
                 */
                bool downColumn = false;
                HashSet<SKPath> drawnLines = new HashSet<SKPath>();
                for (float x = xStart; x < gridMaxX; x += xInc)
                {
                    float y = downColumn ? yStartLow : yStartHigh;
                    for (; y < gridMaxY; y += yInc)
                    {
                        SKPoint centerPoint       = new SKPoint(x, y);
                        SKPoint leftPoint         = new SKPoint(x - triSideLen, y);
                        SKPoint topRightCorner    = new SKPoint(x + xFactor, y - triHeight);
                        SKPoint bottomRightCorner = new SKPoint(x + xFactor, y + triHeight);

                        List<SKPath> currentLines = new List<SKPath>(3);
                        currentLines.Add(CreateLinePath(leftPoint, centerPoint));
                        currentLines.Add(CreateLinePath(centerPoint, topRightCorner));
                        currentLines.Add(CreateLinePath(centerPoint, bottomRightCorner));

                        foreach (SKPath p in currentLines) {
                            if (!drawnLines.Contains(p))
                            {
                                drawingSurface.DrawPath(p, gridBrush);
                                drawnLines.Add(p);
                            }
                        }
                    }
                    downColumn = !downColumn;
                }
            }

            int quarterMarginX = PageData.Instance.MarginX / 4;
            int quarterMarginY = PageData.Instance.MarginY / 4;
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
    }
}
