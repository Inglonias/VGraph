﻿using OpenTK;
using OpenTK.Graphics;
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
        private SKBitmap GridBitmap = null;

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
                return GridBitmap;
            }

            int xSize = (PageData.Instance.SquaresWide * PageData.Instance.SquareSize) + (PageData.Instance.MarginX * 2);
            int ySize = (PageData.Instance.SquaresTall * PageData.Instance.SquareSize) + (PageData.Instance.MarginY * 2);

            SKBitmap grid = new SKBitmap(xSize, ySize);

            //Disposables
            SKSurface gpuSurface = PageData.Instance.GetOpenGlSurface(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

            gpuSurface.Canvas.Clear(ConfigOptions.Instance.BackgroundPaperColor);
            SKPaint gridBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = ConfigOptions.Instance.GridLinesColor };

            //Draw the background image within the border.
            if (OriginalBackgroundImage != null && DrawBackgroundImage)
            {
                SKImageInfo gridSize = new SKImageInfo(PageData.Instance.SquaresWide * PageData.Instance.SquareSize,
                                                       PageData.Instance.SquaresTall * PageData.Instance.SquareSize);
                SKBitmap backgroundImage = OriginalBackgroundImage.Resize(gridSize, SKFilterQuality.None);
                SKPaint alphaPaint = new SKPaint();
                alphaPaint.Color = alphaPaint.Color.WithAlpha(PageData.Instance.BackgroundImageAlpha);
                gpuSurface.Canvas.DrawBitmap(backgroundImage, new SKPointI(PageData.Instance.MarginX, PageData.Instance.MarginY), alphaPaint);
                backgroundImage.Dispose();
            }

            if (DrawGridLines)
            {
                for (int x = 0; x <= PageData.Instance.SquaresWide; x++)
                {
                    int xStart = (x * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
                    int yStart = PageData.Instance.MarginY;
                    int yEnd = PageData.Instance.GetTotalHeight() - PageData.Instance.MarginY;
                    gpuSurface.Canvas.DrawLine(new SKPointI(xStart, yStart), new SKPointI(xStart, yEnd), gridBrush);
                }
                for (int y = 0; y <= PageData.Instance.SquaresTall; y++)
                {
                    int xStart = PageData.Instance.MarginX;
                    int yStart = (y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;
                    int xEnd = PageData.Instance.GetTotalWidth() - PageData.Instance.MarginX;
                    gpuSurface.Canvas.DrawLine(new SKPointI(xStart, yStart), new SKPointI(xEnd, yStart), gridBrush);
                }
            }

            int quarterMarginX = PageData.Instance.MarginX / 4;
            int quarterMarginY = PageData.Instance.MarginY / 4;
            SKPaint borderBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = ConfigOptions.Instance.BorderLinesColor };
            SKRectI borderSquare = new SKRectI(quarterMarginX, quarterMarginY, PageData.Instance.GetTotalWidth() - quarterMarginX, PageData.Instance.GetTotalHeight() - quarterMarginY);
            gpuSurface.Canvas.DrawRect(borderSquare, borderBrush);

            if (DrawCenterLines)
            {
                using (SKPaint centerBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = ConfigOptions.Instance.CenterLinesColor })
                {
                    int halfX = PageData.Instance.GetTotalWidth() / 2;
                    int halfY = PageData.Instance.GetTotalHeight() / 2;

                    gpuSurface.Canvas.DrawLine(halfX, quarterMarginY, halfX, PageData.Instance.GetTotalHeight() - quarterMarginY, centerBrush);
                    gpuSurface.Canvas.DrawLine(quarterMarginX, halfY, PageData.Instance.GetTotalWidth() - quarterMarginX, halfY, centerBrush);
                }
            }
            using (var image = gpuSurface.Snapshot())
            {
                grid = SKBitmap.FromImage(image);
            }
            //Dispose of them.
            gpuSurface.Dispose();
            gridBrush.Dispose();
            borderBrush.Dispose();

            if (GridBitmap != null)
            {
                GridBitmap.Dispose();
            }
            GridBitmap = grid;
            RedrawRequired = false;
            return GridBitmap;
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }

        public SKPoint GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }
    }
}
