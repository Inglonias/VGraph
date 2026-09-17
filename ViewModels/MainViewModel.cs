using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VGraph.Config;
using VGraph.DataLayers;
using VGraph.Objects;

namespace VGraph.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial string WindowTitle { get; set; } = "VGraph";
    [ObservableProperty] public partial string StatusText { get; set; } = "";

    public ICommand MoveThingsUpCommand { get; }
    public ICommand MoveThingsLeftCommand { get; }
    public ICommand MoveThingsDownCommand { get; }
    public ICommand MoveThingsRightCommand { get; }
    public ICommand DeleteThingsCommand { get; }
    public MainCanvasModel CanvasModel { get; }
    public event EventHandler<TextLabel>? EditTextLabelEvent;

    public MainViewModel()
    {
        CanvasModel = new MainCanvasModel();
        
        MoveThingsUpCommand = new RelayCommand(MoveThingsUp);
        MoveThingsLeftCommand = new RelayCommand(MoveThingsLeft);
        MoveThingsDownCommand = new RelayCommand(MoveThingsDown);
        MoveThingsRightCommand = new RelayCommand(MoveThingsRight);
        DeleteThingsCommand = new RelayCommand(DeleteThings);
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        PreviewLayer previewLayer = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER);

        textLayer.EditTextLabelEvent += (_, targetLabel) => 
            {
                CanvasModel.IncrementCanvasVersion();
                EditTextLabelEvent?.Invoke(this, targetLabel);
            };
    }

    private void MoveThings(int x, int y)
    {
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        lineLayer.MoveSelectedLines(x, y);
        textLayer.MoveSelectedLabels(x, y);
        CanvasModel.IncrementCanvasVersion();
    }
    
    private void MoveThingsUp()
    {
        MoveThings(0, -1);
    }

    private void MoveThingsLeft()
    {
        MoveThings(-1, 0);
    }
    private void MoveThingsDown()
    {
        MoveThings(0, 1);
    }
    private void MoveThingsRight()
    {
        MoveThings(1, 0);
    }

    private void DeleteThings()
    {
        LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        TextLayer textLayer = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        lineLayer.DeleteSelectedLines();
        textLayer.DeleteSelectedLabels();
        CanvasModel.IncrementCanvasVersion();
    }

    public void SetWindowTitle()
    {
        WindowTitle = PageData.Instance.GetWindowTitle();
    }

    public bool IsCanvasDirty()
    {
        return PageData.Instance.IsCanvasDirty;
    }
}