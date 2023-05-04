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
        public SKPointI TargetGridPoint { get; set; }
        public TextLabel? AssociatedLabel { get; private set; } = null;
        public LabelPropertiesWindow()
        {
            InitializeComponent();
            ColorSwatch.InvalidateVisual();
        }


        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Okay_OnClick(object sender, RoutedEventArgs e)
        {
            TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            if (AssociatedLabel == null)
            {
                bool oddMode = ((PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER)).OddMode;
                lText.AddTextLabel(TargetGridPoint, TextBoxLabelText.Text, PageData.Instance.CurrentLabelColor.ToString(), ((System.Windows.Media.FontFamily)ComboBoxFonts.SelectedItem).Source, Convert.ToInt32(TextBoxFontSize.Text), ComboBoxAlignment.SelectedIndex, oddMode);
            }
            else
            {
                PageHistory.Instance.CreateUndoPoint(null, lText.LabelList, true);
                AssociatedLabel.LabelColor = PageData.Instance.CurrentLabelColor.ToString();
                AssociatedLabel.LabelText = TextBoxLabelText.Text;
                AssociatedLabel.FontFamily = ((System.Windows.Media.FontFamily)ComboBoxFonts.SelectedItem).Source;
                AssociatedLabel.FontSize = Convert.ToInt32(TextBoxFontSize.Text);
                AssociatedLabel.Alignment = ComboBoxAlignment.SelectedIndex;
                lText.ForceRedraw();
            }
            PageData.Instance.MakeCanvasDirty();
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

        public void AssociateWithLabel(TextLabel tl)
        {
            AssociatedLabel = tl;
            TargetGridPoint = tl.RenderPoint;
            TextBoxLabelText.Text = tl.LabelText;
            SKColor labelColor = PageData.Instance.CurrentLabelColor;
            SKColor.TryParse(tl.LabelColor, out labelColor);
            PageData.Instance.CurrentLabelColor = labelColor;
            ColorSwatch.InvalidateVisual();
            ComboBoxFonts.SelectedItem = new System.Windows.Media.FontFamily(tl.FontFamily);
            ComboBoxAlignment.SelectedIndex = tl.Alignment;
            TextBoxFontSize.Text = tl.FontSize.ToString();
        }
    }
}
