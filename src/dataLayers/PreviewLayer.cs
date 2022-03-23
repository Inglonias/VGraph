using System;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.objects;

namespace VGraph.src.dataLayers
{
    internal class PreviewLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => false;

        public SKPointI PreviewPoint { get; set; }
        public SKPointI PreviewGridPoint { get; set; }

        public bool OddMode { get; set; }

        private SKBitmap LastBitmap;

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

            if (LastBitmap == null || IsRedrawRequired())
            {
                RedrawOverride = false;
                SKBitmap bitmap = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

                //Disposables
                SKCanvas canvas = new SKCanvas(bitmap);

                SKPaint previewBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = new SKColor(0, 128, 0, 128), IsAntialias = true };

                if (PreviewPointActive)
                {
                    SKPointI cursorGridPoint = ((CursorLayer)PageData.Instance.GetDataLayer(PageData.CURSOR_LAYER)).GetCursorGridPoints();
                    LineSegment[] previewLines;
                    if (!OddMode) {
                       previewLines = lLines.SelectedTool.DrawWithTool(PreviewGridPoint, cursorGridPoint);
                    }
                    else
                    {
                        previewLines = lLines.SelectedTool.DrawWithToolOdd(PreviewGridPoint, cursorGridPoint);
                    }
                    if (previewLines != null)
                    {
                        foreach (LineSegment line in previewLines)
                        {
                            SKPointI[] canvasPoints = line.GetCanvasPoints();
                            canvas.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], previewBrush);
                        }
                    }
                }

                //Dispose of them.
                canvas.Dispose();
                previewBrush.Dispose();

                if (LastBitmap != null)
                {
                    LastBitmap.Dispose();
                }
                LastBitmap = bitmap;
            }

            return LastBitmap;
        }

        public SKPoint GetRenderPoint()
        {
            return new SKPointI(0, 0); //TODO: Make this smarter.
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
                    lLines.AddNewLines(lines, true);
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
