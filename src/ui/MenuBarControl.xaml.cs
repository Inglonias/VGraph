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

        private void LineModeItem_OnUncheck(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Unchecked");
            PageData.Instance.LineModeActive = false;
        }

        private void LineModeItem_OnCheck(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Checked");
            PageData.Instance.LineModeActive = true;
        }
    }
}
