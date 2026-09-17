using CommunityToolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;
using VGraph.Config;
using VGraph.DataLayers;
using static VGraph.ViewModels.LabelPropertiesViewModel;

namespace VGraph.ViewModels;

public partial class MirrorToolWindowModel : ViewModelBase
{
    [ObservableProperty] public partial DirectionOption SelectedDirectionInUi { get; set; }
    [ObservableProperty] public partial string MirrorLinePositionInUi { get; set; } = "0";
    [ObservableProperty] public partial bool DeleteLinesInUi { get; set; } = false;
    [ObservableProperty] public partial bool OddModeInUi { get; set; } = false;
    public readonly record struct DirectionOption(string DisplayText, int Value, string PosText);

    public DirectionOption[] DirectionOptions { get; } =
    {
        new DirectionOption("Left to Right", 0, "X ="),
        new DirectionOption("Right to Left", 1, "X ="),
        new DirectionOption("Top to Bottom", 2, "Y ="),
        new DirectionOption("Bottom to Top", 3, "Y =")
    };

    public MirrorToolWindowModel()
    {
        SelectedDirectionInUi = DirectionOptions[0];
    }

    public async Task<bool> FinishMirrorTool()
    {
        int crease = -1;
        if (!int.TryParse(MirrorLinePositionInUi, out crease))
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                            "Invalid mirror axis",
                            "Please enter a valid mirror axis",
                            ButtonEnum.Ok);
            await box.ShowAsync();

            return false;
        }
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        if (lineLayer.MirrorLines(SelectedDirectionInUi.Value, crease, DeleteLinesInUi, OddModeInUi) == 1)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                            "Lines across axis",
                            "There are lines across the selected mirror axis. These lines have been selected. Please handle them and then try again.",
                            ButtonEnum.Ok);
            await box.ShowAsync();
        }
        return true;
    }
}