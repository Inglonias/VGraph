using System.Collections.Generic;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.objects;

namespace VGraph.src.dataLayers
{
    public class PolygonLayer : IDataLayer
    {
        private bool RedrawRequired;
        public bool DrawInExport => true;
        private SKBitmap? LastImage = null;

        private List<Polygon> PolygonList = new List<Polygon>();

        public PolygonLayer()
        {

        }

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }

        public SKBitmap GenerateLayerBitmap()
        {
            if (LastImage == null || IsRedrawRequired())
            {
                SKBitmap image = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());
                SKCanvas drawingSurface = new SKCanvas(image);
                SKPaint standardBrush = new SKPaint { Style = SKPaintStyle.Fill, StrokeWidth = 1, Color = SKColors.Blue.WithAlpha(128), IsAntialias = true };
                SKPaint borderBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 5, Color = SKColors.Blue, IsAntialias = true };
                foreach (Polygon poly in PolygonList)
                {
                    drawingSurface.DrawPath(poly.GetDrawingPath(), standardBrush);
                    drawingSurface.DrawPath(poly.GetDrawingPath(), borderBrush);
                }
                RedrawRequired = false;
                LastImage = image;
            }
            
            return LastImage;

        }

        public SKPointI GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }
    }
}
