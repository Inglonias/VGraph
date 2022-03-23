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

        /// <summary>
        /// For some tools, it may be desirable to be able to have radii of 1.5, 2.5, etc.
        /// This method is designed to accomodate for that. If this doesn't make sense for a given tool, simply call DrawWithTool from here.
        /// Returns an array of line segments to draw onto the page.
        /// These line segments will be drawn in the order they are stored in the array.
        /// </summary>
        /// <param name="start">The starting grid point of whatever you are drawing.</param>
        /// <param name="end">The ending grid point of whatever you are drawing.</param>
        /// <returns>An array of line segments to be drawn to the canvas.</returns>
        LineSegment[] DrawWithToolOdd(SKPointI start, SKPointI end);
    }
}
