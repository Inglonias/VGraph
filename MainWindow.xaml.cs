using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VGraph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int squareWidth = 30;
        int squareHeight = 40;
        int squareSize = 24;
        int margin = 48;

        int xSize = 0;
        int ySize = 0;
        int dotX = 0;
        int dotY = 0;

        SKBitmap gridMap;
        SKBitmap dotMap;

        public MainWindow()
        {
            gridMap = generateGridBitmap(squareWidth, squareHeight, squareSize);
            InitializeComponent();
            mainCanvas.Width = gridMap.Width;
            mainCanvas.Height = gridMap.Height;
            InvalidateVisual();
        }

        private SKBitmap generateGridBitmap(int squaresWide, int squaresTall, int squareSize)
        {
            xSize = ((squaresWide * squareSize) + (margin * 2));
            ySize = ((squaresTall * squareSize) + (margin * 2));

            SKBitmap grid = new SKBitmap(xSize, ySize);

            //Disposables
            SKCanvas gridCanvas = new SKCanvas(grid);
            SKPaint brush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

            for (int x = 0; x < squaresWide; x++)
            {
                for (int y = 0; y < squaresTall; y++)
                {
                    int xStart = (x * squareSize) + margin;
                    int yStart = (y * squareSize) + margin;
                    SKRectI squareToDraw = new SKRectI(xStart, yStart, xStart + squareSize, yStart + squareSize);
                    gridCanvas.DrawRect(squareToDraw, brush);
                }
            }

            //Dispose of them.
            gridCanvas.Dispose();
            brush.Dispose();

            return grid;
        }

        private SKBitmap generateDotBitmap(int dotX, int dotY)
        {
            SKBitmap dotMap = new SKBitmap(xSize, ySize);

            //Disposables
            SKCanvas dotCanvas = new SKCanvas(dotMap);
            SKPaint dotPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill };

            dotCanvas.DrawCircle(dotX, dotY, 4, dotPaint);

            //Dispose of them
            dotCanvas.Dispose();
            dotPaint.Dispose();

            return dotMap;
        }

        private SKPointI roundToNearestIntersection(int x, int y)
        {
            //Subtract the margin out.
            int mouseX = x - margin;
            int mouseY = y - margin;

            //Round to the nearest intersection.
            int targetX = ((mouseX % squareSize) < (squareSize / 2)) ?
                    (mouseX - (mouseX % squareSize)) :
                    (mouseX + (squareSize - (mouseX % squareSize)));
            int targetY = ((mouseY % squareSize) < (squareSize / 2)) ?
                    (mouseY - (mouseY % squareSize)) :
                    (mouseY + (squareSize - (mouseY % squareSize)));

            //Add the margin back in.
            targetX += margin;
            targetY += margin;

            return new SKPointI(targetX, targetY);
        }
        private SKPointI roundToNearestIntersection(SKPointI coords)
        {
            return roundToNearestIntersection(coords.X, coords.Y);
        }

        private void MainCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            int mouseX = Convert.ToInt32(e.GetPosition(mainCanvas).X);
            int mouseY = Convert.ToInt32(e.GetPosition(mainCanvas).Y);

            if ((mouseX - margin < 0) || mouseY - margin < 0 || mouseX + margin > mainCanvas.Width || mouseY + margin > mainCanvas.Height)
            {
                //Ignore it if we're out of bounds.
                return;
            }
            SKPointI targetCoords = roundToNearestIntersection(mouseX, mouseY);

            //Don't re-render if there wasn't actually any sort of change made.
            if (targetCoords.X != dotX || targetCoords.Y != dotY)
            {
                if (dotMap != null)
                {
                    //There's a leak if we don't do this.
                    dotMap.Dispose();
                }
                dotMap = generateDotBitmap(targetCoords.X, targetCoords.Y);
                dotX = targetCoords.X;
                dotY = targetCoords.Y;
                mainCanvas.InvalidateVisual();
            }
        }

        private void MainCanvas_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.White);
            e.Surface.Canvas.DrawBitmap(gridMap, 0, 0);
            if (dotMap != null)
            {
                e.Surface.Canvas.DrawBitmap(dotMap, 0, 0);
            }
        }
    }
}
