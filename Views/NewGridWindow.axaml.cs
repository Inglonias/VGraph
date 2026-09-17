using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using VGraph.Config;
using VGraph.ViewModels;

namespace VGraph.Views;

public partial class NewGridWindow : Window
{

    public event EventHandler? NewGridWindowComplete;
    public required MainWindow ParentWindow { get; init; }
    public NewGridWindow()
    {
        InitializeComponent();
        InvalidateVisual();
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

    private void NewGridWindow_OnCancel(object sender, RoutedEventArgs e)
    {
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
        Close();
    }

    private void NewGridWindow_OnOk(object sender, RoutedEventArgs e)
    {
        if (DataContext is NewGridWindowModel vm)
        {
            vm.FinishGridSetup();
            ParentWindow.IsHitTestVisible = true;
            ParentWindow.IsEnabled = true;
        }
        
        NewGridWindowComplete?.Invoke(this, EventArgs.Empty);
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        ParentWindow.IsHitTestVisible = true;
        ParentWindow.IsEnabled = true;
    }

    private async void BackgroundImageBrowse_OnClick(object sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var file = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Background Image",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });
        if (file.Count > 0)
        {
            if (DataContext is NewGridWindowModel vm)
            {
                vm.ImagePath = file[0].Path.ToString().Substring(8); //Remove the preceding "file//"
            }
        }
    }
}