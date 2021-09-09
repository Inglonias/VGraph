using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using SkiaSharp;

using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraph
{

    public enum Layers
    {
        Grid = 0,
        Lines = 1,
        Cursor = 2
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int DotX = 0;
        private int DotY = 0;

        private bool ClickedOnce = false;

        private readonly GridBackgroundLayer LGrid;
        private readonly LineLayer LLines;
        private readonly CursorLayer LCursor;

        public List<IDataLayer> LayerList { get; } = new List<IDataLayer>();

        public MainWindow()
        {
            //Default values are best estimates for 1/4 inch squares on 8.5 x 11.
            LGrid = new GridBackgroundLayer();
            LLines = new LineLayer();
            LCursor = new CursorLayer();

            OrderAllLayers();

            InitializeComponent();
            LGrid.GenerateLayerBitmap();
            MainCanvas.Width = PageData.Instance.GetTotalWidth();
            MainCanvas.Height = PageData.Instance.GetTotalHeight();
        }

        public IDataLayer GetDataLayer(Layers l)
        {
            return GetDataLayer((int)l);
        }
        private IDataLayer GetDataLayer(int i)
        {
            return LayerList.ElementAt(i);
        }

        private void OrderAllLayers()
        {
            LayerList.Add(LGrid);
            LayerList.Add(LLines);
            LayerList.Add(LCursor);
        }

        private void MainCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            LCursor.CursorPoint = LCursor.RoundToNearestIntersection(e.GetPosition(MainCanvas));
            MainCanvas.InvalidateVisual();
        }

        private void MainCanvas_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            bool anyLayerRedraw = false;
            foreach (IDataLayer l in LayerList)
            {
                if (l.IsRedrawRequired())
                {
                    anyLayerRedraw = true;
                }
            }

            if (!anyLayerRedraw)
            {
                return;
            }
            e.Surface.Canvas.Clear(SKColors.White);
            foreach (IDataLayer l in LayerList)
            {
                e.Surface.Canvas.DrawBitmap(l.GenerateLayerBitmap(), 0, 0);
            }
        }

        private void MainCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PageData.Instance.LineModeActive)
            {
                SKPointI target = LCursor.RoundToNearestIntersection(e.GetPosition(MainCanvas));
                if (!ClickedOnce)
                {
                    ClickedOnce = true;
                    Console.WriteLine("Click at X: " + target.X + " Y: " + target.Y);
                    DotX = target.X;
                    DotY = target.Y;
                }
                else
                {
                    ClickedOnce = false;
                    Console.WriteLine("Click again at X: " + target.X + " Y: " + target.Y);
                    LLines.AddNewLine(new SKPointI(DotX, DotY), new SKPointI(target.X, target.Y));
                    MainCanvas.InvalidateVisual();
                }
            }
            else
            {
                LLines.WhichLineGotClicked(e.GetPosition(MainCanvas));
            }
        }
    }
}
