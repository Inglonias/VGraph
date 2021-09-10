using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using VGraph.src.config;


namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for MenuBarControl.xaml
    /// </summary>
    public partial class MenuBarControl : UserControl
    {

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
            OpenFileDialog d = new OpenFileDialog();
            d.DefaultExt = ".vgp";
            d.Filter = "VGraph JSON files (.vgp)|*.vgp|JSON Files (.json)|*.json";
            Nullable<bool> result = d.ShowDialog();

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
            Nullable<bool> result = d.ShowDialog();

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
            Nullable<bool> result = d.ShowDialog();

            if (result == true)
            {
                PageData.Instance.FileExport(d.FileName);
            }
        }

        private void MenuBar_OnExit(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
