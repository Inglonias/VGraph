using SkiaSharp;
using System;
using System.Collections.Generic;
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
            return null;
        }

        public SKPointI GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }

        public bool IsRedrawRequired()
        {
            return false;
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
