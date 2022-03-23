using System.Windows;
using System.Windows.Input;
using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraph.src.ui
{
    public partial class MainWindow : Window
    {
        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.CreateNewGrid();
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.OpenGrid();
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.SaveGrid();
        }

        private void ExportCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ExportGrid();
        }

        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.ExitApp();
        }

        private void UndoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.Undo();
        }

        private void UndoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            e.CanExecute = ll.CanUndo();
        }

        private void RedoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.Redo();
        }

        private void RedoCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            LineLayer ll = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            e.CanExecute = ll.CanRedo();
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

        private void MergeLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.MergeLines();
        }

        private void MirrorLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainMenuBar.MirrorLines();
        }

        private void MoveLinesUpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveLinesCommandLogic("UP");
        }

        private void MoveLinesDownCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveLinesCommandLogic("DOWN");
        }

        private void MoveLinesLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveLinesCommandLogic("LEFT");
        }

        private void MoveLinesRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveLinesCommandLogic("RIGHT");
        }

        private void DeleteSelectedLinesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.DeleteSelectedLines();
            MainCanvas.InvalidateVisual();
        }

        private void MoveLinesCommandLogic(string direction)
        {
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            if (direction == "UP")
            {
                lLines.MoveSelectedLines(0, -1);
            }
            else if (direction == "DOWN")
            {
                lLines.MoveSelectedLines(0, 1);
            }
            else if (direction == "LEFT")
            {
                lLines.MoveSelectedLines(-1, 0);
            }
            else if (direction == "RIGHT")
            {
                lLines.MoveSelectedLines(1, 0);
            }
            MainCanvas.InvalidateVisual();
        }
    }
    public class MenuCommands
    {
        //File Menu
        public static RoutedCommand NewGridCmd    = new RoutedCommand("NewGridCmd", typeof(MenuCommands));
        public static RoutedCommand OpenGridCmd   = new RoutedCommand("OpenGridCmd", typeof(MenuCommands));
        public static RoutedCommand SaveGridCmd   = new RoutedCommand("SaveGridCmd", typeof(MenuCommands));
        public static RoutedCommand ExportGridCmd = new RoutedCommand("ExportGridCmd", typeof(MenuCommands));
        public static RoutedCommand ExitCmd       = new RoutedCommand("ExitCmd", typeof(MenuCommands));

        //Edit Menu
        public static RoutedCommand UndoCmd = new RoutedCommand("UndoCmd", typeof(MenuCommands));
        public static RoutedCommand RedoCmd = new RoutedCommand("RedoCmd", typeof(MenuCommands));

        //View Menu
        public static RoutedCommand ZoomInCmd = new RoutedCommand("ZoomInCmd", typeof(MenuCommands));
        public static RoutedCommand ZoomOutCmd = new RoutedCommand("ZoomOutCmd", typeof(MenuCommands));
        public static RoutedCommand CenterLinesCmd = new RoutedCommand("CenterLinesCmd", typeof(MenuCommands));

        //Tools Menu
        public static RoutedCommand MergeLinesCmd = new RoutedCommand("MergerLinesCmd", typeof(MenuCommands));
        public static RoutedCommand MirrorLinesCmd = new RoutedCommand("MirrorLinesCmd", typeof(MenuCommands));
    }

    public class UniversalCommands
    {
        public static RoutedCommand MoveLinesUp = new RoutedCommand("MoveLinesUp", typeof(UniversalCommands));
        public static RoutedCommand MoveLinesDown = new RoutedCommand("MoveLinesDown", typeof(UniversalCommands));
        public static RoutedCommand MoveLinesLeft = new RoutedCommand("MoveLinesLeft", typeof(UniversalCommands));
        public static RoutedCommand MoveLinesRight = new RoutedCommand("MoveLinesRight", typeof(UniversalCommands));

        public static RoutedCommand DeleteSelectedLines = new RoutedCommand("DeleteSelectedLines", typeof(UniversalCommands));
    }
}
