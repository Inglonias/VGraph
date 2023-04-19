using SkiaSharp;
using System;
using System.Collections.Generic;
using VGraph.src.config;
using VGraph.src.objects;

namespace VGraph.src.dataLayers
{
    public class TextLayer : IDataLayer
    {
        private bool RedrawOverride = false;
        bool IDataLayer.DrawInExport => true;
        public List<TextLabel> LabelList { get; set; } = new List<TextLabel>();

        public void ForceRedraw()
        {
            RedrawOverride = true;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            RedrawOverride = false;
            foreach (TextLabel t in LabelList)
            {

            }
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

        private SKRectI GetLayerSize()
        {
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();
            int maxX = 0;
            int maxY = 0;
            foreach (TextLabel l in LabelList)
            {
                SKRectI lSize = l.GetLabelSize();
                int lXMin = lSize.Left;
                int lYMin = lSize.Top;
                int lXMax = lSize.Right;
                int lYMax = lSize.Bottom;

                minX = Math.Min(minX, lXMin);
                minY = Math.Min(minY, lYMin);
                maxX = Math.Max(maxX, lXMax);
                maxY = Math.Max(maxY, lYMax);
            }
            return new SKRectI(minX, minY, maxX, maxY);
        }


        public bool IsRedrawRequired()
        {
            return RedrawOverride;
        }

        public void AddTextLabel(SKPointI renderPoint, string labelText, SKColor labelColor)
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
