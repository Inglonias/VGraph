using SkiaSharp;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VGraph.src.config
{
    public class ConfigOptions
    {
        [JsonIgnore]
        public static readonly string CONFIG_FILE_NAME = "userconfig.json";
        [JsonIgnore]
        public SKColor BackgroundPaperColor { get; set; } = new SKColor(255, 255, 255, 255);
        public SKColor BorderLinesColor { get; set; } = new SKColor(64, 64, 64, 32);
        [JsonIgnore]
        public SKColor CenterLinesColor { get; set; } = new SKColor(32, 32, 32, 128);
        [JsonIgnore]
        public SKColor CursorColor { get; set; } = new SKColor(255, 0, 0, 255);
        [JsonIgnore]
        public SKColor DefaultLineColor { get; set; } = new SKColor(0, 128, 255, 255);
        [JsonIgnore]
        public SKColor GridLinesColor { get; set; } = new SKColor(64, 64, 64, 64);
        [JsonIgnore]
        public SKColor SelectionBoxColor { get; set; } = new SKColor(0, 0, 0, 255);

        public string BackgroundPaperColorString { get; set; } = "#ffffffff";
        public string BorderLinesColorString { get; set; } = "#20404040";
        public string CenterLinesColorString { get; set; } = "#80202020";
        public string CursorColorString { get; set; } = "#ffff0000";
        public string DefaultLineColorString { get; set; } = "#ff0080ff";
        public string GridLinesColorString { get; set; } = "#40404040";
        public string SelectionBoxColorString { get; set; } = "#ff000000";
        public int SquaresWide { get; set; } = 32;
        public int SquaresTall { get; set; } = 42;
        public int SquareSize { get; set; } = 24;
        public int MarginX { get; set; } = 24;
        public int MarginY { get; set; } = 24;
        public byte BackgroundImageAlpha { get; set; } = 191;

        public static ConfigOptions Instance { get; private set; } = new ConfigOptions();

        public ConfigOptions()
        {
        }

        public void InitializeConfigFile()
        {
            if (!File.Exists(CONFIG_FILE_NAME))
            {
                File.WriteAllText(CONFIG_FILE_NAME, PrettySerialize());
            }
        }

        public static void SaveConfigFile(ConfigOptions target)
        {
            File.WriteAllText(CONFIG_FILE_NAME, target.PrettySerialize());
        }

        public static void LoadConfigFile()
        {
            ConfigOptions target = JsonSerializer.Deserialize<ConfigOptions>(File.ReadAllText(CONFIG_FILE_NAME));
            target.BackgroundPaperColor = SKColor.Parse(target.BackgroundPaperColorString);
            target.BorderLinesColor = SKColor.Parse(target.BorderLinesColorString);
            target.CenterLinesColor = SKColor.Parse(target.CenterLinesColorString);
            target.CursorColor = SKColor.Parse(target.CursorColorString);
            target.DefaultLineColor = SKColor.Parse(target.DefaultLineColorString);
            target.GridLinesColor = SKColor.Parse(target.GridLinesColorString);
            target.SelectionBoxColor = SKColor.Parse(target.SelectionBoxColorString);
            Instance = target;
        }

        private string PrettySerialize()
        {
            StringWriter rValWriter = new StringWriter();

            rValWriter.WriteLine("{");
            rValWriter.WriteLine("    \"BackgroundPaperColorString\":\"" + BackgroundPaperColorString + "\",");
            rValWriter.WriteLine("    \"BorderLinesColorString\":\"" + BorderLinesColorString + "\",");
            rValWriter.WriteLine("    \"CenterLinesColorString\":\"" + CenterLinesColorString + "\",");
            rValWriter.WriteLine("    \"CursorColorString\":\"" + CursorColorString + "\",");
            rValWriter.WriteLine("    \"DefaultLineColorString\":\"" + DefaultLineColorString + "\",");
            rValWriter.WriteLine("    \"GridLinesColorString\":\"" + GridLinesColorString + "\",");
            rValWriter.WriteLine("    \"SelectionBoxColorString\":\"" + SelectionBoxColorString + "\",");
            rValWriter.WriteLine("    \"SquaresWide\": " + SquaresWide + ",");
            rValWriter.WriteLine("    \"SquaresTall\": " + SquaresTall + ",");
            rValWriter.WriteLine("    \"SquareSize\": " + SquareSize + ",");
            rValWriter.WriteLine("    \"MarginX\": " + MarginX + ",");
            rValWriter.WriteLine("    \"MarginY\": " + MarginY + ",");
            rValWriter.WriteLine("    \"BackgroundImageAlpha\": " + BackgroundImageAlpha);
            rValWriter.WriteLine("}");

            return rValWriter.ToString();
        }

    }
}
