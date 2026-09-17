using Avalonia.Media;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using VGraph.Config;
using VGraph.DataLayers;

namespace VGraph.ViewModels
{
    public partial class ConfigOptionsViewModel : ViewModelBase
    {
        [ObservableProperty] public partial Color BackgroundPaperColorInUi { get; set; }
        [ObservableProperty] public partial Color BorderLinesColorInUi { get; set; }
        [ObservableProperty] public partial Color CenterLinesColorInUi { get; set; }
        [ObservableProperty] public partial Color CursorColorColorInUi { get; set; }
        [ObservableProperty] public partial Color DefaultLineColorInUi { get; set; }
        [ObservableProperty] public partial Color GridLineColorInUi { get; set; }
        [ObservableProperty] public partial Color SelectionHighlightColorInUi { get; set; }
        [ObservableProperty] public partial Color SelectionBoxColorInUi { get; set; }
        [ObservableProperty] public partial string DefaultGridWidthInUi { get; set; }
        [ObservableProperty] public partial string DefaultGridHeightInUi { get; set; }
        [ObservableProperty] public partial string DefaultSquareSizeInUi { get; set; }
        [ObservableProperty] public partial string DefaultXMarginInUi { get; set; }
        [ObservableProperty] public partial string DefaultYMarginInUi { get; set; }
        [ObservableProperty] public partial string FpsLimitInUi { get; set; }

        public ConfigOptionsViewModel()
        {
            BackgroundPaperColorInUi = SKColorToColor(ConfigOptions.Instance.BackgroundPaperColor);
            BorderLinesColorInUi = SKColorToColor(ConfigOptions.Instance.BorderLinesColor);
            CenterLinesColorInUi = SKColorToColor(ConfigOptions.Instance.CenterLinesColor);
            CursorColorColorInUi = SKColorToColor(ConfigOptions.Instance.CursorColor);
            DefaultLineColorInUi = SKColorToColor(ConfigOptions.Instance.DefaultLineColor);
            GridLineColorInUi = SKColorToColor(ConfigOptions.Instance.GridLinesColor);
            SelectionHighlightColorInUi = SKColorToColor(ConfigOptions.Instance.LineHighlightColor);
            SelectionBoxColorInUi = SKColorToColor(ConfigOptions.Instance.SelectionBoxColor);

            DefaultGridWidthInUi = ConfigOptions.Instance.SquaresWide.ToString();
            DefaultGridHeightInUi = ConfigOptions.Instance.SquaresTall.ToString();
            DefaultSquareSizeInUi = ConfigOptions.Instance.SquareSize.ToString();
            DefaultXMarginInUi = ConfigOptions.Instance.MarginX.ToString();
            DefaultYMarginInUi = ConfigOptions.Instance.MarginY.ToString();
            FpsLimitInUi = ConfigOptions.Instance.MaxFrameRate.ToString();
        }

        private Color SKColorToColor(SKColor c)
        {
            return new Color(c.Alpha, c.Red, c.Green, c.Blue);
        }

        public bool FinishPreferencesWindow()
        {
            int targetGridWidth = -1;
            int targetGridHeight = -1;
            int targetSquareSize = -1;
            int targetXMargin = -1;
            int targetYMargin = -1;
            int targetFpsLimit = -1;

            bool result = int.TryParse(DefaultGridWidthInUi, out targetGridWidth) &&
                int.TryParse(DefaultGridHeightInUi, out targetGridHeight) &&
                int.TryParse(DefaultSquareSizeInUi, out targetSquareSize) &&
                int.TryParse(DefaultXMarginInUi, out targetXMargin) &&
                int.TryParse(DefaultYMarginInUi, out targetYMargin) &&
                int.TryParse(FpsLimitInUi, out targetFpsLimit);
            if (!result)
            {
                return false;
            }
            ConfigOptions.Instance.BackgroundPaperColor = BackgroundPaperColorInUi.ToSKColor();
            ConfigOptions.Instance.BorderLinesColor = BorderLinesColorInUi.ToSKColor();
            ConfigOptions.Instance.CenterLinesColor = CenterLinesColorInUi.ToSKColor();
            ConfigOptions.Instance.CursorColor = CursorColorColorInUi.ToSKColor();
            ConfigOptions.Instance.DefaultLineColor = DefaultLineColorInUi.ToSKColor();
            ConfigOptions.Instance.GridLinesColor = GridLineColorInUi.ToSKColor();
            ConfigOptions.Instance.LineHighlightColor = SelectionHighlightColorInUi.ToSKColor();
            ConfigOptions.Instance.SelectionBoxColor = SelectionBoxColorInUi.ToSKColor();

            ConfigOptions.Instance.SquaresWide = targetGridWidth;
            ConfigOptions.Instance.SquaresTall = targetGridHeight;
            ConfigOptions.Instance.SquareSize = targetSquareSize;
            ConfigOptions.Instance.MarginX = targetXMargin;
            ConfigOptions.Instance.MarginY = targetYMargin;
            ConfigOptions.Instance.MaxFrameRate = targetFpsLimit;

            ConfigOptions.SaveConfigFile();
            GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
            gridBackgroundLayer.ForceRedraw();

            return true;
        }
    }
}
