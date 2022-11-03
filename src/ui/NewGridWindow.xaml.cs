using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for NewGridWindow.xaml
    /// </summary>
    public partial class NewGridWindow : Window
    {
        public MainWindow MainWindowParent { get; set; }

        public bool DeleteLines;
        public NewGridWindow(bool deleteLines)
        {
            InitializeComponent();
            if (deleteLines)
            {
                this.Title = "Create New Grid";
            }
            else
            {
                this.Title = "Resize Grid";
            }
            DeleteLines = deleteLines;
            InvalidateVisual();
            GridSquaresWide.Text = Convert.ToString(PageData.Instance.SquaresWide);
            GridSquaresTall.Text = Convert.ToString(PageData.Instance.SquaresTall);
            GridSquareSize.Text = Convert.ToString(PageData.Instance.TrueSquareSize);
            PageMarginX.Text = Convert.ToString(PageData.Instance.MarginX);
            PageMarginY.Text = Convert.ToString(PageData.Instance.MarginY);
            BackgroundOpacitySlider.Value = PageData.Instance.BackgroundImageAlpha;
            BackgroundOpacityTextBox.Text = PageData.Instance.BackgroundImageAlpha.ToString();
            ImagePathBox.Text = PageData.Instance.BackgroundImagePath;
            CalculateImageInfo();

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
                PageData.Instance.MarginY = Math.Max(0, Convert.ToInt32(PageMarginY.Text));
                PageData.Instance.SetBackgroundImage(ImagePathBox.Text);
                PageData.Instance.BackgroundImageAlpha = Convert.ToByte(BackgroundOpacityTextBox.Text);
                GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayers()[PageData.GRID_LAYER];
                LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayers()[PageData.LINE_LAYER];

                gridBackgroundLayer.ForceRedraw();
                if (DeleteLines)
                {
                    PageData.Instance.MakeCanvasClean();
                    lineLayer.ClearAllLines();
                }
                lineLayer.ForceRedraw();
                MainWindowParent.MainCanvas.InvalidateVisual();
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

        private void GridDimensionBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CalculateImageInfo();
        }

        private void BackgroundImageBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog
            {
                DefaultExt = ".vgp",
                Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"
            };
            bool? result = d.ShowDialog();

            if (result == true)
            {
                ImagePathBox.Text = d.FileName;
                CalculateImageInfo();
            }
        }

        private void ImagePathBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            CalculateImageInfo();
        }

        private void CalculateImageInfo()
        {
            const string templateText = "Canvas size ----- (px) : [CANVSIZE]\nGrid size ------- (px) : [GRIDSIZE]\nBackground size - (px) : [BACKSIZE]";
            int squaresWide;
            int squaresTall;
            int squareSize;
            int marginX;
            int marginY;
            bool anyInfoValid = false;
            ImageInfoTextBlock.Text = "Please enter valid info for image size data.";
            string displayText = templateText;
            try
            {
                squaresWide = Convert.ToInt32(GridSquaresWide.Text);
                squaresTall = Convert.ToInt32(GridSquaresTall.Text);
                squareSize = Convert.ToInt32(GridSquareSize.Text);
                marginX = Convert.ToInt32(PageMarginX.Text);
                marginY = Convert.ToInt32(PageMarginY.Text);

                anyInfoValid = true;

                int gridWidth    = squaresWide * squareSize;
                int gridHeight   = squaresTall * squareSize;
                int canvasWidth  = gridWidth   + (marginX * 2);
                int canvasHeight = gridHeight  + (marginY * 2);

                string canvSizeString = canvasWidth.ToString().PadLeft(6) + " x " + canvasHeight.ToString().PadRight(6);
                string gridSizeString = gridWidth.ToString().PadLeft(6) + " x " + gridHeight.ToString().PadRight(6);
                displayText = displayText.Replace("[CANVSIZE]", canvSizeString);
                displayText = displayText.Replace("[GRIDSIZE]", gridSizeString);
            }
            catch (FormatException)
            {
                displayText = displayText.Replace("[CANVSIZE]", "   N/A x N/A   ");
                displayText = displayText.Replace("[GRIDSIZE]", "   N/A x N/A   ");
            }

            if (File.Exists(ImagePathBox.Text))
            {
                anyInfoValid = true;
                SKImageInfo backgroundInfo = SKBitmap.Decode(new SKFileStream(ImagePathBox.Text)).Info;

                int backWidth = backgroundInfo.Width;
                int backHeight = backgroundInfo.Height;

                string backSizeString = backWidth.ToString().PadLeft(6) + " x " + backHeight.ToString().PadRight(6);
                displayText = displayText.Replace("[BACKSIZE]", backSizeString);
            }
            else
            {
                displayText = displayText.Replace("[BACKSIZE]", "   N/A x N/A   ");
            }
            if (anyInfoValid)
            {
                ImageInfoTextBlock.Text = displayText;
            }
            ImageInfoTextBlock.InvalidateVisual();
        }

        private void BackgroundOpacitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (BackgroundOpacityTextBox != null)
            {
                BackgroundOpacityTextBox.Text = Convert.ToInt32(BackgroundOpacitySlider.Value).ToString();
            }
        }
    }
}
