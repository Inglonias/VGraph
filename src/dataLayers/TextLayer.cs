using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VGraph.src.config;
using VGraph.src.objects;
using VGraph.src.ui;

namespace VGraph.src.dataLayers
{
    public class TextLayer : IDataLayer
    {
        private bool RedrawRequired = false;
        bool IDataLayer.DrawInExport => true;
        public List<TextLabel> LabelList { get; set; } = new List<TextLabel>();
        private SKBitmap LastImage;
        public bool ToolActive { get; private set; } = false;
        private string CurrentFontFamily = "Arial";
        private int CurrentFontSize = 12;

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            if (LastImage == null || IsRedrawRequired())
            {
                RedrawRequired = false;
                SKRectI layerSize = GetLayerRect();
                int canvasWidth = layerSize.Width;
                int canvasHeight = layerSize.Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }

                //Disposables
                SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
                SKCanvas drawingSurface = new SKCanvas(image);
                //For debugging:
                //drawingSurface.Clear(SKColors.Yellow);
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
                    drawingSurface.DrawBitmap(label.RenderTextLabel(), canvasPoint);
                }
                if (LastImage != null)
                {
                    LastImage.Dispose();
                }
                LastImage = image;
                //Dispose of them.
                drawingSurface.Dispose();
                standardBrush.Dispose();
                RedrawRequired = false;
            }

            return LastImage;
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
            return RedrawRequired;
        }

        public void AddTextLabel(SKPointI renderPoint, string labelText, string labelColor, string fontName, int fontSize, int alignment)
        {
            LabelList.Add(new TextLabel(renderPoint, labelText, labelColor, fontName, fontSize, alignment));
            ForceRedraw();
        }

        public void SelectTool(string tool)
        {
            ToolActive = tool.Equals("Text_Tool");
        }

        public void HandleCreationClick(SKPointI target, SKPointI targetGrid)
        {
            if (!ToolActive)
            {
                return;
            }
            SKColor color = PageData.Instance.CurrentLineColor;
            LabelPropertiesWindow lpw = new LabelPropertiesWindow();
            lpw.TextBoxFontSize.Text = CurrentFontSize.ToString();
            lpw.ComboBoxFonts.SelectedItem = new FontFamily(CurrentFontFamily);
            lpw.TargetGridPoint = targetGrid;
            lpw.Show();
        }

        public bool HandleSelectionClick(Point point)
        {
            return false;
        }
    }
}
