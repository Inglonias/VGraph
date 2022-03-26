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
using VGraph.src.dataLayers;

namespace VGraph.src.ui
{
    /// <summary>
    /// Interaction logic for ConfigOptionsWindow.xaml
    /// </summary>
    public partial class ConfigOptionsWindow : Window
    {
        public MainWindow MainWindowParent { get; set; }
        private readonly List<ConfigRow> ConfigOptionsList = new List<ConfigRow>();

        public ConfigOptionsWindow()
        {
            InitializeComponent();
            ConfigOptionsList.Add(new ConfigRow("Background paper color:", ConfigRow.TYPE_COLOR, "BackgroundPaperColorString"));
            ConfigOptionsList.Add(new ConfigRow("Border lines color:"    , ConfigRow.TYPE_COLOR, "BorderLinesColorString"));
            ConfigOptionsList.Add(new ConfigRow("Center lines color:"    , ConfigRow.TYPE_COLOR, "CenterLinesColorString"));
            ConfigOptionsList.Add(new ConfigRow("Cursor color:"          , ConfigRow.TYPE_COLOR, "CursorColorString"));
            ConfigOptionsList.Add(new ConfigRow("Default line color:"    , ConfigRow.TYPE_COLOR, "DefaultLineColorString"));
            ConfigOptionsList.Add(new ConfigRow("Grid lines color:"      , ConfigRow.TYPE_COLOR, "GridLinesColorString"));
            ConfigOptionsList.Add(new ConfigRow("Selection box color:"   , ConfigRow.TYPE_COLOR, "SelectionBoxColorString"));

            ConfigOptionsList.Add(new ConfigRow("Default grid width:"   , ConfigRow.TYPE_NUMBER, "SquaresWide"));
            ConfigOptionsList.Add(new ConfigRow("Default grid height:"   , ConfigRow.TYPE_NUMBER, "SquaresTall"));
            ConfigOptionsList.Add(new ConfigRow("Default square size:"   , ConfigRow.TYPE_NUMBER, "SquareSize"));
            ConfigOptionsList.Add(new ConfigRow("Default X margin:"   , ConfigRow.TYPE_NUMBER, "MarginX"));
            ConfigOptionsList.Add(new ConfigRow("Default Y margin:"   , ConfigRow.TYPE_NUMBER, "MarginY"));

            for (int i = 0; i < ConfigOptionsList.Count; i++)
            {
                AddElementToGrid(i, 0, ConfigOptionsList[i].GridLabel);
                AddElementToGrid(i, 1, ConfigOptionsList[i].ValueTextBox);
                if (ConfigOptionsList[i].ConfigType == ConfigRow.TYPE_COLOR)
                {
                    AddElementToGrid(i, 2, ConfigOptionsList[i].GetColorSelectButton());
                }
            }
        }

        private void AddElementToGrid(int row, int col, UIElement target)
        {
            while (ConfigGrid.RowDefinitions.Count <= row)
            {
                ConfigGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(32, GridUnitType.Pixel)
                });
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
            public string TargetPropertyName { get; } //I'm using reflection to access properties by name to avoid having to write specialized code.
            public string TargetPropertyValue { get; private set; }
            public ConfigRow(string label, int configType, string targetObject)
            {
                GridLabel = new Label
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Content = label
                };
                ConfigType = configType;
                TargetPropertyName = targetObject;
                var property = ConfigOptions.Instance.GetType().GetProperty(TargetPropertyName);
                TargetPropertyValue = property.GetValue(ConfigOptions.Instance).ToString();
                ValueTextBox = new TextBox
                {
                    FontFamily = new FontFamily("Courier New"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = TargetPropertyValue
                };
            }

            public Button GetColorSelectButton()
            {
                Button rVal = new Button
                {
                    Content = "Change...",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                rVal.Click += SelectAColor;
                return rVal;
            }

            private void SelectAColor(object sender, RoutedEventArgs e)
            {
                SKColor currentColor = SKColor.Parse(ValueTextBox.Text.ToString());
                System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog
                {
                    AllowFullOpen = true,
                    Color = System.Drawing.Color.FromArgb(currentColor.Alpha, currentColor.Red, currentColor.Green, currentColor.Blue)
                };
                if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //The alpha values can't be changed with this color picker, so I'm leaving it the same to avoid confusion.
                    //Alpha values can be edited in the text strings themselves, though.
                    ValueTextBox.Text = new SKColor(cd.Color.R, cd.Color.G, cd.Color.B, currentColor.Alpha).ToString();
                }
            }
        }

        private void ConfigOptionsWindow_OnOK(object sender, RoutedEventArgs e)
        {
            foreach (ConfigRow cr in ConfigOptionsList)
            {
                var propertyTarget = ConfigOptions.Instance.GetType().GetProperty(cr.TargetPropertyName);
                if (cr.ConfigType == ConfigRow.TYPE_COLOR)
                {
                    propertyTarget.SetValue(ConfigOptions.Instance, cr.ValueTextBox.Text);
                }
                else if (cr.ConfigType == ConfigRow.TYPE_NUMBER)
                {
                    propertyTarget.SetValue(ConfigOptions.Instance, Convert.ToInt32(cr.ValueTextBox.Text));
                }
            }
            //Write config to file and reload immediately.
            ConfigOptions.SaveConfigFile();
            ConfigOptions.LoadConfigFile();

            foreach (var l in PageData.Instance.GetDataLayers())
            {
                l.Value.ForceRedraw(); //To apply property changes immediately.
            }
            MainWindowParent.MainCanvas.InvalidateVisual();
            this.Close();
        }

        private void ConfigOptionsWindow_OnCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
