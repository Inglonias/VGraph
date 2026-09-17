using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using VGraph.ViewModels;

namespace VGraph.Views;

public partial class ConfigOptionsWindow : Window
{
    public required MainWindow ParentWindow { get; set; }

    public ConfigOptionsWindow()
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
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }

    private void Okay_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigOptionsViewModel vm)
        {
            if (!vm.FinishPreferencesWindow())
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Invalid value",
                    "Please enter valid values for all preferences",
                    ButtonEnum.Ok);
                return;
            }
        }
        this.Close();
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
        ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }

}