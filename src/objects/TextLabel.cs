using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.objects
{
    public class TextLabel
    {
        public SKPointI RenderPoint { get; set; }
        public string LabelText { get; set; } = "";
        public SKColor LabelColor { get; set; }

        public SKPointI GetCanvasPoint()
        {
            int startX = (RenderPoint.X * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
            int startY = (RenderPoint.Y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;

            SKPointI rVal = new SKPointI(startX, startY);

            return rVal;
        }
    }
}
