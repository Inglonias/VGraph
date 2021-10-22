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
            try
            {
                PageData.Instance.SquaresWide = Math.Max(1, Convert.ToInt32(GridSquaresWide.Text));
                PageData.Instance.SquaresTall = Math.Max(1, Convert.ToInt32(GridSquaresTall.Text));
                PageData.Instance.SquareSize = Math.Max(4, Convert.ToInt32(GridSquareSize.Text));
                PageData.Instance.TrueSquareSize = Math.Max(4, Convert.ToInt32(GridSquareSize.Text));
                PageData.Instance.Margin = Math.Max(0, Convert.ToInt32(PageMargin.Text));
                GridBackgroundLayer gbl = (GridBackgroundLayer)PageData.Instance.GetDataLayers()[PageData.GRID_LAYER];
                LineLayer ll = (LineLayer)PageData.Instance.GetDataLayers()[PageData.LINE_LAYER];

                gbl.ForceRedraw();
                ll.ClearAllLines();

                Close();
            }
            catch (FormatException)
            {

            }
        }

        private void NewGridWindow_OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
