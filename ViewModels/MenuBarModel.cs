using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VGraph.Config;
using VGraph.DataLayers;
using VGraph.Objects;


namespace VGraph.ViewModels;

public partial class MenuBarModel : ViewModelBase
{
    [ObservableProperty] public partial bool UndoEnabled { get; set; } = false;
    [ObservableProperty] public partial bool RedoEnabled { get; set; } = false;

    public Color ChosenColor
    {
        get => new Color(PageData.Instance.CurrentLineColor.Alpha,
                         PageData.Instance.CurrentLineColor.Red,
                         PageData.Instance.CurrentLineColor.Green,
                         PageData.Instance.CurrentLineColor.Blue);

        set
        {
            PageData.Instance.CurrentLineColor = value.ToSKColor();
            OnPropertyChanged();
        }
    }

    public Func<Task<bool>>? RequestUnsavedChangesConfirmation { get; set; }

    public event EventHandler<bool>? ShowNewGridWindow;
    
    //Commands
    public ICommand CreateNewGridCommand { get; }
    public ICommand EditExistingGridCommand { get; }
    public ICommand MergeLinesCommand { get; }

    public MenuBarModel()
    {
        CreateNewGridCommand = new RelayCommand(CreateNewGrid);
        EditExistingGridCommand = new RelayCommand(EditExistingGrid);
        MergeLinesCommand = new RelayCommand(MergeLines);
        PageHistory.Instance.PageHistoryChanged += (sender, args) =>
        {
            CheckEditButtonValidity();
        };
        PageData.Instance.LineColorChanged += (sender, color) =>
        {
            OnPropertyChanged(nameof(ChosenColor));
        };
        ChosenColor = Color.FromArgb(ConfigOptions.Instance.DefaultLineColor.Alpha,
            ConfigOptions.Instance.DefaultLineColor.Red, 
            ConfigOptions.Instance.DefaultLineColor.Green,
            ConfigOptions.Instance.DefaultLineColor.Blue);
    }

    private void CreateNewGrid()
    {
        _ = CreateNewGridAsync();
    }
    
    private async Task CreateNewGridAsync()
    {
        if (await CheckUnsavedChangesAsync())
        {
            ShowNewGridWindow?.Invoke(this, true);
        }
    }

    private void EditExistingGrid()
    {
        ShowNewGridWindow?.Invoke(this, false);
    }

    private void MergeLines()
    {
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        lineLayer.MergeAllLines();
    }

    public void SelectTool(string tool)
    {
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        lineLayer.SelectTool(tool);
        textLayer.SelectTool(tool);
    }

    public void ToggleOddMode(bool value)
    {
        PreviewLayer previewLayer = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER); 
        previewLayer.OddMode = value;
    }

    public void ToggleEyedropper(bool value)
    {
        PageData.Instance.IsEyedropperActive = value;
    }

    //Returns true if there are no unsaved changes, and/or the user wants to continue. False otherwise.
    //CAUTION: Method written by CoPilot
    private async Task<bool> CheckUnsavedChangesAsync()
    {
        if (!PageData.Instance.IsCanvasDirty)
        {
            return true;
        }

        if (RequestUnsavedChangesConfirmation is null)
        {
            return true; // fallback if no UI is attached
        }

        return await RequestUnsavedChangesConfirmation.Invoke();
    }

    public void CheckEditButtonValidity()
    {
        UndoEnabled = PageHistory.Instance.CanUndo();
        RedoEnabled = PageHistory.Instance.CanRedo();
    }

    public void OpenVgpFile(string filePath)
    {
        PageData.Instance.FileOpen(filePath);
    }

    public bool SaveCurrentVgp()
    {
        if (string.IsNullOrEmpty(PageData.Instance.LastSavePath))
        {
            return false;
        }
        PageData.Instance.FileSave();
        return true;
    }

    public void SaveNewVgp(string filePath)
    {
        PageData.Instance.FileSave(filePath);
    }

    public void ImportVgp(string filePath)
    {
        PageData.Instance.FileImport(filePath);
        CheckEditButtonValidity();
    }

    public void ExportVgp(string filePath)
    {
        PageData.Instance.FileExport(filePath);
    }

    public void UndoLastAction()
    {
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        PageHistory.Instance.CreateRedoPoint(lineLayer.LineList, textLayer.LabelList);
        PageHistory.PageState ps = PageHistory.Instance.PopUndoAction();

        if (ps.Lines != null)
        {
            lineLayer.LineList = ps.Lines;
            lineLayer.ForceRedraw();
        }
        if (ps.Labels != null)
        {
            textLayer.LabelList = ps.Labels;
            textLayer.ForceRedraw();
        }
    }

    public void RedoLastAction()
    {
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        PageHistory.Instance.CreateUndoPoint(lineLayer.LineList, textLayer.LabelList, false);
        PageHistory.PageState ps = PageHistory.Instance.PopRedoAction();

        if (ps.Lines != null)
        {
            lineLayer.LineList = ps.Lines;
            lineLayer.ForceRedraw();
        }
        if (ps.Labels != null)
        {
            textLayer.LabelList = ps.Labels;
            textLayer.ForceRedraw();
        }
        CheckEditButtonValidity();
    }

    public void ZoomIn()
    {
        PageData.Instance.ZoomIn();
    }

    public void ZoomOut()
    {
        PageData.Instance.ZoomOut();
    }
    
    public bool ToggleCenterLines()
    {
        GridBackgroundLayer gridLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
        return gridLayer.ToggleCenterLines();
    }
    
    public bool ToggleGridLines()
    {
        GridBackgroundLayer gridLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
        return gridLayer.ToggleGridLines();
    }
    
    public bool ToggleBackgroundImage()
    {
        GridBackgroundLayer gridLayer = (GridBackgroundLayer)PageData.Instance.GetDataLayer(PageData.GRID_LAYER);
        return gridLayer.ToggleBackgroundImage();
    }
}