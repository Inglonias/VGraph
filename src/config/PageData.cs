using SkiaSharp;
using System.Collections.Generic;
using VGraph.src.dataLayers;

namespace VGraph.src.config
{
    //Singleton containing commonly used and modified properties and methods that require wide application access.
    public class PageData
    {
        //Default values are a best guess for 4 squares per inch, at a size of 8.5" x 11"
        public int SquaresWide { get; set; } = 30;
        public int SquaresTall { get; set; } = 39;
        public int SquareSize { get; set; } = 24;
        public int Margin { get; set; } = 48;

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

        public List<IDataLayer> GetDataLayers() {
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
            return false;
        }

        public bool FileSave(string fileName)
        {
            return false;
        }

        public bool FileExport(string fileName)
        {
            SKFileWStream exportedImage = new SKFileWStream(fileName);
            SKBitmap composite = new SKBitmap(GetTotalWidth(), GetTotalHeight());
            SKCanvas canvas = new SKCanvas(composite);
            foreach (IDataLayer l in DataLayerList)
            {
                if (!(l is CursorLayer))
                {
                    canvas.DrawBitmap(l.GenerateLayerBitmap(), l.GetRenderPoint());
                }
            }
            return composite.Encode(exportedImage, SKEncodedImageFormat.Png, 0);
        }

    }
}
