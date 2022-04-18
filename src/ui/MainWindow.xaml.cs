using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using SkiaSharp;

using VGraph.src.config;
using VGraph.src.dataLayers;
using VGraph.src.objects;

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
        private readonly History<long> FrameRateHistory = new History<long>(30);

        public MainWindow()
        {
            LGrid = new GridBackgroundLayer();
            LLines = new LineLayer();
            LPreview = new PreviewLayer();
            LCursor = new CursorLayer();

            AssignPageData();

            InitializeComponent();
            MainMenuBar.MainWindowParent = this;
            LGrid.GenerateLayerImage();
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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
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

            MainCanvas.Width = PageData.Instance.GetTotalWidth();
            MainCanvas.Height = PageData.Instance.GetTotalHeight();

            e.Surface.Canvas.Clear(SKColors.White);
            int viewTop = Math.Max(0, Convert.ToInt32(Math.Floor(PrimaryBufferPanel.VerticalOffset - 100)));
            int viewLeft = Math.Max(0, Convert.ToInt32(Math.Floor(PrimaryBufferPanel.HorizontalOffset - 100)));

            SKRectI viewport = new SKRectI
            {
                Top = viewTop,
                Left = viewLeft,
                Right = viewLeft + Convert.ToInt32(PrimaryBufferPanel.ViewportWidth + 100),
                Bottom = viewTop + Convert.ToInt32(PrimaryBufferPanel.ViewportHeight + 100)
            };
            SKSurface drawingSurface = SKSurface.Create(new SKImageInfo(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight()));

            foreach (KeyValuePair<string, IDataLayer> l in PageData.Instance.GetDataLayers())
            {
                SKImage renderThis = l.Value.GenerateLayerImage();
                if (renderThis != null)
                {
                    drawingSurface.Canvas.DrawImage(renderThis, l.Value.GetRenderPoint());
                    //renderThis.Dispose();
                }
                else
                {
                    Console.WriteLine(l.Value.GetType().Name + " did not render!");
                }
            }
            e.Surface.Canvas.DrawImage(drawingSurface.Snapshot(viewport), new SKPointI(viewLeft, viewTop));
            drawingSurface.Dispose();
            sw.Stop();
            FrameRateHistory.Push(sw.ElapsedMilliseconds);
            CursorStatusTextBlock.Text = "Cursor position: ( " + LCursor.GetCursorGridPoints().X + " , " + LCursor.GetCursorGridPoints().Y + " )        Avg. Draw Time (ms): " + GetDrawTime();
            CursorStatusBar.InvalidateVisual();
        }

        private string GetDrawTime()
        {
            long sum = 0;
            foreach (long l in FrameRateHistory)
            {
                sum += l;
            }

            return (sum / Convert.ToDouble(FrameRateHistory.Count)).ToString();
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

        private void VGraphMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainMenuBar.ExitApp();
        }
    }
}
