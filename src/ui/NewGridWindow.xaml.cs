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
            PageMarginX.Text = Convert.ToString(PageData.Instance.MarginX);
            PageMarginY.Text = Convert.ToString(PageData.Instance.MarginX);
        }

        private void NewGridWindow_OnOK(object sender, RoutedEventArgs e)
        {
            try
            {
                PageData.Instance.SquaresWide = Math.Max(1, Convert.ToInt32(GridSquaresWide.Text));
                PageData.Instance.SquaresTall = Math.Max(1, Convert.ToInt32(GridSquaresTall.Text));
                PageData.Instance.SquareSize = Math.Min(128, Math.Max(4, Convert.ToInt32(GridSquareSize.Text)));
                PageData.Instance.TrueSquareSize = Math.Min(128, Math.Max(4, Convert.ToInt32(GridSquareSize.Text)));
                PageData.Instance.MarginX = Math.Max(0, Convert.ToInt32(PageMarginX.Text));
                PageData.Instance.MarginX = Math.Max(0, Convert.ToInt32(PageMarginY.Text));
                GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayers()[PageData.GRID_LAYER];
                LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayers()[PageData.LINE_LAYER];

                gridBackgroundLayer.ForceRedraw();
                lineLayer.ClearAllLines();

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
