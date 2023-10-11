using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SkiaSharp;

using VGraph.src.config;
using VGraph.src.dataLayers;
using VGraph.src.objects;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace VGraph.src.ui
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int VIEWPORT_BORDER = 200;
        private static bool FrameDrawAllowed = true;
        private readonly System.Timers.Timer FrameCapTimer;
        private readonly GridBackgroundLayer LGrid;
        private readonly LineLayer LLines;
        private readonly PreviewLayer LPreview;
        private readonly CursorLayer LCursor;
        private readonly TextLayer LText;
        private readonly History<long> FrameRateHistory = new History<long>(10);

        public MainWindow()
        {
            LGrid = new GridBackgroundLayer();
            LLines = new LineLayer();
            LPreview = new PreviewLayer();
            LCursor = new CursorLayer();
            LText = new TextLayer();
            //LText.AddTextLabel(new SKPointI(10, 10),"Hello world", "#ff000000", TextLabel.ALIGN_CENTER_CENTER);
            AssignPageData();
            FrameCapTimer = new System.Timers.Timer(1000.0 / ConfigOptions.Instance.MaxFrameRate);
            FrameCapTimer.Elapsed += AllowFrameDraw;
            FrameCapTimer.AutoReset = true;
            FrameCapTimer.Enabled = true;
            float scale = (float)(Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth);
            if (scale != 1.00f)
            {
            }
            InitializeComponent();
            MainMenuBar.MainWindowParent = this;
            PageData.Instance.MainWindow = this;
            LGrid.GenerateLayerBitmap();
            MainCanvas.Width = PageData.Instance.GetTotalWidth();
            MainCanvas.Height = PageData.Instance.GetTotalHeight();
        }

        private static void AllowFrameDraw(Object source, ElapsedEventArgs e)
        {
            FrameDrawAllowed = true;
        }

        private void AssignPageData()
        {
            PageData.Instance.GetDataLayers()[PageData.GRID_LAYER] = LGrid;
            PageData.Instance.GetDataLayers()[PageData.LINE_LAYER] = LLines;
            PageData.Instance.GetDataLayers()[PageData.PREVIEW_LAYER] = LPreview;
            PageData.Instance.GetDataLayers()[PageData.TEXT_LAYER] = LText;
            PageData.Instance.GetDataLayers()[PageData.CURSOR_LAYER] = LCursor;
        }

        private void MainCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            HandleCursor(e);
            CursorStatusTextBlock.Text = "X: " + LCursor.GetCursorGridPoints().X + " , Y: " + LCursor.GetCursorGridPoints().Y;
            StatusBar.InvalidateVisual();
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
                    LText.HandleBoxSelect(selectionBox, maintainSelection);
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
            float scale = (float)(Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth);
            if (!FrameDrawAllowed)
            {
                return;
            }
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool anyLayerRedraw = false;
            foreach (KeyValuePair<string, IDataLayer> l in PageData.Instance.GetDataLayers())
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

            if (scale == 1.00f)
            {
                MainCanvas.Width = PageData.Instance.GetTotalWidth();
                MainCanvas.Height = PageData.Instance.GetTotalHeight();
            }
            else
            {
                MainCanvas.Width = PageData.Instance.GetTotalWidth() / scale;
                MainCanvas.Height = PageData.Instance.GetTotalHeight() / scale;
            }
            e.Surface.Canvas.Clear(ConfigOptions.Instance.BackgroundPaperColor);
            int viewTop = Math.Max(0, Convert.ToInt32(Math.Floor(PrimaryBufferPanel.VerticalOffset - VIEWPORT_BORDER)));
            int viewLeft = Math.Max(0, Convert.ToInt32(Math.Floor(PrimaryBufferPanel.HorizontalOffset - VIEWPORT_BORDER)));

            SKRectI viewport = new SKRectI
            {
                Top = viewTop,
                Left = viewLeft,
                Right = viewLeft + Convert.ToInt32(PrimaryBufferPanel.ViewportWidth + VIEWPORT_BORDER),
                Bottom = viewTop + Convert.ToInt32(PrimaryBufferPanel.ViewportHeight + VIEWPORT_BORDER)
            };
            if (scale != 1.00f)
            {
                viewport.Right = (int)Math.Round(viewport.Right * scale);
                viewport.Bottom = (int)Math.Round(viewport.Bottom * scale);
            }
            //Only happens during initial render.
            if (PrimaryBufferPanel.ViewportWidth == 0 || PrimaryBufferPanel.ViewportHeight == 0)
            {
                viewport.Right = PageData.Instance.GetTotalWidth();
                viewport.Bottom = PageData.Instance.GetTotalHeight();
            }
            SKBitmap drawingImage = new SKBitmap(new SKImageInfo(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight()));
            SKCanvas drawingSurface = new SKCanvas(drawingImage);
            /* Actually render the layers.
             * The goal here is to do everything in our power to avoid having to actually render anything, since rendering is the single
             * most expensive thing we do here. */
            foreach (KeyValuePair<string, IDataLayer> l in PageData.Instance.GetDataLayers())
            {
                SKBitmap fullLayer = l.Value.GenerateLayerBitmap();
                if (scale != 1.00f && PrimaryBufferPanel.Width > 0)
                {
                    PrimaryBufferPanel.Width = (int)Math.Round(PrimaryBufferPanel.Width / scale);
                    PrimaryBufferPanel.Height = (int)Math.Round(PrimaryBufferPanel.Height / scale);
                }
                if (fullLayer != null)
                {
                    int layerLeft = Math.Max(0, viewport.Left - l.Value.GetRenderPoint().X);
                    int layerTop = Math.Max(0, viewport.Top - l.Value.GetRenderPoint().Y);
                    int layerRight = Math.Min(fullLayer.Width, viewport.Right - l.Value.GetRenderPoint().X);
                    int layerBottom = Math.Min(fullLayer.Height, viewport.Bottom - l.Value.GetRenderPoint().Y);
                    SKRectI layerViewport = new SKRectI(layerLeft, layerTop, layerRight, layerBottom);
                    SKRectI actualPosition = new SKRectI()
                    {
                        Left = l.Value.GetRenderPoint().X,
                        Top = l.Value.GetRenderPoint().Y,
                        Right = l.Value.GetRenderPoint().X + fullLayer.Width,
                        Bottom = l.Value.GetRenderPoint().Y + fullLayer.Height
                    };
                    //Don't render if we won't even see it.
                    if (viewport.IntersectsWith(actualPosition))
                    {
                        //If the layer is smaller than the viewport, don't resize it!
                        if ((layerViewport.Width < viewport.Width) || (layerViewport.Height < viewport.Height))
                        {
                            drawingSurface.DrawBitmap(fullLayer, l.Value.GetRenderPoint());
                        }
                        else
                        {
                            drawingSurface.DrawBitmap(fullLayer, layerViewport, viewport);
                        }
                    }
                }
            }
            e.Surface.Canvas.DrawBitmap(drawingImage, viewport, viewport);
            drawingImage.Dispose();
            drawingSurface.Dispose();
            this.Title = PageData.Instance.GetWindowTitle();
            sw.Stop();
            FrameRateHistory.Push(sw.ElapsedMilliseconds);
            ToolStatusTextBlock.Text = ((PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER)).GetStatusText();
            DrawTimeTextBlock.Text = "Draw Time: " + GetDrawTime() + " ms";
            StatusBar.InvalidateVisual();
            FrameDrawAllowed = false;
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
                LText.HandleCreationClick(target, targetGrid);
                LPreview.HandleCreationClick(target, targetGrid);
                MainMenuBar.CheckEditButtonValidity();
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                float scale = (float)(Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth);
                var targetPosition = e.GetPosition(MainCanvas);
                if (scale != 1.00f)
                {
                    targetPosition.X = targetPosition.X * scale;
                    targetPosition.Y = targetPosition.Y * scale;
                }
                bool maintainSelection = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
                bool selectionMade = LLines.HandleSelectionClick(targetPosition, maintainSelection) || LText.HandleSelectionClick(targetPosition, maintainSelection);
                if (selectionMade && PageData.Instance.IsEyedropperActive)
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
        
        private void MainCanvas_OnScroll(object sender, ScrollChangedEventArgs e)
        {
            MainCanvas.InvalidateVisual();
        }

        private void VGraphMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = MainMenuBar.ExitApp();
        }
    }
}
