using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using System;
using System.Runtime.CompilerServices;
using VGraph.ViewModels;

namespace VGraph.Views;

public partial class LabelPropertiesWindow : Window
{
    public required MainWindow ParentWindow { get; init; }
    public LabelPropertiesWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ParentWindow.IsHitTestVisible = false;
        ParentWindow.IsEnabled = false;
        ParentWindow.Closed += (_, _) =>
        {
            this.Close();
        };
    }

    private void Okay_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LabelPropertiesViewModel vm)
        {
            vm.FinishLabelSetup();
        }
        ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        this.Close();
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }
    
    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }
}