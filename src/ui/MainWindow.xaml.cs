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
            HandleCursor(e);
            MainCanvas.InvalidateVisual();

            CursorStatusTextBlock.Text = "Cursor position: ( " + LCursor.GetCursorGridPoints().X + " , " + LCursor.GetCursorGridPoints().Y + " )";
            CursorStatusBar.InvalidateVisual();
        }

        private void HandleCursor(MouseEventArgs e)
        {
            LCursor.CanvasPoint = e.GetPosition(MainCanvas);
            if (!Mouse.LeftButton.Equals(MouseButtonState.Pressed))
            {
                SKRect selectionBox = LCursor.StopClickDrag();
                bool maintainSelection = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
                if (!selectionBox.Equals(SKRect.Empty))
                {
                    LLines.HandleBoxSelect(selectionBox, maintainSelection);
                }
                LCursor.CursorPoint = LCursor.RoundToNearestIntersection(LCursor.CanvasPoint);
            }
            else
            {
                LCursor.StartClickDrag();
            }
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
                bool maintainSelection = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
                if (LLines.HandleSelectionClick(e.GetPosition(MainCanvas), maintainSelection) && PageData.Instance.IsEyedropperActive)
                {
                    PageData.Instance.IsEyedropperActive = false;
                    MainMenuBar.Eyedropper_Tool.IsChecked = false;
                }
                MainMenuBar.InvalidateVisual();
                MainMenuBar.ColorSwatch.InvalidateVisual();
            }
            MainCanvas.InvalidateVisual();
        }

        public void ForceRedraw()
        {
            MainCanvas.InvalidateVisual();
        }

        private void MainCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            HandleCursor(e);
            MainCanvas.InvalidateVisual();
        }
    }
}
