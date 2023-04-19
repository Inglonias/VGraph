using SkiaSharp;
using System;
using System.Collections.Generic;
using VGraph.src.config;
using VGraph.src.objects;

namespace VGraph.src.dataLayers
{
    public class TextLayer : IDataLayer
    {
        private bool RedrawRequired = false;
        bool IDataLayer.DrawInExport => true;
        public List<TextLabel> LabelList { get; set; } = new List<TextLabel>();
        private SKBitmap LastImage;

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            if (LastImage == null || IsRedrawRequired())
            {
                RedrawRequired = false;
                SKRectI layerSize = GetLayerSize();
                int canvasWidth = layerSize.Width;
                int canvasHeight = layerSize.Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }

                //Disposables
                SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
                SKCanvas drawingSurface = new SKCanvas(image);
                SKPaint standardBrush = new SKPaint { Color = SKColors.Blue, IsAntialias = true };

                SKPointI topLeft = GetRenderPoint();
                foreach (TextLabel label in LabelList)
                {
                    SKColor labelColor = TextLabel.DEFAULT_COLOR;
                    SKColor.TryParse(label.LabelColor, out labelColor);
                    standardBrush.Color = labelColor;
                    SKPointI canvasPoint = label.GetCanvasPoint();
                    canvasPoint.X -= topLeft.X;
                    canvasPoint.Y -= topLeft.Y - label.GetLabelSize().Height;
                    drawingSurface.DrawText(label.LabelText, canvasPoint.X, canvasPoint.Y, standardBrush);
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
                    minY = p.Y - l.GetLabelSize().Height;
                }
                
            }

            return new SKPointI(minX, minY);
        }

        private SKRectI GetLayerSize()
        {
            int maxX = 0;
            int maxY = 0;
            foreach (TextLabel l in LabelList)
            {
                SKRectI lSize = l.GetLabelSize();
                int lXMax = lSize.Width;
                int lYMax = lSize.Height;

                maxX = Math.Max(maxX, lXMax);
                maxY = Math.Max(maxY, lYMax);

            }
            return new SKRectI(0, 0, maxX, maxY);
        }


        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }

        public void AddTextLabel(SKPointI renderPoint, string labelText, string labelColor)
        {
            TextLabel tl = new TextLabel
            {
                RenderPoint = renderPoint,
                LabelText = labelText,
                LabelColor = labelColor
            };
            LabelList.Add(tl);
        }
    }
}
