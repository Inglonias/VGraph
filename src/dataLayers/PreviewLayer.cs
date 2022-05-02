using System;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.objects;

namespace VGraph.src.dataLayers
{
    public class PreviewLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => false;

        public SKPointI PreviewPoint { get; set; }
        public SKPointI PreviewGridPoint { get; set; }

        private LineSegment[] PreviewLines;

        public bool OddMode { get; set; }

        private SKBitmap LastImage;

        private bool PreviewPointActive = false;
        private bool RedrawOverride = false;

        public PreviewLayer()
        {
            OddMode = false;
        }

        public void ForceRedraw()
        {
            RedrawOverride = true;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);

            if (LastImage == null || IsRedrawRequired())
            {
                RedrawOverride = false;
                int canvasWidth = GetLayerSize().Width;
                int canvasHeight = GetLayerSize().Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }

                //Disposables
                SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
                SKCanvas drawingSurface = new SKCanvas(image);
                SKPaint previewBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = PageData.Instance.CurrentLineColor.WithAlpha(86), IsAntialias = true };

                if (PreviewPointActive)
                {
                    SKPointI cursorGridPoint = ((CursorLayer)PageData.Instance.GetDataLayer(PageData.CURSOR_LAYER)).GetCursorGridPoints();
                    if (!OddMode) {
                       PreviewLines = lLines.SelectedTool.DrawWithTool(PreviewGridPoint, cursorGridPoint);
                    }
                    else
                    {
                        PreviewLines = lLines.SelectedTool.DrawWithToolOdd(PreviewGridPoint, cursorGridPoint);
                    }
                    if (PreviewLines != null)
                    {
                        foreach (LineSegment line in PreviewLines)
                        {
                            SKPointI[] canvasPoints = line.GetCanvasPoints();
                            SKPointI topLeft = GetRenderPoint();
                            canvasPoints[LineSegment.START].X -= topLeft.X;
                            canvasPoints[LineSegment.START].Y -= topLeft.Y;
                            canvasPoints[LineSegment.END].X -= topLeft.X;
                            canvasPoints[LineSegment.END].Y -= topLeft.Y;
                            drawingSurface.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], previewBrush);
                        }
                    }
                }
                else
                {
                    PreviewLines = null;
                }

                //Dispose of them.
                drawingSurface.Dispose();
                previewBrush.Dispose();

                if (LastImage != null)
                {
                    LastImage.Dispose();
                }
                LastImage = image;
            }

            return LastImage;
        }

        public SKPointI GetRenderPoint()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();

            if (PreviewLines == null)
            {
                return new SKPointI(0, 0);
            }
            foreach (LineSegment l in PreviewLines)
            {
                foreach (SKPointI p in l.GetCanvasPoints())
                {
                    if (p.X < minX)
                    {
                        minX = p.X;
                    }
                    if (p.Y < minY)
                    {
                        minY = p.Y;
                    }
                }
            }

            return new SKPointI(minX - drawRadius, minY - drawRadius);
        }

        private SKRectI GetLayerSize()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();
            int maxX = 0;
            int maxY = 0;
            if (PreviewLines == null || PreviewLines.Length == 0)
            {
                return new SKRectI(0, 0, 1, 1);
            }
            foreach (LineSegment l in PreviewLines)
            {
                foreach (SKPointI p in l.GetCanvasPoints())
                {
                    if (p.X < minX)
                    {
                        minX = p.X;
                    }
                    if (p.Y < minY)
                    {
                        minY = p.Y;
                    }
                    if (p.X > maxX)
                    {
                        maxX = p.X;
                    }
                    if (p.Y > maxY)
                    {
                        maxY = p.Y;
                    }
                }
            }
            return new SKRectI(minX - 100, minY - 100, maxX + 100, maxY + 100);
        }

        public bool IsRedrawRequired()
        {
            return PreviewPointActive || RedrawOverride;
        }

        public void HandleCreationClick(SKPointI point, SKPointI gridPoint)
        {
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);

            if (PreviewPointActive)
            {
                PreviewPointActive = false;
                LineSegment[] lines;
                if (!OddMode)
                {
                    lines = lLines.SelectedTool.DrawWithTool(PreviewGridPoint, gridPoint);
                }
                else
                {
                    lines = lLines.SelectedTool.DrawWithToolOdd(PreviewGridPoint, gridPoint);
                }
                if (lines != null)
                {
                    lLines.CreateUndoPoint();
                    lLines.AddNewLines(lines);
                    PageData.Instance.MakeCanvasDirty();
                    ForceRedraw();
                }
            }
            else
            {
                PreviewPointActive = true;
                PreviewPoint = point;
                PreviewGridPoint = gridPoint;
            }
        }
    }
}
