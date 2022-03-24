using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for MirrorToolWindow.xaml
    /// </summary>
    public partial class MirrorToolWindow : Window
    {
        public MirrorToolWindow()
        {
            InitializeComponent();
        }

        private void MirrorDirection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MirrorLinePosLabel != null)
            {
                MirrorLinePosLabel.Content = MirrorDirection.SelectedIndex <= 1 ? "X = " : "Y = ";
            }
        }

        private void MirrorToolWindow_OnOk(object sender, RoutedEventArgs e)
        {
            LineLayer lineLayer = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            try
            {
                const int SUCCESS = 0;
                const int LINES_ACROSS_CREASE = 1;
                
                int result = lineLayer.MirrorLines(MirrorDirection.SelectedIndex, Convert.ToInt32(MirrorLinePosition.Text), DestructiveMirror.IsChecked.Value);
                
                if (result == LINES_ACROSS_CREASE)
                {
                    MessageBox.Show("There are one or more lines crossing the selected mirror line. These lines have been selected. Please move or delete them.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (FormatException)
            {

            }
            Close();
        }

        private void MirrorToolWindow_OnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
