using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VGraph.src.dataLayers;
using static VGraph.src.dataLayers.LineLayer;

namespace VGraph.src.config
{
    public enum Layers
    {
        Grid = 0,
        Lines = 1,
        Cursor = 2
    }
    //Singleton containing commonly used and modified properties and methods that require wide application access.
    public class PageData
    {
        //Default values produce an 8.5" x 11" piece of paper at 96 dpi.
        public int SquaresWide { get; set; } = 32;
        public int SquaresTall { get; set; } = 42;
        public int SquareSize { get; set; } = 24;
        public int Margin { get; set; } = 24;
        public int TrueSquareSize { get; set; } = 24; //This size is used when saving or exporting.

        private readonly List<IDataLayer> DataLayerList = new List<IDataLayer>();

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

        public List<IDataLayer> GetDataLayers()
        {
            return DataLayerList;
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

                LineLayer lineLayer = (LineLayer)DataLayerList[Convert.ToInt32(Layers.Lines)];

                lineLayer.ClearAllLines();

                foreach (LineSegment l in saveFile.Lines)
                {
                    lineLayer.AddNewLine(l.StartPointGrid, l.EndPointGrid, false);
                }

                foreach (IDataLayer l in DataLayerList)
                {
                    l.ForceRedraw();
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
            LineLayer lineLayer = (LineLayer)DataLayerList[Convert.ToInt32(Layers.Lines)];
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
            foreach (IDataLayer l in DataLayerList)
            {
                l.ForceRedraw();
            }
            SKFileWStream exportedImage = new SKFileWStream(fileName);
            SKBitmap composite = new SKBitmap(GetTotalWidth(), GetTotalHeight());
            SKCanvas canvas = new SKCanvas(composite);
            using (SKPaint whiteBrush = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill })
            {
                canvas.DrawRect(0, 0, composite.Width, composite.Height, whiteBrush);
            }
            foreach (IDataLayer l in DataLayerList)
            {
                if (!(l is CursorLayer))
                {
                    canvas.DrawBitmap(l.GenerateLayerBitmap(), l.GetRenderPoint());
                }
            }
            bool result = composite.Encode(exportedImage, SKEncodedImageFormat.Png, 0);
            composite.Dispose();
            exportedImage.Dispose();
            return result;
        }

        public void ZoomIn()
        {
            SquareSize += 4;
            foreach (IDataLayer l in DataLayerList)
            {
                l.ForceRedraw();
            }
        }

        public void ZoomOut()
        {
            SquareSize = Math.Max(4, SquareSize - 4);
            foreach (IDataLayer l in DataLayerList)
            {
                l.ForceRedraw();
            }
        }
    }
}
