using SkiaSharp;
using System;
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

        public SKRectI GetLabelSize()
        {
            SKPaint textBrush = new SKPaint();
            textBrush.Color = LabelColor;
            SKRect textBounds = new SKRect();
            textBrush.MeasureText(LabelText, ref textBounds);

            textBrush.Dispose();

            return SKRectI.Ceiling(textBounds);
        }
    }
}
