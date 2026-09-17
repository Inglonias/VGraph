using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using VGraph.ViewModels;

namespace VGraph.Views;

public partial class MirrorToolWindow : Window
{
    public required MainWindow ParentWindow { get; set; }

    public MirrorToolWindow()
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

    private async void MirrorToolWindow_OnOk(object sender, RoutedEventArgs e)
    {
        if (DataContext is MirrorToolWindowModel vm)
        {
            bool result = await vm.FinishMirrorTool();
            if (result)
            {
                ParentWindow.IsHitTestVisible = true;
                ParentWindow.IsEnabled = true;
                Close();
            }
        }
    }
    
    private void MirrorToolWindow_OnCancel(object sender, RoutedEventArgs e)
    {
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }
}