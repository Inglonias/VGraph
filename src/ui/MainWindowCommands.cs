using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
    }
    public class MenuCommands
    {
        //File Menu
        public static RoutedCommand NewGridCmd = new RoutedCommand("NewGridCmd", typeof(MenuCommands));
        public static RoutedCommand OpenGridCmd = new RoutedCommand("OpenGridCmd", typeof(MenuCommands));
        public static RoutedCommand SaveGridCmd = new RoutedCommand("SaveGridCmd", typeof(MenuCommands));
        public static RoutedCommand ExportGridCmd = new RoutedCommand("ExportGridCmd", typeof(MenuCommands));
        public static RoutedCommand ExitCmd = new RoutedCommand("ExitCmd", typeof(MenuCommands));

        //Edit Menu
        public static RoutedCommand UndoCmd = new RoutedCommand("UndoCmd", typeof(MenuCommands));
        public static RoutedCommand RedoCmd = new RoutedCommand("RedoCmd", typeof(MenuCommands));

        //View Menu

        //Tools Menu
    }
}
