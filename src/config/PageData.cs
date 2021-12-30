using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VGraph.src.dataLayers;
using VGraph.src.objects;
using static VGraph.src.dataLayers.LineLayer;

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
        public int Margin { get; set; } = 24;
        public int TrueSquareSize { get; set; } = 24; //This size is used when saving or exporting.

        private readonly Dictionary<string, IDataLayer> DataLayers = new Dictionary<string, IDataLayer>();


        /// <summary>
        /// Calculate the total width of the canvas in pixels.
        /// </summary>
        /// <returns>Width of the canvas in pixels.</returns>
        public int GetTotalWidth()
        {
            return (SquaresWide * SquareSize) + (Margin * 2);
        }

        /// <summary>
        /// Calculate the total height of the canvas in pixels.
        /// </summary>
        /// <returns>Height of the canvas in pixels.</returns>
        public int GetTotalHeight()
        {
            return (SquaresTall * SquareSize) + (Margin * 2);
        }

        public Dictionary<string, IDataLayer> GetDataLayers()
        {
            return DataLayers;
        }

        public IDataLayer GetDataLayer(string key)
        {
            return DataLayers[key];
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
                Margin = saveFile.Margin;

                LineLayer lineLayer = (LineLayer)DataLayers[LINE_LAYER];

                lineLayer.ClearAllLines();

                lineLayer.AddNewLines(saveFile.Lines.ToArray(), false);

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

        public bool FileSave(string fileName)
        {
            LineLayer lineLayer = (LineLayer)DataLayers[LINE_LAYER];
            VgpFile saveFile = new VgpFile
            {
                SquaresWide = SquaresWide,
                SquaresTall = SquaresTall,
                SquareSize = TrueSquareSize,
                Margin = Margin,
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
            GridBackgroundLayer gbl = (GridBackgroundLayer) DataLayers[GRID_LAYER];
            bool centerLineState = gbl.DrawCenterLines;
            gbl.DrawCenterLines = false;
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
            gbl.DrawCenterLines = centerLineState;
            if (centerLineState)
            {
                gbl.ForceRedraw();
            }
            return result;
        }

        public void ZoomIn()
        {
            SquareSize = Math.Min(64, SquareSize + 4);
            foreach (KeyValuePair<string, IDataLayer> l in DataLayers)
            {
                l.Value.ForceRedraw();
            }
        }

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
