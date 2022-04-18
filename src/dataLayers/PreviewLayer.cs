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

        private SKImage LastImage;

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

        public SKImage GenerateLayerImage()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);

            if (LastImage == null || IsRedrawRequired())
            {
                RedrawOverride = false;
                SKImage image = SKImage.Create(new SKImageInfo(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight()));

                //Disposables
                SKSurface gpuSurface = PageData.Instance.GetOpenGlSurface(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

                SKPaint previewBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = PageData.Instance.CurrentLineColor.WithAlpha(86), IsAntialias = true };

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
                            gpuSurface.Canvas.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], previewBrush);
                        }
                    }
                }

                image = gpuSurface.Snapshot();

                //Dispose of them.
                gpuSurface.Dispose();
                previewBrush.Dispose();

                if (LastImage != null)
                {
                    LastImage.Dispose();
                }
                LastImage = image;
            }

            return LastImage;
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
                    lLines.CreateUndoPoint();
                    lLines.AddNewLines(lines);
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
