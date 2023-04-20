using System;
using System.Drawing;
using System.Windows;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.dataLayers;
using VGraph.src.objects;

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for LabelPropertiesWindow.xaml
    /// </summary>
    public partial class LabelPropertiesWindow : Window
    {
        public LabelPropertiesWindow()
        {
            InitializeComponent();
            ColorSwatch.InvalidateVisual();
        }

        public SKPointI TargetGridPoint { get; set; }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Okay_OnClick(object sender, RoutedEventArgs e)
        {
            TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            lText.AddTextLabel(TargetGridPoint, TextBoxLabelText.Text, PageData.Instance.CurrentLabelColor.ToString(), ((System.Windows.Media.FontFamily)ComboBoxFonts.SelectedItem).Source, Convert.ToInt32(TextBoxFontSize.Text), ComboBoxAlignment.SelectedIndex);
            Close();
        }

        private void ColorSelect_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog
            {
                AllowFullOpen = true,
                Color = Color.FromArgb(PageData.Instance.CurrentLineColor.Red,
                                       PageData.Instance.CurrentLineColor.Green,
                                       PageData.Instance.CurrentLineColor.Blue)
            };
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SKColor colorNew = new SKColor(cd.Color.R, cd.Color.G, cd.Color.B);
                LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
                LineSegment[] selectedLines = lineLayer.GetSelectedLines();

                PageData.Instance.CurrentLabelColor = colorNew;
                ColorSwatch.InvalidateVisual();
            }
        }

        private void ColorSwatch_OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            e.Surface.Canvas.Clear(PageData.Instance.CurrentLabelColor);
        }
    }
}
