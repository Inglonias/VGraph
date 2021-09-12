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

        private bool MenuBar_OnOpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog
            {
                DefaultExt = ".vgp",
                Filter = "VGraph JSON files (.vgp)|*.vgp|JSON Files (.json)|*.json"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                result = PageData.Instance.FileOpen(d.FileName);
            }
            return (bool)result;
        }

        private bool MenuBar_OnSaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog
            {
                DefaultExt = ".vgp",
                Filter = "VGraph JSON files (.vgp)|*.vgp|JSON Files (.json)|*.json"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                result = PageData.Instance.FileSave(d.FileName);
            }
            return (bool)result;
        }

        private bool MenuBar_OnExportFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "Portable network graphic (.png)|*.png"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                result = PageData.Instance.FileExport(d.FileName);
            }
            return (bool)result;
        }

        private void MenuBar_OnExit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
