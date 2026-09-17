using SkiaSharp;
using System;
using VGraph.Config;

namespace VGraph.DataLayers
{
    public class GridBackgroundLayer : IDataLayer
    {
        private bool _redrawRequired = true;
        public bool DrawCenterLines { get; set; } = false;
        public bool DrawGridLines { get; set; } = true;

        public bool DrawBackgroundImage { get; set; } = true;
        private SKImage? _originalBackgroundImage = null;
        private SKImage? _lastImage;
        SKImage? IDataLayer.LastImage => _lastImage;
        public SKImageInfo BackgroundImageOriginalInfo { get; private set; }


        bool IDataLayer.DrawInExport => true;

        public GridBackgroundLayer()
        {
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

        public void ForceRedraw()
        {
            _redrawRequired = true;
        }

        public bool SetBackgroundImage(string path)
        {
            if (path == null || path.Length == 0)
            {
                _originalBackgroundImage = null;
                return true;
            }
            SKFileStream imageStream = new SKFileStream(path);
            if (!imageStream.IsValid)
            {
                return false;
            }

            _originalBackgroundImage = SKImage.FromBitmap(SKBitmap.Decode(imageStream));
            BackgroundImageOriginalInfo = _originalBackgroundImage.Info;
            if (_originalBackgroundImage == null)
            {
                return false;
            }
            return true;
        }

        public SKPointI GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }

        public bool IsRedrawRequired()
        {
            return _redrawRequired;
        }

        public SKImage? GenerateLayerImage()
        {
            if (!_redrawRequired)
            {
                return _lastImage;
            }
            int canvasWidth = PageData.Instance.GetTotalWidth();
            int canvasHeight = PageData.Instance.GetTotalHeight();
            SKSurface drawingSurface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
            var drawingCanvas = drawingSurface.Canvas;

            drawingCanvas.Clear(ConfigOptions.Instance.BackgroundPaperColor);
            SKPaint gridBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = ConfigOptions.Instance.GridLinesColor };

            //Draw the background image within the border.
            if (_originalBackgroundImage != null && DrawBackgroundImage)
            {
                SKRect gridSize = SKRect.Create(new SKSize(PageData.Instance.SquaresWide * PageData.Instance.SquareSize, PageData.Instance.SquaresTall * PageData.Instance.SquareSize));
                SKPaint alphaPaint = new SKPaint();
                alphaPaint.Color = alphaPaint.Color.WithAlpha(PageData.Instance.BackgroundImageAlpha);
                drawingCanvas.DrawImage(_originalBackgroundImage, gridSize, alphaPaint);
            }

            if (DrawGridLines)
            {
                for (int x = 0; x <= PageData.Instance.SquaresWide; x++)
                {
                    int xStart = (x * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
                    int yStart = PageData.Instance.MarginY;
                    int yEnd = PageData.Instance.GetTotalHeight() - PageData.Instance.MarginY;
                    drawingCanvas.DrawLine(new SKPointI(xStart, yStart), new SKPointI(xStart, yEnd), gridBrush);
                }
                for (int y = 0; y <= PageData.Instance.SquaresTall; y++)
                {
                    int xStart = PageData.Instance.MarginX;
                    int yStart = (y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;
                    int xEnd = PageData.Instance.GetTotalWidth() - PageData.Instance.MarginX;
                    drawingCanvas.DrawLine(new SKPointI(xStart, yStart), new SKPointI(xEnd, yStart), gridBrush);
                }
            }

            int quarterMarginX = PageData.Instance.MarginX / 4;
            int quarterMarginY = PageData.Instance.MarginY / 4;
            SKPaint borderBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = ConfigOptions.Instance.BorderLinesColor };
            SKRectI borderSquare = new SKRectI(quarterMarginX, quarterMarginY, PageData.Instance.GetTotalWidth() - quarterMarginX, PageData.Instance.GetTotalHeight() - quarterMarginY);
            drawingCanvas.DrawRect(borderSquare, borderBrush);

            if (DrawCenterLines)
            {
                using (SKPaint centerBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = ConfigOptions.Instance.CenterLinesColor })
                {
                    int halfX = PageData.Instance.GetTotalWidth() / 2;
                    int halfY = PageData.Instance.GetTotalHeight() / 2;

                    drawingCanvas.DrawLine(halfX, quarterMarginY, halfX, PageData.Instance.GetTotalHeight() - quarterMarginY, centerBrush);
                    drawingCanvas.DrawLine(quarterMarginX, halfY, PageData.Instance.GetTotalWidth() - quarterMarginX, halfY, centerBrush);
                }
            }
            //Dispose of them.
            if (_lastImage != null)
            {
                _lastImage.Dispose();
            }
            _lastImage = drawingSurface.Snapshot();
            drawingSurface.Dispose();
            gridBrush.Dispose();
            borderBrush.Dispose();


            _redrawRequired = false;
            return _lastImage;
        }
    }
}
