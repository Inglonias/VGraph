using SkiaSharp;

namespace VGraph.src.objects
{
    public class TextLabel
    {
        SKPointI RenderPoint { get; set; }
        string LabelText { get; set; } = "";
        SKColor LabelColor { get; set; }
    }
}
