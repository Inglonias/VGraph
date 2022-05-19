using System.Collections.Generic;
using SkiaSharp;
using VGraph.src.config;

namespace VGraph.src.objects
{
    class Polygon
    {
        public static readonly SKColor DEFAULT_COLOR = ConfigOptions.Instance.DefaultLineColor.WithAlpha(128);
        public List<SKPointI> GridVertices { get; set; } = new List<SKPointI>();
        public SKColor PolygonColor { get; set; } = DEFAULT_COLOR;

        public SKPath GetDrawingPath()
        {
            SKPath rVal = new SKPath();
            int startX = (PageData.Instance.SquareSize * GridVertices[0].X) + PageData.Instance.MarginX;
            int startY = (PageData.Instance.SquareSize * GridVertices[0].Y) + PageData.Instance.MarginY;
            rVal.MoveTo(new SKPointI(startX, startY));
            for (int i = 1; i < GridVertices.Count; i++)
            {
                int canvasX = (PageData.Instance.SquareSize * GridVertices[i].X) + PageData.Instance.MarginX;
                int canvasY = (PageData.Instance.SquareSize * GridVertices[i].Y) + PageData.Instance.MarginY;
                rVal.LineTo(new SKPointI(canvasX, canvasY));
            }
            rVal.Close();
            return rVal;
        }
    }
}
