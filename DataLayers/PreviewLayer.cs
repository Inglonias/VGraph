using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGraph.Config;
using VGraph.Objects;

namespace VGraph.DataLayers
{
    public class PreviewLayer : IDataLayer
    {
        public SKPointI PreviewPoint { get; set; }
        public SKPointI PreviewGridPoint { get; set; }
        private LineSegment[]? _previewLines;
        public bool OddMode { get; set; }
        private SKImage? _lastImage;
        SKImage? IDataLayer.LastImage => _lastImage;
        bool IDataLayer.DrawInExport => false;
        private bool _previewPointActive = false;
        private bool _redrawOverride = false;
        public PreviewLayer()
        {
            OddMode = false;
        }

        public void ForceRedraw()
        {
            _redrawOverride = true;
        }

        public SKPointI GetRenderPoint()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();

            if (_previewLines == null)
            {
                return new SKPointI(0, 0);
            }
            foreach (LineSegment l in _previewLines)
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
            if (_previewLines == null || _previewLines.Length == 0)
            {
                return new SKRectI(0, 0, 1, 1);
            }
            foreach (LineSegment l in _previewLines)
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
            return _previewPointActive || _redrawOverride;
        }

        public void HandleCreationClick(SKPointI point, SKPointI gridPoint)
        {
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);

            if (lLines.SelectedTool == null)
            {
                return;
            }

            if (_previewPointActive)
            {
                _previewPointActive = false;
                LineSegment[] lines;
                if (!OddMode)
                {
                    lines = lLines.SelectedTool.DrawWithTool(PreviewGridPoint, gridPoint)!;
                }
                else
                {
                    lines = lLines.SelectedTool.DrawWithToolOdd(PreviewGridPoint, gridPoint)!;
                }

                PageHistory.Instance.CreateUndoPoint(lLines.LineList, null, true);
                lLines.AddNewLines(lines);
                PageData.Instance.MakeCanvasDirty();
                ForceRedraw();
            }
            else
            {
                _previewPointActive = true;
                PreviewPoint = point;
                PreviewGridPoint = gridPoint;
            }
        }

        public string GetStatusText()
        {
            if (!_previewPointActive)
            {
                return "";
            }
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            SKPointI cursorGridPoint = ((CursorLayer)PageData.Instance.GetDataLayer(PageData.CURSOR_LAYER)).GetCursorGridPoints();
            string rVal = lLines.SelectedTool!.GenerateStatusText(PreviewGridPoint, cursorGridPoint);

            return rVal;
        }

        public SKImage? GenerateLayerImage()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);

            if (_lastImage == null || IsRedrawRequired())
            {
                _redrawOverride = false;
                int canvasWidth = GetLayerSize().Width;
                int canvasHeight = GetLayerSize().Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }

                //Disposables
                SKSurface drawingSurface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
                SKCanvas drawingCanvas = drawingSurface.Canvas;
                SKPaint previewBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = PageData.Instance.CurrentLineColor.WithAlpha(86), IsAntialias = true };

                if (_previewPointActive && lLines.SelectedTool != null)
                {
                    SKPointI cursorGridPoint = ((CursorLayer)PageData.Instance.GetDataLayer(PageData.CURSOR_LAYER)).GetCursorGridPoints();
                    if (!OddMode)
                    {
                        _previewLines = lLines.SelectedTool.DrawWithTool(PreviewGridPoint, cursorGridPoint);
                    }
                    else
                    {
                        _previewLines = lLines.SelectedTool.DrawWithToolOdd(PreviewGridPoint, cursorGridPoint);
                    }
                    if (_previewLines != null)
                    {
                        foreach (LineSegment line in _previewLines)
                        {
                            SKPointI[] canvasPoints = line.GetCanvasPoints();
                            SKPointI topLeft = GetRenderPoint();
                            canvasPoints[LineSegment.Start].X -= topLeft.X;
                            canvasPoints[LineSegment.Start].Y -= topLeft.Y;
                            canvasPoints[LineSegment.End].X -= topLeft.X;
                            canvasPoints[LineSegment.End].Y -= topLeft.Y;
                            drawingCanvas.DrawLine(canvasPoints[LineSegment.Start], canvasPoints[LineSegment.End], previewBrush);
                        }
                    }
                }
                else
                {
                    _previewLines = null;
                }
                //Dispose of them.
                if (_lastImage != null)
                {
                    _lastImage.Dispose();
                }
                _lastImage = drawingSurface.Snapshot();
                drawingSurface.Dispose();
                previewBrush.Dispose();

            }

            return _lastImage;
        }
    }
}
