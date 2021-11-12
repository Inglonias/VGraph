using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        }

        private void MenuBar_OnNewGrid(object sender, RoutedEventArgs e)
        {
            NewGridWindow ngw = new NewGridWindow();
            ngw.Show();
        }

        private void MenuBar_OnOpenFile(object sender, RoutedEventArgs e)
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

        private void MenuBar_OnSaveFile(object sender, RoutedEventArgs e)
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

        private void MenuBar_OnExportFile(object sender, RoutedEventArgs e)
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

        private void MenuBar_OnExit(object sender, RoutedEventArgs e)
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
            toolMenuItems.Add(Box_Tool);
            toolMenuItems.Add(Circle_Tool);
            toolMenuItems.Add(Boxy_Circle_Tool);

            foreach (ToggleButton m in toolMenuItems)
            {
                m.IsChecked = m.Name.Equals(tool);
            }
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.SelectTool(tool);
        }

        private void MenuBar_OnUndo(object sender, RoutedEventArgs e)
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.UndoLastAction();
            MainWindowParent.MainCanvas.InvalidateVisual();
        }

        private void MenuBar_OnRedo(object sender, RoutedEventArgs e)
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            ll.RedoLastAction();
            MainWindowParent.MainCanvas.InvalidateVisual();
        }
    }
}
