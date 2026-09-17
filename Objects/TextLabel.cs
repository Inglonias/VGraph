using System;
using System.Text.Json.Serialization;
using Avalonia;
using SkiaSharp;
using VGraph.Config;

namespace VGraph.Objects
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
        public bool OddMode { get; set; } = false;
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public TextLabel(SKPointI renderPoint, string labelText, string labelColor, string fontFamily, int fontSize, int alignment, bool oddMode)
        {
            RenderPoint = renderPoint;
            LabelText = labelText;
            LabelColor = labelColor;
            FontFamily = fontFamily;
            FontSize = fontSize;
            Alignment = alignment;
            IsSelected = false;
            OddMode = oddMode;
        }

        //If, for whatever reason, we choose not to deal with alignment, this will get the canvas point corresponding to the relevant grid square.
        public SKPointI GetRawCanvasPoint()
        {
            int startX = (RenderPoint.X * PageData.Instance.SquareSize) + PageData.Instance.MarginX;
            int startY = (RenderPoint.Y * PageData.Instance.SquareSize) + PageData.Instance.MarginY;
            if (OddMode)
            {
                startX += PageData.Instance.SquareSize / 2;
                startY += PageData.Instance.SquareSize / 2;
            }

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
        public SKImage RenderTextLabel()
        {
            //1.5x the height to account for letters like p and q.
            int targetWidth = GetLabelRect().Width;
            int targetHeight = (GetLabelRect().Height * 3) / 2;
            using (SKSurface drawingSurface = SKSurface.Create(new SKImageInfo(targetWidth, targetHeight)))
            {
                var drawingCanvas = drawingSurface.Canvas;

                SKColor labelColor = TextLabel.DEFAULT_COLOR;
                SKColor.TryParse(LabelColor, out labelColor);
                SKFont textFont = new SKFont { Typeface = SKTypeface.FromFamilyName(FontFamily), Size = FontSize };
                SKPaint textBrush = new SKPaint { Color = labelColor };
                float[] intervals = { 5.0f, 5.0f };
                SKPathEffect dashPathEffect = SKPathEffect.CreateDash(intervals, 5.0f);
                SKPaint selectedBrush = new SKPaint { Style = SKPaintStyle.Stroke, PathEffect = dashPathEffect, StrokeWidth = 5.0f, Color = ConfigOptions.Instance.LineHighlightColor, IsAntialias = true };
                drawingCanvas.DrawText(LabelText, 0, GetLabelRect().Height, SKTextAlign.Left, textFont, textBrush);
                if (IsSelected)
                {
                    SKRectI selectedRect = new SKRectI(0, 0, targetWidth, targetHeight);
                    drawingCanvas.DrawRect(selectedRect, selectedBrush);
                }
                textFont.Dispose();
                textBrush.Dispose();
                selectedBrush.Dispose();
                return drawingSurface.Snapshot();
            }
        }

        /// <summary>
        /// Determines whether "clickPoint" was intended to select a line.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool WasLabelSelected(Point clickPoint)
        {
            SKRectI rect = GetCanvasRect();
            int x = (int)clickPoint.X;
            int y = (int)clickPoint.Y;
            SKPointI skClickPoint = new SKPointI(x, y);
            return rect.Contains(skClickPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool WasLabelSelected(SKRect boundingBox)
        {
            SKRectI rect = GetCanvasRect();
            return boundingBox.Contains(rect);
        }

        //Note that this does NOT retrieve the size of the label. It retrieves the BOUNDS of the label.
        //The major difference is that the top and left can be negative.
        //If you want the raw size of the label, use this rectangle's width and height.
        public SKRectI GetLabelRect()
        {
            SKColor labelColor = DEFAULT_COLOR;
            SKColor.TryParse(LabelColor, out labelColor);
            SKFont textFont = new SKFont { Typeface = SKTypeface.FromFamilyName(FontFamily), Size = FontSize };
            SKPaint textBrush = new SKPaint { Color = labelColor };
            SKRect textBounds = new SKRect();
            textFont.MeasureText(LabelText, out textBounds, textBrush);

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
