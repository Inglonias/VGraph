using SkiaSharp;
using System;
using VGraph.src.config;

namespace VGraph.src.objects
{
    public class TextLabel
    {
        public static readonly SKColor DEFAULT_COLOR = ConfigOptions.Instance.DefaultLineColor;

        public SKPointI RenderPoint { get; set; }
        public string LabelText { get; set; } = "";
        public string LabelColor { get; set; } //Stored as #AARRGGBB due to serialization issues with SKColor

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
            SKRect textBounds = new SKRect();
            textBrush.MeasureText(LabelText, ref textBounds);

            textBrush.Dispose();

            return SKRectI.Ceiling(textBounds);
        }
    }
}
