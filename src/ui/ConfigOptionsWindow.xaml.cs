using SkiaSharp;
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

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for ConfigOptionsWindow.xaml
    /// </summary>
    public partial class ConfigOptionsWindow : Window
    {
        public ConfigOptionsWindow()
        {
            InitializeComponent();
            List<ConfigRow> configOptions = new List<ConfigRow>();
            configOptions.Add(new ConfigRow("Background paper color:", ConfigRow.TYPE_COLOR, ConfigOptions.Instance.BackgroundPaperColor));

            for (int i = 0; i < configOptions.Count; i++)
            {
                AddElementToGrid(i, 0, configOptions[i].GridLabel);
                AddElementToGrid(i, 1, configOptions[i].ValueTextBox);
                if (configOptions[i].ConfigType == ConfigRow.TYPE_COLOR)
                {
                    AddElementToGrid(i, 2, configOptions[i].GetColorSelectButton());
                }
            }
        }

        private void AddElementToGrid(int row, int col, UIElement target)
        {
            while (ConfigGrid.RowDefinitions.Count <= row)
            {
                ConfigGrid.RowDefinitions.Add(new RowDefinition());
            }
            while (ConfigGrid.ColumnDefinitions.Count <= col)
            {
                ConfigGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            Grid.SetRow(target, row);
            Grid.SetColumn(target, col);
            ConfigGrid.Children.Add(target);
        }

        private class ConfigRow
        {
            public static readonly int TYPE_NUMBER = 0;
            public static readonly int TYPE_COLOR = 1;

            public Label GridLabel { get; }
            public TextBox ValueTextBox { get; }
            public int ConfigType { get; }
            public object TargetObject { get; private set; }
            public ConfigRow(string label, int configType, object targetObject)
            {
                GridLabel = new Label
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = label
                };
                ConfigType = configType;
                TargetObject = targetObject;
                ValueTextBox = new TextBox
                {
                    FontFamily = new FontFamily("Courier New"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = TargetObject.ToString()
                };
            }

            public Button GetColorSelectButton()
            {
                Button rVal = new Button
                {
                    Content = "Select Color",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                rVal.Click += SelectAColor;
                return rVal;
            }

            private void SelectAColor(object sender, RoutedEventArgs e)
            {
                SKColor currentColor = (SKColor)TargetObject;
                System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog
                {
                    AllowFullOpen = true,
                    Color = System.Drawing.Color.FromArgb(currentColor.Alpha, currentColor.Red, currentColor.Green, currentColor.Blue)
                };
                if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TargetObject = new SKColor(cd.Color.R, cd.Color.G, cd.Color.B, cd.Color.A);
                    ValueTextBox.Text = TargetObject.ToString();
                }
            }
        }
    }
}
