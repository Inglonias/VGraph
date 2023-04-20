using SkiaSharp;
using System;
using System.Text.Json.Serialization;
using VGraph.src.config;

namespace VGraph.src.objects
{
    public class TextLabel
    {
        public static readonly SKColor DEFAULT_COLOR = ConfigOptions.Instance.DefaultLineColor;
        public static readonly int ALIGN_TOP_LEFT = 0;
        public static readonly int ALIGN_TOP_CENTER = 1;
        public static readonly int ALIGN_TOP_RIGHT = 2;
        public static readonly int ALIGN_CENTER_LEFT = 3;
        public static readonly int ALIGN_CENTER_CENTER = 4;
        public static readonly int ALIGN_CENTER_RIGHT = 5;
        public static readonly int ALIGN_BOTTOM_LEFT = 6;
        public static readonly int ALIGN_BOTTOM_CENTER = 7;
        public static readonly int ALIGN_BOTTOM_RIGHT = 8;

        public SKPointI RenderPoint { get; set; }
        public string LabelText { get; set; } = "";
        public string LabelColor { get; set; } //Stored as #AARRGGBB due to serialization issues with SKColor
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public int Alignment { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public TextLabel(SKPointI renderPoint, string labelText, string labelColor, string fontFamily, int fontSize, int alignment)
        {
            RenderPoint = renderPoint;
            LabelText = labelText;
            LabelColor = labelColor;
            FontFamily = fontFamily;
            FontSize = fontSize;
            Alignment = alignment;
            IsSelected = false;
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
            //Double the height to account for letters like p and q.
            SKBitmap image = new SKBitmap(new SKImageInfo(GetLabelRect().Width, GetLabelRect().Height * 2));
            SKCanvas drawingSurface = new SKCanvas(image);
            //drawingSurface.Clear(SKColors.Yellow);
            SKColor labelColor = TextLabel.DEFAULT_COLOR;
            SKColor.TryParse(LabelColor, out labelColor);
            SKPaint textBrush = new SKPaint { Typeface = SKTypeface.FromFamilyName(FontFamily), TextSize = FontSize, Color = labelColor };
            drawingSurface.DrawText(LabelText, 0, GetLabelRect().Height, textBrush);
            textBrush.Dispose();
            return image;
        }

        //Note that this does NOT retrieve the size of the label. It retrieves the BOUNDS of the label.
        //The major difference is that the top and left can be negative.
        //If you want the raw size of the label, use this rectangle's width and height.
        public SKRectI GetLabelRect()
        {
            SKColor labelColor = TextLabel.DEFAULT_COLOR;
            SKColor.TryParse(LabelColor, out labelColor);
            SKPaint textBrush = new SKPaint { Typeface = SKTypeface.FromFamilyName(FontFamily), TextSize = FontSize, Color = labelColor };
            SKRect textBounds = new SKRect();
            textBrush.MeasureText(LabelText, ref textBounds);

            textBrush.Dispose();

            return SKRectI.Round(textBounds);
        }

        public SKRectI GetCanvasRect()
        {
            SKPointI origin = GetCanvasPoint();
            SKRectI size = GetLabelRect();
            SKRectI rVal = new SKRectI(origin.X, origin.Y, origin.X + size.Width, origin.Y + size.Height);
            return rVal;
        }

        public SKPointI GetAlignmentOffset()
        {
            SKPointI rVal = new SKPointI(0, 0);
            SKRectI bounds = GetLabelRect();
            if (Alignment / 3 == 0) //0, 1, 2
            {
                rVal.Y -= bounds.Height;
            }
            else if (Alignment / 3 == 1) //3, 4, 5
            {
                rVal.Y -= bounds.Height / 2;
            }
            else if (Alignment / 3 == 2) //6, 7, 8
            {
                //Do nothing
            }

            if (Alignment % 3 == 0) //0, 3, 6
            {
                rVal.X -= bounds.Width;
            }
            else if (Alignment % 3 == 1) //1, 4, 7
            {
                rVal.X -= bounds.Width / 2;
            }
            else if (Alignment % 3 == 2) // 2, 5, 8
            {
                //Do nothing
            }

            return rVal;
        }
    }
}
