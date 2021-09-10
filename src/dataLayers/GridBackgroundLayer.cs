using SkiaSharp;

using VGraph.src.config;

namespace VGraph.src.dataLayers
{
    public class GridBackgroundLayer : IDataLayer
    {
        private bool RedrawRequired = true;

        private SKBitmap GridBitmap;

        public GridBackgroundLayer()
        {
        }

        public SKBitmap GenerateLayerBitmap()
        {
            if (!RedrawRequired)
            {
                return GridBitmap;
            }

            int xSize = (PageData.Instance.SquaresWide * PageData.Instance.SquareSize) + (PageData.Instance.Margin * 2);
            int ySize = (PageData.Instance.SquaresTall * PageData.Instance.SquareSize) + (PageData.Instance.Margin * 2);

            SKBitmap grid = new SKBitmap(xSize, ySize);

            //Disposables
            SKCanvas gridCanvas = new SKCanvas(grid);
            SKPaint brush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = new SKColor(64, 64, 64, 64) };

            for (int x = 0; x < PageData.Instance.SquaresWide; x++)
            {
                for (int y = 0; y < PageData.Instance.SquaresTall; y++)
                {
                    int xStart = (x * PageData.Instance.SquareSize) + PageData.Instance.Margin;
                    int yStart = (y * PageData.Instance.SquareSize) + PageData.Instance.Margin;
                    SKRectI squareToDraw = new SKRectI(xStart, yStart, xStart + PageData.Instance.SquareSize, yStart + PageData.Instance.SquareSize);
                    gridCanvas.DrawRect(squareToDraw, brush);
                }
            }

            SKPaint borderBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = new SKColor(64, 64, 64, 32) };
            int quarterMargin = PageData.Instance.Margin / 4;
            SKRectI borderSquare = new SKRectI(quarterMargin, quarterMargin, PageData.Instance.GetTotalWidth() - quarterMargin, PageData.Instance.GetTotalHeight() - quarterMargin);
            gridCanvas.DrawRect(borderSquare, borderBrush);

            //Dispose of them.
            gridCanvas.Dispose();
            brush.Dispose();
            borderBrush.Dispose();

            if (GridBitmap != null)
            {
                GridBitmap.Dispose();
            }
            GridBitmap = grid;
            RedrawRequired = false;
            return GridBitmap;
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }

        public void RequireLayerRedraw()
        {
            RedrawRequired = true;
        }
    }
}
