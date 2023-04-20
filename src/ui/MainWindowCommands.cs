using System;
using System.Windows;
using System.Windows.Input;
using VGraph.src.config;
using VGraph.src.dataLayers;
using VGraph.src.objects;

namespace VGraph.src.ui
{
    public partial class MainWindow : Window
    {
        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.CreateNewGrid(true);
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.OpenGrid();
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.SaveGrid();
        }

        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.SaveGridAs();
        }

        private void ExportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ExportGrid();
        }

        private void ImportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ImportGrid();
        }

        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool cancelClose = MainMenuBar.ExitApp();
            if (!cancelClose)
            {
                Environment.Exit(0);
            }
        }

        private void UndoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.Undo();
        }

        private void UndoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PageHistory.Instance.CanUndo();
        }

        private void RedoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.Redo();
        }

        private void RedoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PageHistory.Instance.CanRedo();
        }

        private void ResizeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.CreateNewGrid(false);
        }

        private void ZoomInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PageData.Instance.ZoomIn();
            MainCanvas.InvalidateVisual();
        }
        
        private void ZoomOutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PageData.Instance.ZoomOut();
            MainCanvas.InvalidateVisual();
        }

        private void CenterLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ToggleCenterLines();
        }

        private void GridLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ToggleGridLines();
        }

        private void BackgroundImageCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ToggleBackgroundImage();
        }

        private void MergeLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.MergeLines();
        }

        private void MirrorLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.MirrorLines();
        }

        private void ConfigWindowCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ShowConfigWindow();
        }

        private void MoveUpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveItemsCommandLogic("UP");
        }

        private void MoveDownCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveItemsCommandLogic("DOWN");
        }

        private void MoveLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveItemsCommandLogic("LEFT");
        }

        private void MoveRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveItemsCommandLogic("RIGHT");
        }

        private void DeleteSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lineLayer.DeleteSelectedLines();
            MainCanvas.InvalidateVisual();
        }

        private void SelectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lineLayer.SelectAllLines();
            MainCanvas.InvalidateVisual();
        }

        private void MoveItemsCommandLogic(string direction)
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            if (direction == "UP")
            {
                lineLayer.MoveSelectedLines(0, -1);
            }
            else if (direction == "DOWN")
            {
                lineLayer.MoveSelectedLines(0, 1);
            }
            else if (direction == "LEFT")
            {
                lineLayer.MoveSelectedLines(-1, 0);
            }
            else if (direction == "RIGHT")
            {
                lineLayer.MoveSelectedLines(1, 0);
            }
            MainCanvas.InvalidateVisual();
        }
    }
    public class MenuCommands
    {
        //File Menu
        public static RoutedCommand NewGridCmd         = new RoutedCommand("NewGridCmd", typeof(MenuCommands));
        public static RoutedCommand OpenGridCmd        = new RoutedCommand("OpenGridCmd", typeof(MenuCommands));
        public static RoutedCommand SaveGridCmd        = new RoutedCommand("SaveGridCmd", typeof(MenuCommands));
        public static RoutedCommand SaveGridAsCmd      = new RoutedCommand("SaveGridAsCmd", typeof(MenuCommands));
        public static RoutedCommand ExportGridCmd      = new RoutedCommand("ExportGridCmd", typeof(MenuCommands));
        public static RoutedCommand ImportGridCmd      = new RoutedCommand("ImportGridCmd", typeof(MenuCommands));
        public static RoutedCommand ExitCmd            = new RoutedCommand("ExitCmd", typeof(MenuCommands));

        //Edit Menu
        public static RoutedCommand UndoCmd            = new RoutedCommand("UndoCmd", typeof(MenuCommands));
        public static RoutedCommand RedoCmd            = new RoutedCommand("RedoCmd", typeof(MenuCommands));
        public static RoutedCommand ResizeGridCmd      = new RoutedCommand("ResizeGridCmd", typeof(MenuCommands));
        public static RoutedCommand SelectAllCmd       = new RoutedCommand("SelectAllCmd", typeof(MenuCommands));

        //View Menu
        public static RoutedCommand ZoomInCmd          = new RoutedCommand("ZoomInCmd", typeof(MenuCommands));
        public static RoutedCommand ZoomOutCmd         = new RoutedCommand("ZoomOutCmd", typeof(MenuCommands));
        public static RoutedCommand CenterLinesCmd     = new RoutedCommand("CenterLinesCmd", typeof(MenuCommands));
        public static RoutedCommand GridLinesCmd       = new RoutedCommand("GridLinesCmd", typeof(MenuCommands));
        public static RoutedCommand BackgroundImageCmd = new RoutedCommand("BackgroundImageCmd", typeof(MenuCommands));

        //Tools Menu
        public static RoutedCommand MergeLinesCmd      = new RoutedCommand("MergeLinesCmd", typeof(MenuCommands));
        public static RoutedCommand MirrorLinesCmd     = new RoutedCommand("MirrorLinesCmd", typeof(MenuCommands));
        public static RoutedCommand ConfigWindowCmd    = new RoutedCommand("ConfigWindowCmd", typeof(MenuCommands));
    }

    public class UniversalCommands
    {
        public static RoutedCommand MoveItemsUp    = new RoutedCommand("MoveItemsUp", typeof(UniversalCommands));
        public static RoutedCommand MoveItemsDown  = new RoutedCommand("MoveItemsDown", typeof(UniversalCommands));
        public static RoutedCommand MoveItemsLeft  = new RoutedCommand("MoveItemsLeft", typeof(UniversalCommands));
        public static RoutedCommand MoveItemsRight = new RoutedCommand("MoveItemsRight", typeof(UniversalCommands));

        public static RoutedCommand DeleteSelectedItems = new RoutedCommand("DeleteSelectedItems", typeof(UniversalCommands));
    }
}
