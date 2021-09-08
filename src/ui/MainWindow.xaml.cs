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
        int DotX = 0;
        int DotY = 0;

        bool ClickedOnce = false;

        //SKBitmap gridMap;
        readonly GridBackground Grid;
        SKBitmap DotMap;

        public MainWindow()
        {
            //Default values are best estimates for 1/4 inch squares on 8.5 x 11.
            Grid = new GridBackground(30, 39, 24, 48);
            InitializeComponent();
            Grid.GetGridBitmap();
            MainCanvas.Width = Grid.GetTotalWidth();
            MainCanvas.Height = Grid.GetTotalHeight();
        }

        

        private SKBitmap generateDotBitmap(int dotX, int dotY)
        {
            SKBitmap dotMap = new SKBitmap(Grid.GetTotalWidth(), Grid.GetTotalHeight());

            //Disposables
            SKCanvas dotCanvas = new SKCanvas(dotMap);
            SKPaint dotPaint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill };

            dotCanvas.DrawCircle(dotX, dotY, 4, dotPaint);

            //Dispose of them
            dotCanvas.Dispose();
            dotPaint.Dispose();

            return dotMap;
        }

        private void MainCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            SKPointI targetCoords = Grid.RoundToNearestIntersection(e.GetPosition(MainCanvas));
            int mouseX = targetCoords.X;
            int mouseY = targetCoords.Y;

            if ((mouseX - Grid.GetMargin() < 0) || mouseY - Grid.GetMargin() < 0 || mouseX + Grid.GetMargin() > MainCanvas.Width || mouseY + Grid.GetMargin() > MainCanvas.Height)
            {
                //Ignore it if we're out of bounds.
                return;
            }
            

            //Don't re-render if there wasn't actually any sort of change made.
            if (targetCoords.X != DotX || targetCoords.Y != DotY)
            {
                if (DotMap != null)
                {
                    //There's a leak if we don't do this.
                    DotMap.Dispose();
                }
                DotMap = generateDotBitmap(targetCoords.X, targetCoords.Y);
                DotX = targetCoords.X;
                DotY = targetCoords.Y;
                MainCanvas.InvalidateVisual();
            }
        }

        private void MainCanvas_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(SKColors.White);
            e.Surface.Canvas.DrawBitmap(Grid.GetGridBitmap(), 0, 0);
            if (DotMap != null)
            {
                e.Surface.Canvas.DrawBitmap(DotMap, 0, 0);
            }
        }

        private void MainCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SKPointI target = Grid.RoundToNearestIntersection(e.GetPosition(MainCanvas));
            if (!ClickedOnce)
            {
                ClickedOnce = true;
                Console.WriteLine("Click at X: " + target.X + " Y: " + target.Y);
            }
            else
            {
                ClickedOnce = false;
                Console.WriteLine("Click again at X: " + target.X + " Y: " + target.Y);
            }
        }
    }
}
