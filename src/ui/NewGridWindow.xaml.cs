using System;
using System.Windows;

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
            GridSquaresWide.Text = Convert.ToString(PageData.Instance.SquaresWide);
            GridSquaresTall.Text = Convert.ToString(PageData.Instance.SquaresTall);
            GridSquareSize.Text = Convert.ToString(PageData.Instance.SquareSize);
            PageMargin.Text = Convert.ToString(PageData.Instance.Margin);
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
