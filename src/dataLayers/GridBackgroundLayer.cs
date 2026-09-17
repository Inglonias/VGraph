using SkiaSharp;

using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    public class GridBackgroundLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => true;

        private bool RedrawRequired = true;

        public bool DrawCenterLines { get; set; } = false;
        public bool DrawGridLines { get; set; } = true;

        public bool DrawBackgroundImage { get; set; } = true;

        private SKBitmap OriginalBackgroundImage = null;

        public SKImageInfo BackgroundImageOriginalInfo { get; private set; }
        private SKBitmap LastImage = null;

        public GridBackgroundLayer()
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
            SKPaint gridBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = ConfigOptions.Instance.GridLinesColor };

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

            if (DrawGridLines)
            {
                for (int x = 0; x <= PageData.Instance.SquaresWide; x++)
                {
                    int xStart = (x * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
                    int yStart = PageData.Instance.MarginY;
                    int yEnd = PageData.Instance.GetTotalHeight() - PageData.Instance.MarginY;
                    drawingSurface.DrawLine(new SKPointI(xStart, yStart), new SKPointI(xStart, yEnd), gridBrush);
                }
                for (int y = 0; y <= PageData.Instance.SquaresTall; y++)
                {
                    int xStart = PageData.Instance.MarginX;
                    int yStart = (y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;
                    int xEnd = PageData.Instance.GetTotalWidth() - PageData.Instance.MarginX;
                    drawingSurface.DrawLine(new SKPointI(xStart, yStart), new SKPointI(xEnd, yStart), gridBrush);
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
