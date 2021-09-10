using System;
using System.Windows;
using System.Windows.Controls;

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
    }
}
