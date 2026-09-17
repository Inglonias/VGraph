using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using VGraph.Config;
using VGraph.DataLayers;
using VGraph.Objects;
using VGraph.ViewModels;

namespace VGraph.Views;

public class MainCanvas : Control
{
    private MainCanvasModel ViewModel => (MainCanvasModel)DataContext!;

    private readonly LayerDrawOperation _drawOp = new LayerDrawOperation();
    private bool _redrawPending = true;

    public MainCanvas()
    {
        double frameTime = 1000.0 / ConfigOptions.Instance.MaxFrameRate;
        DispatcherTimer timer = new()
        {
            Interval = TimeSpan.FromMilliseconds(frameTime) //Configured framerate
        };

        timer.Tick += (_, _) =>
        {
            if (_redrawPending)
            {
                _redrawPending = false;
                foreach (var layer in _drawOp.Layers)
                {
                    if (layer.IsRedrawRequired())
                    {
                        InvalidateVisual();
                        InvalidateMeasure();
                        break;
                    }
                }
            }
        };

        timer.Start();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ViewModel.CanvasVersion))
                {
                    InvalidateVisual();
                    InvalidateMeasure();
                }
            };
        }
    }
    
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.Custom(_drawOp);
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var position = e.GetPosition(this);
        ViewModel.HandlePointerMoved(e.Properties.IsLeftButtonPressed, e.KeyModifiers.HasFlag(KeyModifiers.Control), position);
        
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        ViewModel.HandlePointerPressed(e.Properties.IsLeftButtonPressed, e.Properties.IsRightButtonPressed, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.GetPosition(this));

    }
    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(_drawOp.Bounds.Width, _drawOp.Bounds.Height);
    }
}
