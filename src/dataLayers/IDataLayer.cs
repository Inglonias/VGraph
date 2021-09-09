using SkiaSharp;

namespace VGraph.src.dataLayers
{
    public interface IDataLayer
    {
        /// <summary>
        /// This method should be used to determine if a data layer should be redrawn or not.
        /// </summary>
        /// <returns>Returns true if the data layer should be redrawn, and false otherwise.</returns>
        bool IsRedrawRequired();
        /// <summary>
        /// Draw the relevant data layer as an SKBitmap
        /// </summary>
        /// <returns>An SKBitmap representing the current data layer.</returns>
        SKBitmap GenerateLayerBitmap();
    }
}
