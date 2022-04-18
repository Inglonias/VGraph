using SkiaSharp;

namespace VGraph.src.dataLayers
{
    public interface IDataLayer
    {
        bool DrawInExport { get; }

        /// <summary>
        /// This method should be used to determine if a data layer should be redrawn or not.
        /// If any layer needs to be redrawn, then the entire main canvas is redrawn.
        /// Because the cursor layer just returns true here no matter what, this is a little redundant, but it exists nonetheless.
        /// Who knows, maybe I'll add some way to disable the rendering of layers in the future? I dunno.
        /// </summary>
        /// <returns>Returns true if the data layer should be redrawn, and false otherwise.</returns>
        bool IsRedrawRequired();
        /// <summary>
        /// Draw the relevant data layer as an SKImage.
        /// </summary>
        /// <returns>An SKImage representing the current data layer.</returns>
        SKImage GenerateLayerImage();
        /// <summary>
        /// Not every layer has to be the same size. This method exists to tell the rendering event where to start drawing this layer.
        /// The background and line layers currently return (0, 0) here, but the cursor layer is a special case.
        /// </summary>
        /// <returns>World coordinates to render this layer at.</returns>
        SKPointI GetRenderPoint();
        /// <summary>
        /// Force a layer to redraw at the next PaintSurface event.
        /// </summary>
        void ForceRedraw();
    }
}
