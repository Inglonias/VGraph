using Avalonia;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System;
using System.Windows.Input;
using VGraph.Config;
using VGraph.DataLayers;
using VGraph.Objects;

namespace VGraph.ViewModels;

public partial class MainCanvasModel : ViewModelBase
{
    [ObservableProperty] public partial int CanvasVersion { get; set; }
    [ObservableProperty] public partial string WindowTitle { get; set; } = "VGraph 2.0";
    [ObservableProperty] public partial string CursorStatusText { get; set; } = "";
    [ObservableProperty] public partial string ToolStatusText { get; set; } = "";
    public MainCanvasModel()
    {
        CanvasVersion = 0;
    }

    public void HandlePointerMoved(bool leftButtonPressed, bool controlPressed, Point position)
    {
        LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        CursorLayer lCursor = (CursorLayer)PageData.Instance.GetDataLayer(PageData.CURSOR_LAYER);
        TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        PreviewLayer lPreview = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER);
        if (!leftButtonPressed)
        {
            SKRect selectionBox = lCursor.StopClickDrag();
            if (!selectionBox.Equals(SKRect.Empty))
            {
                lLines.HandleBoxSelect(selectionBox, controlPressed);
                lText.HandleBoxSelect(selectionBox, controlPressed);
            }
        }
        else
        {
            lCursor.StartClickDrag();
        }
        lCursor.MoveCursor(position);
        CursorStatusText = "X: " + lCursor.GetCursorGridPoints().X + " , Y: " + lCursor.GetCursorGridPoints().Y;
        ToolStatusText = lPreview.GetStatusText();
        WindowTitle = PageData.Instance.GetWindowTitle();
        IncrementCanvasVersion();
    }


    public void HandlePointerPressed(bool leftButtonPressed, bool rightButtonPressed, bool controlPressed, Point position)
    {
        LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
        CursorLayer lCursor = (CursorLayer)PageData.Instance.GetDataLayer(PageData.CURSOR_LAYER);
        PreviewLayer lPreview = (PreviewLayer)PageData.Instance.GetDataLayer(PageData.PREVIEW_LAYER);
        TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
        if (rightButtonPressed)
        {
            SKPointI target = lCursor.RoundToNearestIntersection(position);
            SKPointI targetGrid = lCursor.GetCursorGridPoints();
            lText.HandleCreationClick(target, targetGrid);
            lPreview.HandleCreationClick(target, targetGrid);
        }
        else if (leftButtonPressed)
        {
            bool selectionMade = lLines.HandleSelectionClick(position, controlPressed) || lText.HandleSelectionClick(position, controlPressed);
            if (selectionMade && PageData.Instance.IsEyedropperActive)
            {
                
            }
        }
        IncrementCanvasVersion();
    }

    public void IncrementCanvasVersion()
    {
        CanvasVersion = (CanvasVersion + 1) % int.MaxValue;
    }
}