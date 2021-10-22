using SkiaSharp;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.drawTools
{
    public interface IDrawTool
    {
        LineSegment[] DrawWithTool(SKPointI start, SKPointI end);
    }
}
