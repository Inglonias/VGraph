using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VGraph.src.dataLayers;
using VGraph.src.objects;

namespace VGraph.src.config
{
    //Singleton containing commonly used and modified properties and methods that require wide application access.
    public class PageData
    {
        public const string GRID_LAYER    = "Grid Layer";
        public const string LINE_LAYER    = "Line Layer";
        public const string PREVIEW_LAYER = "Preview Layer";
        public const string CURSOR_LAYER  = "Cursor Layer";
        //Default values produce an 8.5" x 11" piece of paper at 96 dpi.
        public int SquaresWide { get; set; } = 32;
        public int SquaresTall { get; set; } = 42;
        public int SquareSize { get; set; } = 24;
        public int MarginX { get; set; } = 24;
        public int MarginY { get; set; } = 24;
        public int TrueSquareSize { get; set; } = 24; //This size is used when saving or exporting.
        public byte BackgroundImageAlpha { get; set; } = 128;
        public string BackgroundImagePath { get; private set; } = "";
        public SKColor CurrentLineColor { get; set; } = LineSegment.DEFAULT_COLOR;

        public bool ExportCenterLines { get; set; } = false;
        public bool ExportGridLines { get; set; } = true;

        private readonly Dictionary<string, IDataLayer> DataLayers = new Dictionary<string, IDataLayer>();


        /// <summary>
        /// Calculate the total width of the canvas in pixels.
        /// </summary>
        /// <returns>Width of the canvas in pixels.</returns>
        public int GetTotalWidth()
        {
            return (SquaresWide * SquareSize) + (MarginX * 2);
        }

        /// <summary>
        /// Calculate the total height of the canvas in pixels.
        /// </summary>
        /// <returns>Height of the canvas in pixels.</returns>
        public int GetTotalHeight()
        {
            return (SquaresTall * SquareSize) + (MarginY * 2);
        }

        /// <summary>
        /// Get the dictionary containing all of the data layers used for drawing the canvas.
        /// </summary>
        /// <returns>Dictionary containing all data layers.</returns>
        public Dictionary<string, IDataLayer> GetDataLayers()
        {
            return DataLayers;
        }

        /// <summary>
        /// Get a single data layer from the dictionary of data layers used for drawing the canvas.
        /// </summary>
        /// <param name="key">The dictionary key. Use the layer constants for best results</param>
        /// <returns>A data layer from the dictionary, if the key is correct.</returns>
        public IDataLayer GetDataLayer(string key)
        {
            return DataLayers[key];
        }

        public bool SetBackgroundImage(string path)
        {
            GridBackgroundLayer gridLayer = (GridBackgroundLayer)DataLayers[GRID_LAYER];
            if (gridLayer.SetBackgroundImage(path))
            {
                BackgroundImagePath = path;
                return true;
            }
            else
            {
                BackgroundImagePath = "";
                return false;
            }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static PageData()
        {
        }

        private PageData()
        {
        }

        public static PageData Instance { get; } = new PageData();

        /// <summary>
        /// Attempt to load the VGP file from the path passed to the function.
        /// </summary>
        /// <param name="fileName">Path to the file that will be loaded</param>
        /// <returns>True if the file is valid and no errors occurred. False otherwise.</returns>
        public bool FileOpen(string fileName)
        {
            VgpFile saveFile;
            try
            {
                string jsonString = File.ReadAllText(fileName);
                saveFile = JsonSerializer.Deserialize<VgpFile>(jsonString);
                SquaresWide = saveFile.SquaresWide;
                SquaresTall = saveFile.SquaresTall;
                SquareSize = saveFile.SquareSize;
                TrueSquareSize = saveFile.SquareSize;
                MarginX = saveFile.MarginX;
                MarginY = saveFile.MarginY;

                LineLayer lineLayer = (LineLayer)DataLayers[LINE_LAYER];

                lineLayer.ClearAllLines();
                lineLayer.AddNewLines(saveFile.Lines.ToArray());

                foreach (KeyValuePair<string, IDataLayer> l in DataLayers)
                {
                    l.Value.ForceRedraw();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempt to import the VGP file from the path passed to the function.
        /// In this context, that means "add all lines from the target file to the canvas"
        /// </summary>
        /// <param name="fileName">Path to the file that will be loaded</param>
        /// <returns>True if the file is valid and no errors occurred. False otherwise.</returns>
        public bool FileImport(string fileName)
        {
            VgpFile saveFile;
            try
            {
                string jsonString = File.ReadAllText(fileName);
                saveFile = JsonSerializer.Deserialize<VgpFile>(jsonString);
                SquaresWide = Math.Max(this.SquaresWide, saveFile.SquaresWide);
                SquaresTall = Math.Max(this.SquaresTall, saveFile.SquaresTall);

                LineLayer lineLayer = (LineLayer)DataLayers[LINE_LAYER];

                //lineLayer.ClearAllLines();

                lineLayer.CreateUndoPoint();
                lineLayer.AddNewLines(saveFile.Lines.ToArray());

                foreach (KeyValuePair<string, IDataLayer> l in DataLayers)
                {
                    l.Value.ForceRedraw();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the currently loaded canvas to the path specified.
        /// </summary>
        /// <param name="fileName">Path to the file to save</param>
        /// <returns>True if the save operation was successful. False otherwise.</returns>
        public bool FileSave(string fileName)
        {
            LineLayer lineLayer = (LineLayer)DataLayers[LINE_LAYER];
            VgpFile saveFile = new VgpFile
            {
                SquaresWide = SquaresWide,
                SquaresTall = SquaresTall,
                SquareSize = TrueSquareSize,
                MarginX = MarginX,
                MarginY = MarginY,
                Lines = lineLayer.LineList
            };
            string jsonString = JsonSerializer.Serialize(saveFile);
            try
            {
                File.WriteAllText(fileName, jsonString);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a PNG image of the current canvas at the specified path in the file system.
        /// </summary>
        /// <param name="fileName">Path to the image to save.</param>
        /// <returns>True if the file exports successfully. False otherwise.</returns>
        public bool FileExport(string fileName)
        {
            SquareSize = TrueSquareSize;
            foreach (KeyValuePair<string,IDataLayer> l in DataLayers)
            {
                l.Value.ForceRedraw();
            }
            SKFileWStream exportedImage = new SKFileWStream(fileName);
            SKBitmap composite = new SKBitmap(GetTotalWidth(), GetTotalHeight());
            SKCanvas canvas = new SKCanvas(composite);
            using (SKPaint whiteBrush = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill })
            {
                canvas.DrawRect(0, 0, composite.Width, composite.Height, whiteBrush);
            }
            GridBackgroundLayer gridBackgroundLayer = (GridBackgroundLayer) DataLayers[GRID_LAYER];
            bool centerLineState = gridBackgroundLayer.DrawCenterLines;
            bool gridLineState = gridBackgroundLayer.DrawGridLines;
            gridBackgroundLayer.DrawCenterLines = ExportCenterLines;
            gridBackgroundLayer.DrawGridLines = ExportGridLines;
            foreach (KeyValuePair<string, IDataLayer> l in DataLayers)
            {
                if (!(l.Value is CursorLayer))
                {
                    canvas.DrawBitmap(l.Value.GenerateLayerBitmap(), l.Value.GetRenderPoint());
                }
            }
            bool result = composite.Encode(exportedImage, SKEncodedImageFormat.Png, 0);
            composite.Dispose();
            exportedImage.Dispose();
            gridBackgroundLayer.DrawCenterLines = centerLineState;
            gridBackgroundLayer.DrawGridLines = gridLineState;
            if (centerLineState || gridLineState)
            {
                gridBackgroundLayer.ForceRedraw();
            }
            return result;
        }

        /// <summary>
        /// Zooms in the user's view by increasing the square size of the grid, up to a maximum of 64 pixels per square.
        /// </summary>
        public void ZoomIn()
        {
            SquareSize = Math.Min(64, SquareSize + 4);
            foreach (KeyValuePair<string, IDataLayer> l in DataLayers)
            {
                l.Value.ForceRedraw();
            }
        }
        /// <summary>
        /// Zooms out the user's view by decreasing the square size of the grid, down to a minimum of 4 pixels per square.
        /// </summary>
        public void ZoomOut()
        {
            SquareSize = Math.Max(4, SquareSize - 4);
            foreach (KeyValuePair<string, IDataLayer> l in DataLayers)
            {
                l.Value.ForceRedraw();
            }
        }
    }
}
