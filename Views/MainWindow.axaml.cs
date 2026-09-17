using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using VGraph.DataLayers;
using VGraph.Objects;
using VGraph.ViewModels;

namespace VGraph.Views;

public partial class MainWindow : Window
{
    private bool _allowClose = false;


    public MainWindow()
    {
        InitializeComponent();
        MainMenuBar.DataContext = new MenuBarModel();
        MainMenuBar.ParentWindow = this;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is MainViewModel vm)
        {
            vm.EditTextLabelEvent += (_, targetLabel) => EditLabel(targetLabel);
        }
    }

    private void EditLabel(TextLabel target)
    {
        LabelPropertiesWindow lpw = new LabelPropertiesWindow
        {
            ParentWindow = this,
            DataContext = new LabelPropertiesViewModel(target)
        };
        lpw.Show();
    }
    
    //Caution: Method written by CoPilot
    private async void MainWindow_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }
        bool canvasIsDirty = false;

        if (DataContext is MainViewModel vm)
        {
            canvasIsDirty = vm.IsCanvasDirty();
        }
        e.Cancel = true;

        if (canvasIsDirty)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Warning - Unsaved changes",
                "You have unsaved changes. Are you sure you want to continue?",
                ButtonEnum.YesNo);

            if (await box.ShowAsync() != ButtonResult.Yes)
            {
                return;
            }
        }
        _allowClose = true;
        Close();
    }
}