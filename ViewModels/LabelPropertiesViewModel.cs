using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using VGraph.Config;
using VGraph.DataLayers;
using VGraph.Objects;

namespace VGraph.ViewModels;

public partial class LabelPropertiesViewModel : ViewModelBase
{
    [ObservableProperty] public partial IFontCollection InstalledFonts { get; set; }
    [ObservableProperty] public partial TextLabel TargetLabel { get; set; }
    [ObservableProperty] public partial string LabelTextInUi { get; set; }
    [ObservableProperty] public partial int FontSizeInUi { get; set; }
    [ObservableProperty] public partial FontFamily FontInUi { get; set; }
    [ObservableProperty] public partial AlignmentOption SelectedAlignmentInUi { get; set; }
    public readonly record struct AlignmentOption(string DisplayText, int Value);

    public AlignmentOption[] AlignmentOptions { get; } =
    {
        new AlignmentOption("Top Left", 0),
        new AlignmentOption("Top Center", 1),
        new AlignmentOption("Top Right", 2),
        new AlignmentOption("Center Left", 3),
        new AlignmentOption("Center Center", 4),
        new AlignmentOption("Center Right", 5),
        new AlignmentOption("Bottom Left", 6),
        new AlignmentOption("Bottom Center", 7),
        new AlignmentOption("Bottom Right", 8),
    };

    public Color ChosenColorInUi
    {
        get { return _chosenColorInUi; }
        set { _chosenColorInUi = SetChosenColor(value); }

    }
    private Color _chosenColorInUi;

    public LabelPropertiesViewModel(TextLabel targetLabel)
    {
        TargetLabel = targetLabel;
        LabelTextInUi = TargetLabel.LabelText;
        FontSizeInUi = TargetLabel.FontSize;
        InstalledFonts = FontManager.Current.SystemFonts;
        FontInUi = InstalledFonts[0];
        foreach (FontFamily f in InstalledFonts)
        {
            if (f.Name.Equals(TargetLabel.FontFamily))
            {
                FontInUi = f;
                break;
            }
        }
        int alignmentIndex = TargetLabel.Alignment;
        SelectedAlignmentInUi = AlignmentOptions[alignmentIndex];
        _chosenColorInUi = Color.FromArgb(PageData.Instance.CurrentLabelColor.Alpha,
            PageData.Instance.CurrentLabelColor.Red,
            PageData.Instance.CurrentLabelColor.Green,
            PageData.Instance.CurrentLabelColor.Blue);
    }

    public Color SetChosenColor(Color newColor)
    {
        _chosenColorInUi = newColor;
        PageData.Instance.CurrentLabelColor = newColor.ToSKColor();
        return _chosenColorInUi;
    }

    public void FinishLabelSetup()
    {
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        TargetLabel.LabelText = LabelTextInUi;
        TargetLabel.FontSize = FontSizeInUi;
        TargetLabel.FontFamily = FontInUi.Name;
        TargetLabel.Alignment = SelectedAlignmentInUi.Value;
        TargetLabel.LabelColor = ChosenColorInUi.ToSKColor().ToString();
        textLayer.ForceRedraw();
    }
}