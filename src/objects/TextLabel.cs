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
        public int Alignment { get; set; } = 0;

        public static readonly int ALIGN_TOP_LEFT      = 0;
        public static readonly int ALIGN_TOP_CENTER    = 1;
        public static readonly int ALIGN_TOP_RIGHT     = 2;
        public static readonly int ALIGN_CENTER_LEFT   = 3;
        public static readonly int ALIGN_CENTER_CENTER = 4;
        public static readonly int ALIGN_CENTER_RIGHT  = 5;
        public static readonly int ALIGN_BOTTOM_LEFT   = 6;
        public static readonly int ALIGN_BOTTOM_CENTER = 7;
        public static readonly int ALIGN_BOTTOM_RIGHT  = 8;

        public TextLabel(SKPointI renderPoint, string labelText, string labelColor, int alignment)
        {
            RenderPoint = renderPoint;
            LabelText = labelText;
            LabelColor = labelColor;
            Alignment = alignment;
        }

        //If, for whatever reason, we choose not to deal with alignment, this will get the canvas point corresponding to the relevant grid square.
        public SKPointI GetRawCanvasPoint()
        {
            int startX = (RenderPoint.X * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
            int startY = (RenderPoint.Y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;

            SKPointI rVal = new SKPointI(startX, startY);

            return rVal;
        }

        //This gets the point to render the text label in pixels, with alignment offset taken into account.
        public SKPointI GetCanvasPoint()
        {
            SKPointI rVal = GetRawCanvasPoint();
            rVal.Offset(GetAlignmentOffset());
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

        public SKPointI GetAlignmentOffset()
        {
            return GetAlignmentOffset(Alignment);
        }

        private SKPointI GetAlignmentOffset(int alignType)
        {
            SKPointI rVal = new SKPointI(0, 0);
            SKRectI bounds = GetLabelRect();
            if (alignType / 3 == 0) //0, 1, 2
            {

            }
            else if (alignType / 3 == 1) //3, 4, 5
            {
                rVal.Y -= bounds.Height / 2;
            }
            else if (alignType / 3 == 2) //6, 7, 8
            {
                rVal.Y -= bounds.Height;
            }

            if (alignType % 3 == 0) //0, 3, 6
            {
                rVal.X -= bounds.Width;
            }
            else if (alignType % 3 == 1) //1, 4, 7
            {
                rVal.X -= bounds.Width / 2;
            }
            else if (alignType % 3 == 2) // 2, 5, 8
            {

            }

            return rVal;
        }
    }
}
