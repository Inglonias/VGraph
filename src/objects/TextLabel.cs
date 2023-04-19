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
        public const int X_OFFSET = 0;
        public const int Y_OFFSET = 0;

        public SKPointI GetCanvasPoint()
        {
            int startX = (RenderPoint.X * PageData.Instance.SquareSize) + PageData.Instance.MarginX + X_OFFSET;
            int startY = (RenderPoint.Y * PageData.Instance.SquareSize) + PageData.Instance.MarginY + Y_OFFSET;

            SKPointI rVal = new SKPointI(startX, startY);

            return rVal;
        }

        //The origin point for text can vary depending on a lot of things - notably its alignment and size.
        //This method guarantees a reliable origin point - the top left of the label.
        //If we want to align things, we'll need to do it ourselves.
        public SKBitmap RenderTextLabel()
        {
            SKBitmap image = new SKBitmap(new SKImageInfo(GetLabelRect().Width, GetLabelRect().Height));
            SKCanvas drawingSurface = new SKCanvas(image);
            //drawingSurface.Clear(SKColors.Yellow);
            SKColor labelColor = TextLabel.DEFAULT_COLOR;
            SKColor.TryParse(LabelColor, out labelColor);
            SKPaint standardBrush = new SKPaint { Color = labelColor, IsAntialias = true };
            drawingSurface.DrawText(LabelText, 0, GetLabelRect().Height, standardBrush);
            standardBrush.Dispose();
            return image;
        }

        //Note that this does NOT retrieve the size of the label. It retrieves the BOUNDS of the label.
        //The major difference is that the top and left can be negative.
        //If you want the raw size of the label, use this rectangle's width and height.
        public SKRectI GetLabelRect()
        {
            SKPaint textBrush = new SKPaint { TextAlign = SKTextAlign.Right };
            SKRect textBounds = new SKRect();
            textBrush.MeasureText(LabelText, ref textBounds);

            textBrush.Dispose();

            return SKRectI.Round(textBounds);
        }
    }
}
