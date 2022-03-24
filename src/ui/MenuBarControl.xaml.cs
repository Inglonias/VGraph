using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32;

using VGraph.src.config;
using VGraph.src.dataLayers;

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
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.SelectTool(tool);
        }

        public void Undo()
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.UndoLastAction();
            CheckEditButtonValidity();
            MainWindowParent.MainCanvas.InvalidateVisual();
            InvalidateVisual();
        }

        public void Redo()
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.RedoLastAction();
            CheckEditButtonValidity();
            MainWindowParent.MainCanvas.InvalidateVisual();
            InvalidateVisual();
        }

        public void CheckEditButtonValidity()
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            Undo_Button.IsEnabled = ll.CanUndo();
            Redo_Button.IsEnabled = ll.CanRedo();
        }

        public void MergeLines()
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.MergeAllLines();
        }

        public void MirrorLines()
        {
            MirrorToolWindow mirrorTool = new MirrorToolWindow();
            mirrorTool.Show();
        }

        public void ToggleCenterLines()
        {
            GridBackgroundLayer gbl = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
            Center_Lines_Button.IsChecked = gbl.ToggleCenterLines();
            MainWindowParent.MainCanvas.InvalidateVisual();
        }

        private void OddMode_OnClick(object sender, RoutedEventArgs e)
        {
            PreviewLayer pl = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER);
            pl.OddMode = OddModeCheckbox.IsChecked.Value;
        }

        private void ColorSwatch_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(PageData.Instance.CurrentLineColor);
        }

        private void ColorSelect_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.Color = PageData.Instance.GetLineColorAsColor();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PageData.Instance.CurrentLineColor = new SkiaSharp.SKColor(cd.Color.R, cd.Color.G, cd.Color.B);
                ColorSwatch.InvalidateVisual();
            }
        }
    }
}
