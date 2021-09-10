using System;
using System.Windows;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    internal class CursorLayer : IDataLayer
    {
        public SKPointI CursorPoint { get; set; } = new SKPointI(0, 0);
        private SKBitmap Bitmap;

        private const int RADIUS = 4;

        public CursorLayer()
        {
        }

        /// <summary>
        /// This method rounds a point on the main canvas to the main canvas grid such that the return value is equal to a multiple of PageData.Instance.SquareSize + PageData.Instance.Margin
        /// </summary>
        /// <param name="p">Point to round off</param>
        /// <returns></returns>
        public SKPointI RoundToNearestIntersection(Point p)
        {
            return RoundToNearestIntersection(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        private SKPointI RoundToNearestIntersection(int x, int y)
        {
            //Ignore it if we're out of bounds.
            if ((x - PageData.Instance.Margin < 0) || (y - PageData.Instance.Margin < 0) || (x + PageData.Instance.Margin > PageData.Instance.GetTotalWidth()) || (y + PageData.Instance.Margin > PageData.Instance.GetTotalHeight()))
            {
                return CursorPoint;
            }

            //Subtract the margin out.
            int mouseX = x - PageData.Instance.Margin;
            int mouseY = y - PageData.Instance.Margin;

            //Round to the nearest intersection.
            int targetX = ((mouseX % PageData.Instance.SquareSize) < (PageData.Instance.SquareSize / 2)) ?
                    (mouseX - (mouseX % PageData.Instance.SquareSize)) :
                    (mouseX + (PageData.Instance.SquareSize - (mouseX % PageData.Instance.SquareSize)));
            int targetY = ((mouseY % PageData.Instance.SquareSize) < (PageData.Instance.SquareSize / 2)) ?
                    (mouseY - (mouseY % PageData.Instance.SquareSize)) :
                    (mouseY + (PageData.Instance.SquareSize - (mouseY % PageData.Instance.SquareSize)));

            //Add the margin back in.
            targetX += PageData.Instance.Margin;
            targetY += PageData.Instance.Margin;

            return new SKPointI(targetX, targetY);
        }

        public SKPointI GetRenderPoint()
        {
            return new SKPointI(CursorPoint.X - RADIUS, CursorPoint.Y - RADIUS);
        }

        public SKBitmap GenerateLayerBitmap()
        {

            //This code is commented out as a monument to my own stupidity. This canvas ABSOLUTELY DID NOT need to be this big.
            //SKBitmap replaceBitmap = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());
            if (Bitmap == null)
            {
                Bitmap = new SKBitmap(RADIUS * 2, RADIUS * 2);
                if (PageData.Instance.LineModeActive)
                {
                    //Disposables
                    SKCanvas canvas = new SKCanvas(Bitmap);
                    SKPaint brush = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Red };

                    canvas.DrawCircle(new SKPointI(RADIUS, RADIUS), RADIUS, brush);

                    //Dispose of them.
                    canvas.Dispose();
                    brush.Dispose();
                }
            }
            return Bitmap;
        }

        public bool IsRedrawRequired()
        {
            return true;
        }
    }
}