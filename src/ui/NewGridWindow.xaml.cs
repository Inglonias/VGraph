using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for NewGridWindow.xaml
    /// </summary>
    public partial class NewGridWindow : Window
    {
        public NewGridWindow()
        {
            InitializeComponent();
        }

        private void NewGridWindow_OnOK(object sender, RoutedEventArgs e)
        {
            PageData.Instance.SquaresWide = Convert.ToInt32(GridSquaresWide.Text);
            PageData.Instance.SquaresTall = Convert.ToInt32(GridSquaresTall.Text);
            PageData.Instance.SquareSize  = Convert.ToInt32(GridSquareSize.Text);
            PageData.Instance.Margin      = Convert.ToInt32(PageMargin.Text);
            GridBackgroundLayer gbl = (GridBackgroundLayer)PageData.Instance.GetDataLayers()[0];
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayers()[1];

            gbl.RequireLayerRedraw();
            ll.ClearAllLines();

            Close();
        }

        private void NewGridWindow_OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
