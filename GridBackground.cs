using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VGraph
{
    class GridBackground
    {
        readonly int SquaresWide;
        readonly int SquaresTall;
        readonly int SquareSize;
        readonly int Margin;

        SKBitmap GridBitmap;

        public GridBackground(int squaresWide, int squaresTall, int squareSize, int margin)
        {
            SquaresWide = squaresWide;
            SquaresTall = squaresTall;
            SquareSize = squareSize;
            Margin = margin;
        }

        public int GetTotalWidth()
        {
            if (GridBitmap != null)
            {
                return GridBitmap.Width;
            }
            return -1;
        }

        public int GetTotalHeight()
        {
            if (GridBitmap != null)
            {
                return GridBitmap.Height;
            }
            return -1;
        }

        public int GetMargin()
        {
            return Margin;
        }

        public SKBitmap GetGridBitmap()
        {
            if (GridBitmap != null)
            {
                return GridBitmap;
            }

            int xSize = (SquaresWide * SquareSize) + (Margin * 2);
            int ySize = (SquaresTall * SquareSize) + (Margin * 2);

            SKBitmap grid = new SKBitmap(xSize, ySize);

            //Disposables
            SKCanvas gridCanvas = new SKCanvas(grid);
            SKPaint brush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

            for (int x = 0; x < SquaresWide; x++)
            {
                for (int y = 0; y < SquaresTall; y++)
                {
                    int xStart = (x * SquareSize) + Margin;
                    int yStart = (y * SquareSize) + Margin;
                    SKRectI squareToDraw = new SKRectI(xStart, yStart, xStart + SquareSize, yStart + SquareSize);
                    gridCanvas.DrawRect(squareToDraw, brush);
                }
            }

            //Dispose of them.
            gridCanvas.Dispose();
            brush.Dispose();

            GridBitmap = grid;
            return GridBitmap;
        }
        public SKPointI RoundToNearestIntersection(Point p)
        {
            return RoundToNearestIntersection(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
        }

        private SKPointI RoundToNearestIntersection(int x, int y)
        {
            //Subtract the margin out.
            int mouseX = x - Margin;
            int mouseY = y - Margin;

            //Round to the nearest intersection.
            int targetX = ((mouseX % SquareSize) < (SquareSize / 2)) ?
                    (mouseX - (mouseX % SquareSize)) :
                    (mouseX + (SquareSize - (mouseX % SquareSize)));
            int targetY = ((mouseY % SquareSize) < (SquareSize / 2)) ?
                    (mouseY - (mouseY % SquareSize)) :
                    (mouseY + (SquareSize - (mouseY % SquareSize)));

            //Add the margin back in.
            targetX += Margin;
            targetY += Margin;

            return new SKPointI(targetX, targetY);
        }
    }
}
