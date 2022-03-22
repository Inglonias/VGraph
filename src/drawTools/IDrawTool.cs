using SkiaSharp;
using VGraph.src.objects;

namespace VGraph.src.drawTools
{
    public interface IDrawTool
    {
        /// <summary>
        /// Returns an array of line segments to draw onto the page.
        /// These line segments will be drawn in the order they are stored in the array.
        /// </summary>
        /// <param name="start">The starting grid point of whatever you are drawing.</param>
        /// <param name="end">The ending grid point of whatever you are drawing.</param>
        /// <returns>An array of line segments to be drawn to the canvas.</returns>
        LineSegment[] DrawWithTool(SKPointI start, SKPointI end);
    }
}
