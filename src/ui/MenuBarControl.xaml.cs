﻿using System;
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
using static VGraph.src.objects.PageHistory;

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

            GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
            CenterLinesButton.IsChecked = gridBackgroundLayer.DrawCenterLines;
            GridLinesButton.IsChecked = gridBackgroundLayer.DrawGridLines;
            BackgroundImageButton.IsChecked = gridBackgroundLayer.DrawBackgroundImage;
        }

        public void CreateNewGrid(bool deleteLines)
        {
            if (deleteLines)
            {
                if (CheckUnsavedChanges())
                {
                    return;
                }
            }
            NewGridWindow ngw = new NewGridWindow(deleteLines);
            ngw.MainWindowParent = MainWindowParent;
            ngw.Show();
        }

        public void ShowConfigWindow()
        {
            ConfigOptionsWindow cow = new ConfigOptionsWindow();
            cow.Show();
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
                MainWindowParent.Title = "VGraph - " + System.IO.Path.GetFileName(PageData.Instance.LastSavePath);
            }
        }

        public void SaveGrid()
        {
            if (PageData.Instance.LastSavePath.Length == 0)
            {
                SaveGridAs();
            }
            else
            {
                if (PageData.Instance.FileSave())
                {
                    MainWindowParent.Title = PageData.Instance.GetWindowTitle();
                }
            }
        }

        public void SaveGridAs()
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
                MainWindowParent.Title = PageData.Instance.GetWindowTitle();
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

        private void ExportGridLines_OnClick(object sender, RoutedEventArgs e)
        {
            ExportGridLines.IsChecked = !ExportGridLines.IsChecked;
            PageData.Instance.ExportGridLines = ExportGridLines.IsChecked;
        }

        private void ExportCenterLines_OnClick(object sender, RoutedEventArgs e)
        {
            ExportCenterLines.IsChecked = !ExportCenterLines.IsChecked;
            PageData.Instance.ExportCenterLines = ExportCenterLines.IsChecked;
        }

        private void ExportBackgroundImage_OnClick(object sender, RoutedEventArgs e)
        {
            ExportBackgroundImage.IsChecked = !ExportBackgroundImage.IsChecked;
            PageData.Instance.ExportBackgroundImage = ExportBackgroundImage.IsChecked;
        }

        public bool ExitApp()
        {
            return CheckUnsavedChanges();
        }

        private bool CheckUnsavedChanges()
        {
            if (PageData.Instance.IsCanvasDirty)
            {
                if (MessageBox.Show("You have unsaved changes. Are you sure you want to continue?", "Warning - Unsaved changes", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return true;
                }
            }
            return false;
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
            toolMenuItems.Add(Text_Tool);

            foreach (ToggleButton m in toolMenuItems)
            {
                m.IsChecked = m.Name.Equals(tool);
            }
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            lineLayer.SelectTool(tool);
            textLayer.SelectTool(tool);
        }

        public void Undo()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            PageHistory.Instance.CreateRedoPoint(lineLayer.LineList, textLayer.LabelList);
            PageState ps = PageHistory.Instance.PopUndoAction();

            if (ps.Lines != null)
            {
                lineLayer.LineList = ps.Lines;
                lineLayer.ForceRedraw();
            }
            if (ps.Labels != null)
            {
                textLayer.LabelList = ps.Labels;
                textLayer.ForceRedraw();
            }
            CheckEditButtonValidity();
            MainWindowParent.MainCanvas.InvalidateVisual();
            InvalidateVisual();
        }

        public void Redo()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            PageHistory.Instance.CreateUndoPoint(lineLayer.LineList, textLayer.LabelList, false);
            PageState ps = PageHistory.Instance.PopRedoAction();

            if (ps.Lines != null)
            {
                lineLayer.LineList = ps.Lines;
                lineLayer.ForceRedraw();
            }
            if (ps.Labels != null)
            {
                textLayer.LabelList = ps.Labels;
                textLayer.ForceRedraw();
            }
            CheckEditButtonValidity();
            MainWindowParent.MainCanvas.InvalidateVisual();
            InvalidateVisual();
        }

        public void CheckEditButtonValidity()
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            UndoButton.IsEnabled = PageHistory.Instance.CanUndo();
            RedoButton.IsEnabled = PageHistory.Instance.CanRedo();
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
            CenterLinesButton.IsChecked = gridBackgroundLayer.ToggleCenterLines();
            MainWindowParent.MainCanvas.InvalidateVisual();
        }

        public void ToggleGridLines()
        {
            GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
            GridLinesButton.IsChecked = gridBackgroundLayer.ToggleGridLines();
            MainWindowParent.MainCanvas.InvalidateVisual();
        }

        public void ToggleBackgroundImage()
        {
            GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
            BackgroundImageButton.IsChecked = gridBackgroundLayer.ToggleBackgroundImage();
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
                    PageHistory.Instance.CreateUndoPoint(lineLayer.LineList, null, true);
                    foreach (LineSegment l in selectedLines)
                    {
                        l.LineColor = colorNew.ToString();
                    }
                    lineLayer.ForceRedraw();
                    PageData.Instance.MakeCanvasDirty();
                    MainWindowParent.MainCanvas.InvalidateVisual();
                }

                PageData.Instance.CurrentLineColor = colorNew;
                ColorSwatch.InvalidateVisual();
            }
        }

        private void Eyedropper_OnChecked(object sender, RoutedEventArgs e)
        {
            PageData.Instance.IsEyedropperActive = true;
            InvalidateVisual();
        }

        private void Eyedropper_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PageData.Instance.IsEyedropperActive = false;
            InvalidateVisual();
        }
    }
}
