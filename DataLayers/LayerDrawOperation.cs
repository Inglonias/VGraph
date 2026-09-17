using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using System;
using System.Collections.Generic;
using VGraph.Config;

namespace VGraph.DataLayers
{
    public class LayerDrawOperation : ICustomDrawOperation
    {
        public List<IDataLayer> Layers { get; set; } = new List<IDataLayer>();
        public Rect Bounds => GetBounds();

        public LayerDrawOperation()
        {
            //Create layers.
            GridBackgroundLayer lGrid = new GridBackgroundLayer();
            LineLayer lLines = new LineLayer();
            TextLayer lText = new TextLayer();
            CursorLayer lCursor = new CursorLayer();
            PreviewLayer lPreview = new PreviewLayer();
            
            //Assign page data
            PageData.Instance.GetDataLayers()[PageData.GRID_LAYER] = lGrid;
            PageData.Instance.GetDataLayers()[PageData.LINE_LAYER] = lLines;
            PageData.Instance.GetDataLayers()[PageData.TEXT_LAYER] = lText;
            PageData.Instance.GetDataLayers()[PageData.PREVIEW_LAYER] = lPreview;
            PageData.Instance.GetDataLayers()[PageData.CURSOR_LAYER] = lCursor;
            
            //Add layers to the rendering stack.
            Layers.Add(lGrid);
            Layers.Add(lLines);
            Layers.Add(lText);
            Layers.Add(lCursor);
            Layers.Add(lPreview);
        }
        
        public void Dispose()
        {
            
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            throw new NotImplementedException();
        }

        public bool HitTest(Point p)
        {
            return Bounds.Contains(p);
        }

        private Rect GetBounds()
        {
            return new Rect(new Point(0,0), new Point(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight()));
        }

        //CAUTION: CoPilot helped to generate this method.
        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using var lease = leaseFeature.Lease();

            foreach (var layer in Layers)
            {
                var renderPoint = layer.GetRenderPoint();
                if (layer.GenerateLayerImage() != null)
                {
                    lease.SkCanvas.DrawImage(layer.GenerateLayerImage(), renderPoint.X, renderPoint.Y);
                }
            }
        }
    }
}
