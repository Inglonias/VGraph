using SkiaSharp;
using VGraph.src.objects;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.drawTools
{
    public interface IDrawTool
    {
        /// <summary>
        /// Returns a list of line segments to draw onto the page.
        /// These line segments will be drawn in the order they are stored in the array.
        /// </summary>
        /// <param name="start">The start point of whatever you are drawing. Interpret this however you like.</param>
        /// <param name="end">The end point of whatever you are drawing. Interpret this however you like.</param>
        /// <returns></returns>
        LineSegment[] DrawWithTool(SKPointI start, SKPointI end);
    }
}
