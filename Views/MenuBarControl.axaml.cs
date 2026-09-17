using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using VGraph.ViewModels;

namespace VGraph.Views
{
    public partial class MenuBarControl : UserControl
    {
        public required MainWindow ParentWindow { get; set; }
        public MenuBarControl()
        {
            InitializeComponent();
            this.DataContextChanged += (_, _) =>
            {
                if (DataContext is MenuBarModel vm)
                {
                    vm.RequestUnsavedChangesConfirmation = async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard(
                            "Warning - Unsaved changes",
                            "You have unsaved changes. Are you sure you want to continue?",
                            ButtonEnum.YesNo);

                        var result = await box.ShowAsync();
                        return result == ButtonResult.Yes;
                    };
                    vm.ShowNewGridWindow += (_, deleteLines) =>
                    {
                        OpenNewGridWindow(deleteLines);
                    };
                }
            };
        }
        
        private void ToolMenu_OnChecked(object sender, RoutedEventArgs e)
        {
            ToggleButton toolClicked = (ToggleButton)sender;
            if (toolClicked.Name != null)
            {
                string targetTool = toolClicked.Name;
                SelectTool(targetTool);
            }

            InvalidateVisual();
        }
        
        private void SelectTool(string tool)
        {
            List<ToggleButton> toolMenuItems =
            [
                LineTool,
                TriTool,
                BoxTool,
                CircleTool,
                BoxyCircleTool,
                EllipseTool,
                TextTool,
            ];

            foreach (ToggleButton m in toolMenuItems)
            {
                m.IsChecked = m.Name != null && m.Name.Equals(tool);
            }

            if (DataContext is MenuBarModel vm)
            {
                vm.SelectTool(tool);
            }
        }

        private void OddMode_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                vm.ToggleOddMode(OddModeCheckbox.IsChecked.GetValueOrDefault());
            }
        }

        private void EyedropperTool_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                vm.ToggleEyedropper(EyedropperTool.IsChecked.GetValueOrDefault());
            }
        }

        private async void OpenButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open VGP File",
                AllowMultiple = false,
                FileTypeFilter = [Vgp]
            });
            if (file.Count > 0)
            {
                if (DataContext is MenuBarModel vm)
                {
                    vm.OpenVgpFile(file[0].Path.ToString()[8..]);
                }
                ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
                ParentWindow.PrimaryDrawingPanel.InvalidateMeasure();
            }
        }

        private void SaveButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                if (!vm.SaveCurrentVgp())
                {
                    SaveAsButton_OnClick(sender, e);
                }

                ParentWindow.PrimaryDrawingPanel.InvalidateVisual(); //To set the window title.
            }
        }
        
        private async void SaveAsButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save VGP File",
                DefaultExtension = "vgp",
                FileTypeChoices = new[] { Vgp }
            });
            if (file is not null)
            {
                if (DataContext is MenuBarModel vm)
                {
                    vm.SaveNewVgp(file.Path.ToString()[8..]);
                }
            }
        }
        
        private async void ImportButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import VGP File",
                AllowMultiple = false,
                FileTypeFilter = [Vgp]
            });
            if (file.Count > 0)
            {
                if (DataContext is MenuBarModel vm)
                {
                    vm.ImportVgp(file[0].Path.ToString()[8..]);
                }
                ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
                ParentWindow.PrimaryDrawingPanel.InvalidateMeasure();
            }
        }
        
        private async void ExportButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Image",
                DefaultExtension = "png",
                FileTypeChoices = new[] { FilePickerFileTypes.ImagePng }
            });
            if (file is not null)
            {
                if (DataContext is MenuBarModel vm)
                {
                    vm.ExportVgp(file.Path.ToString()[8..]);
                }
            }
        }
        
        private void UndoButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                vm.UndoLastAction();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }

        private void RedoButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                vm.RedoLastAction();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }

        private void ZoomIn_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                vm.ZoomIn();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }
        
        private void ZoomOut_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                vm.ZoomOut();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }
        
        private void CenterLinesButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                CenterLinesButton.IsChecked = vm.ToggleCenterLines();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }

        private void GridLinesButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                GridLinesButton.IsChecked = vm.ToggleGridLines();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }

        private void BackgroundImageButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MenuBarModel vm)
            {
                BackgroundImageButton.IsChecked = vm.ToggleBackgroundImage();
            }
            ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
        }

        private void MirrorTool_OnClick(object? sender, RoutedEventArgs e)
        {
            MirrorToolWindow mtw = new MirrorToolWindow
            {
                ParentWindow = ParentWindow,
                DataContext = new MirrorToolWindowModel()
            };
            mtw.ParentWindow = ParentWindow;
            mtw.Show();
        }

        private void Preferences_OnClick(object? sender, RoutedEventArgs e)
        {
            ConfigOptionsWindow cow = new ConfigOptionsWindow //Moo!
            {
                ParentWindow = ParentWindow,
                DataContext = new ConfigOptionsViewModel()
            };
            cow.ParentWindow = ParentWindow;
            cow.Show();
        }

        private void OpenNewGridWindow(bool deleteLines)
        {
            NewGridWindow ngw = new NewGridWindow
            {
                ParentWindow = this.ParentWindow
            };
            var ngwm = new NewGridWindowModel
            {
                DeleteLines = deleteLines
            };
            ngw.DataContext = ngwm;
            ngw.NewGridWindowComplete += (_, _) =>
            {
                ParentWindow.PrimaryDrawingPanel.InvalidateVisual();
                ParentWindow.PrimaryDrawingPanel.InvalidateMeasure();
            };

            ngw.Show();
        }
        
        private static FilePickerFileType Vgp { get; } = new FilePickerFileType("VGP File")
        {
            Patterns = (IReadOnlyList<string>) new string[1]
            {
                "*.vgp"
            },
            AppleUniformTypeIdentifiers = (IReadOnlyList<string>) new string[1]
            {
                "public.data"
            },
            MimeTypes = (IReadOnlyList<string>) new string[1]
            {
                "application/x-vgp"
            }
        };
    }
}