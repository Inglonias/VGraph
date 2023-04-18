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
        public List<TextLabel> LabelList { get; private set; } = new List<TextLabel>();

        public void ForceRedraw()
        {
            RedrawOverride = true;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            throw new NotImplementedException();
        }

        public SKPointI GetRenderPoint()
        {
            throw new NotImplementedException();
        }

        public bool IsRedrawRequired()
        {
            throw new NotImplementedException();
        }
    }
}
