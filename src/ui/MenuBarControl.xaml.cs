using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.dataLayers;
using VGraph.src.objects;

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for MenuBarControl.xaml
    /// </summary>
    public partial class MenuBarControl : UserControl
    {
        public MainWindow MainWindowParent { get; set; }
        public MenuBarControl()
        {
            InitializeComponent();
            CheckEditButtonValidity();
        }

        public void CreateNewGrid()
        {
            NewGridWindow ngw = new NewGridWindow();
            ngw.Show();
        }


        public void OpenGrid()
        {
            OpenFileDialog d = new OpenFileDialog
            {
                DefaultExt = ".vgp",
                Filter = "VGraph JSON files (.vgp)|*.vgp|JSON Files (.json)|*.json"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                PageData.Instance.FileOpen(d.FileName);
            }
        }

        public void SaveGrid()
        {
            SaveFileDialog d = new SaveFileDialog
            {
                DefaultExt = ".vgp",
                Filter = "VGraph JSON files (.vgp)|*.vgp|JSON Files (.json)|*.json"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                PageData.Instance.FileSave(d.FileName);
            }
        }

        public void ExportGrid()
        {
            SaveFileDialog d = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "Portable network graphic (.png)|*.png"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                PageData.Instance.FileExport(d.FileName);
            }
        }

        public void ImportGrid()
        {
            OpenFileDialog d = new OpenFileDialog
            {
                DefaultExt = ".vgp",
                Filter = "VGraph JSON files (.vgp)|*.vgp|JSON Files (.json)|*.json"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                PageData.Instance.FileImport(d.FileName);
            }
        }

        public void ExitApp()
        {
            Environment.Exit(0);
        }

        private void ToolMenu_OnChecked(object sender, RoutedEventArgs e)
        {
            ToggleButton toolClicked = (ToggleButton)sender;
            string targetTool = (string)toolClicked.Name;
            SelectTool(targetTool);
            InvalidateVisual();
        }

        private void SelectTool(string tool)
        {
            List<ToggleButton> toolMenuItems = new List<ToggleButton>();

            toolMenuItems.Add(Line_Tool);
            toolMenuItems.Add(Tri_Tool);
            toolMenuItems.Add(Box_Tool);
            toolMenuItems.Add(Circle_Tool);
            toolMenuItems.Add(Boxy_Circle_Tool);
            toolMenuItems.Add(Ellipse_Tool);

            foreach (ToggleButton m in toolMenuItems)
            {
                m.IsChecked = m.Name.Equals(tool);
            }
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lineLayer.SelectTool(tool);
        }

        public void Undo()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lineLayer.UndoLastAction();
            CheckEditButtonValidity();
            MainWindowParent.MainCanvas.InvalidateVisual();
            InvalidateVisual();
        }

        public void Redo()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lineLayer.RedoLastAction();
            CheckEditButtonValidity();
            MainWindowParent.MainCanvas.InvalidateVisual();
            InvalidateVisual();
        }

        public void CheckEditButtonValidity()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            Undo_Button.IsEnabled = lineLayer.CanUndo();
            Redo_Button.IsEnabled = lineLayer.CanRedo();
        }

        public void MergeLines()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lineLayer.MergeAllLines();
        }

        public void MirrorLines()
        {
            MirrorToolWindow mirrorTool = new MirrorToolWindow();
            mirrorTool.Show();
        }

        public void ToggleCenterLines()
        {
            GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
            Center_Lines_Button.IsChecked = gridBackgroundLayer.ToggleCenterLines();
            MainWindowParent.MainCanvas.InvalidateVisual();
        }

        private void OddMode_OnClick(object sender, RoutedEventArgs e)
        {
            PreviewLayer previewLayer = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER);
            previewLayer.OddMode = OddModeCheckbox.IsChecked.Value;
        }

        private void ColorSwatch_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(PageData.Instance.CurrentLineColor);
        }

        private void ColorSelect_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog
            {
                AllowFullOpen = true,
                Color = Color.FromArgb(PageData.Instance.CurrentLineColor.Red,
                                       PageData.Instance.CurrentLineColor.Green,
                                       PageData.Instance.CurrentLineColor.Blue)
            };
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SKColor colorNew = new SKColor(cd.Color.R, cd.Color.G, cd.Color.B);
                LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
                LineSegment[] selectedLines = lineLayer.GetSelectedLines();

                if (selectedLines.Length > 0)
                {
                    lineLayer.CreateUndoPoint();
                    foreach (LineSegment l in selectedLines)
                    {
                        l.LineColor = colorNew.ToString();
                    }
                    lineLayer.ForceRedraw();
                    MainWindowParent.MainCanvas.InvalidateVisual();
                }

                PageData.Instance.CurrentLineColor = colorNew;
                ColorSwatch.InvalidateVisual();
            }
        }
    }
}
