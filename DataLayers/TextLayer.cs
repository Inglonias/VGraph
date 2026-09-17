using Avalonia;
using Avalonia.Media;
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
    public class TextLayer : IDataLayer
    {
        private bool _redrawRequired = false;
        public List<TextLabel> LabelList { get; set; } = new List<TextLabel>();
        public bool ToolActive { get; private set; } = false;
        bool IDataLayer.DrawInExport => false;
        private SKImage? _lastImage;
        SKImage? IDataLayer.LastImage => _lastImage;
        public event EventHandler<TextLabel>? EditTextLabelEvent;

        public void ForceRedraw()
        {
            _redrawRequired = true;
        }

        public SKPointI GetRenderPoint()
        {
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();

            foreach (TextLabel l in LabelList)
            {
                SKPointI p = l.GetCanvasPoint();
                if (p.X < minX)
                {
                    minX = p.X;
                }
                if (p.Y < minY)
                {
                    minY = p.Y;
                }

            }

            return new SKPointI(minX, minY);
        }

        private SKRectI GetLayerRect()
        {
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();
            int maxX = 0;
            int maxY = 0;
            foreach (TextLabel l in LabelList)
            {
                SKRectI lPos = l.GetCanvasRect();
                minX = Math.Min(minX, lPos.Left);
                minY = Math.Min(minY, lPos.Top);
                maxX = Math.Max(maxX, lPos.Right);
                maxY = Math.Max(maxY, lPos.Bottom + lPos.Height); //To account for letters dipping below the origin.

            }
            return new SKRectI(minX, minY, maxX, maxY);
        }


        public bool IsRedrawRequired()
        {
            return _redrawRequired;
        }

        public void AddTextLabel(SKPointI renderPoint, string labelText, string labelColor, string fontName, int fontSize, int alignment, bool oddMode)
        {
            PageHistory.Instance.CreateUndoPoint(null, LabelList, true);
            LabelList.Add(new TextLabel(renderPoint, labelText, labelColor, fontName, fontSize, alignment, oddMode));
            PageData.Instance.MakeCanvasDirty();
            ForceRedraw();
        }

        public void SelectTool(string tool)
        {
            ToolActive = tool.Equals("TextTool");
        }

        public void HandleCreationClick(SKPointI target, SKPointI targetGrid)
        {
            if (!ToolActive)
            {
                return;
            }
            PreviewLayer previewLayer = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER);
            TextLabel newLabel = new TextLabel(targetGrid, "New Label", SKColors.Black.ToString(), "Arial", 12, TextLabel.ALIGN_CENTER_CENTER, previewLayer.OddMode);
            LabelList.Add(newLabel);
            ForceRedraw();
            EditTextLabelEvent?.Invoke(this, newLabel);
        }

        public bool HandleSelectionClick(Point point, bool maintainSelection)
        {
            //This function does NOT handle box selections. Because of that, we're looking for one line that was clicked on.
            //As soon as we find that line, we return true. If we go through the whole list and find nothing, return false.
            foreach (TextLabel l in LabelList)
            {
                if (l.WasLabelSelected(point))
                {
                    if (l.IsSelected)
                    {
                        //If we're clicking an already selected label, we want to edit it.
                        EditTextLabelEvent?.Invoke(this, l);

                    }
                }

            }
            if (!maintainSelection)
            {
                DeselectLabels();
            }
            foreach (TextLabel l in LabelList)
            {
                if (l.WasLabelSelected(point))
                {

                    l.IsSelected = true;
                    ForceRedraw();
                    return true;
                }
            }
            ForceRedraw();
            return false;
        }

        public void SelectAllLabels()
        {
            foreach (TextLabel l in LabelList)
            {
                l.IsSelected = true;
            }
            ForceRedraw();
        }

        public void HandleBoxSelect(SKRect boundingBox, bool maintainSelection)
        {
            if (!maintainSelection)
            {
                DeselectLabels();
            }
            foreach (TextLabel l in LabelList)
            {
                bool staySelected = l.IsSelected && maintainSelection;
                if (l.WasLabelSelected(boundingBox) || staySelected)
                {
                    l.IsSelected = true;
                }
            }
            ForceRedraw();
        }

        public bool DeleteSelectedLabels()
        {
            bool labelsDeleted = false;
            for (int i = LabelList.Count - 1; i >= 0; i--)
            {
                if (LabelList[i].IsSelected)
                {
                    if (!labelsDeleted)
                    {
                        labelsDeleted = true;
                        PageHistory.Instance.CreateUndoPoint(null, LabelList, true);
                        PageData.Instance.MakeCanvasDirty();
                    }
                    LabelList.RemoveAt(i);
                    ForceRedraw();
                }
            }
            return labelsDeleted;
        }

        public void MoveSelectedLabels(int x, int y)
        {
            TextLabel[] targetLabels = GetSelectedLabels();
            if (targetLabels.Length == 0)
            {
                return;
            }
            bool moveValid = true;
            foreach (TextLabel l in GetSelectedLabels())
            {
                int targetX = l.RenderPoint.X + x;
                int targetY = l.RenderPoint.Y + y;
                moveValid = targetX >= 0 && targetX <= PageData.Instance.SquaresWide && targetY >= 0 && targetY <= PageData.Instance.SquaresTall;
                if (!moveValid)
                {
                    return;
                }
            }
            foreach (TextLabel l in GetSelectedLabels())
            {
                l.RenderPoint = new SKPointI(l.RenderPoint.X + x, l.RenderPoint.Y + y);
            }
            PageData.Instance.MakeCanvasDirty();
            ForceRedraw();
        }

        public TextLabel[] GetSelectedLabels()
        {
            List<TextLabel> selectedLines = new List<TextLabel>();
            foreach (TextLabel l in LabelList)
            {
                if (l.IsSelected)
                {
                    selectedLines.Add(l);
                }
            }
            return selectedLines.ToArray();
        }

        private void DeselectLabels()
        {
            foreach (TextLabel l in LabelList)
            {
                l.IsSelected = false;
            }
        }

        public void ClearAllLabels()
        {
            LabelList.Clear();
            ForceRedraw();
        }

        public void AddNewLabels(TextLabel[] textLabels)
        {
            foreach (TextLabel l in textLabels)
            {
                LabelList.Add(l);
            }
            PageData.Instance.MakeCanvasDirty();
            ForceRedraw();
        }

        public SKImage? GenerateLayerImage()
        {
            if (IsRedrawRequired())
            {
                _redrawRequired = false;
                SKRectI layerSize = GetLayerRect();
                int canvasWidth = layerSize.Width;
                int canvasHeight = layerSize.Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }

                //Disposables
                SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
                SKSurface drawingSurface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
                var drawingCanvas = drawingSurface.Canvas;
                //For debugging:
                //drawingCanvas.Clear(SKColors.Yellow);
                SKPaint standardBrush = new SKPaint { Color = SKColors.Blue, IsAntialias = true };

                SKPointI topLeft = GetRenderPoint();
                foreach (TextLabel label in LabelList)
                {
                    SKColor labelColor = TextLabel.DEFAULT_COLOR;
                    SKColor.TryParse(label.LabelColor, out labelColor);
                    standardBrush.Color = labelColor;
                    SKPointI canvasPoint = label.GetCanvasPoint();
                    canvasPoint.X -= topLeft.X;
                    canvasPoint.Y -= topLeft.Y;
                    drawingCanvas.DrawImage(label.RenderTextLabel(), canvasPoint);
                }
                if (_lastImage != null)
                {
                    _lastImage.Dispose();
                }
                _lastImage = drawingSurface.Snapshot();
                //Dispose of them.
                drawingSurface.Dispose();
                standardBrush.Dispose();
                _redrawRequired = false;
            }

            return _lastImage;
        }
    }
}
