using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using SkiaSharp;

using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraph.src.ui
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GridBackgroundLayer LGrid;
        private readonly LineLayer LLines;
        private readonly PreviewLayer LPreview;
        private readonly CursorLayer LCursor;

        public MainWindow()
        {
            //Default values are best estimates for 1/4 inch squares on 8.5 x 11.
            LGrid = new GridBackgroundLayer();
            LLines = new LineLayer();
            LPreview = new PreviewLayer();
            LCursor = new CursorLayer();

            AssignPageData();

            InitializeComponent();
            MainMenuBar.MainWindowParent = this;
            LGrid.GenerateLayerBitmap();
            MainCanvas.Width = PageData.Instance.GetTotalWidth();
            MainCanvas.Height = PageData.Instance.GetTotalHeight();
        }

        private void AssignPageData()
        {
            PageData.Instance.GetDataLayers()[PageData.GRID_LAYER]    = LGrid;
            PageData.Instance.GetDataLayers()[PageData.LINE_LAYER]    = LLines;
            PageData.Instance.GetDataLayers()[PageData.PREVIEW_LAYER] = LPreview;
            PageData.Instance.GetDataLayers()[PageData.CURSOR_LAYER]  = LCursor;
        }

        private void MainCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            LCursor.CanvasPoint = e.GetPosition(MainCanvas);
            if (!Mouse.LeftButton.Equals(MouseButtonState.Pressed))
            {
                SKRect selectionBox = LCursor.StopClickDrag();
                if (!selectionBox.Equals(SKRect.Empty))
                {
                    LLines.HandleBoxSelect(selectionBox);
                }
                LCursor.CursorPoint = LCursor.RoundToNearestIntersection(LCursor.CanvasPoint);
            }
            else
            {
                LCursor.StartClickDrag();
            }
            MainCanvas.InvalidateVisual();

            CursorStatusTextBlock.Text = "Cursor position: ( " + LCursor.GetCursorGridPoints().X + " , " + LCursor.GetCursorGridPoints().Y + " )";
            CursorStatusBar.InvalidateVisual();
        }

        private void MainCanvas_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            bool anyLayerRedraw = false;
            foreach (KeyValuePair<string,IDataLayer> l in PageData.Instance.GetDataLayers())
            {
                if (l.Value.IsRedrawRequired())
                {
                    anyLayerRedraw = true;
                }
            }

            if (!anyLayerRedraw)
            {
                return;
            }

            MainCanvas.Width = LGrid.GenerateLayerBitmap().Width;
            MainCanvas.Height = LGrid.GenerateLayerBitmap().Height;

            e.Surface.Canvas.Clear(SKColors.White);

            foreach (KeyValuePair<string, IDataLayer> l in PageData.Instance.GetDataLayers())
            {
                e.Surface.Canvas.DrawBitmap(l.Value.GenerateLayerBitmap(), l.Value.GetRenderPoint());
            }
        }

        private void MainCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                SKPointI target = LCursor.RoundToNearestIntersection(e.GetPosition(MainCanvas));
                SKPointI targetGrid = LCursor.GetCursorGridPoints();
                LPreview.HandleCreationClick(target, targetGrid);
                MainMenuBar.CheckEditButtonValidity();
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                LLines.HandleSelectionClick(e.GetPosition(MainCanvas));
            }
            MainCanvas.InvalidateVisual();
        }

        private void VGraphWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    LLines.DeleteSelectedLines();
                    MainCanvas.InvalidateVisual();
                    break;

                case Key.W:
                    LLines.MoveSelectedLines(0, -1);
                    break;

                case Key.D:
                    LLines.MoveSelectedLines(1, 0);
                    break;

                case Key.S:
                    LLines.MoveSelectedLines(0, 1);
                    break;

                case Key.A:
                    LLines.MoveSelectedLines(-1, 0);
                    break;

                case Key.Add:
                    PageData.Instance.ZoomIn();
                    break;

                case Key.Subtract:
                    PageData.Instance.ZoomOut();
                    break;
            }
            MainCanvas.InvalidateVisual();
        }

        public void ForceRedraw()
        {
            MainCanvas.InvalidateVisual();
        }
    }
}
