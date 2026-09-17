using Avalonia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGraph.Config;
using VGraph.DrawTools;
using VGraph.Objects;

namespace VGraph.DataLayers
{
    public class LineLayer : IDataLayer
    {
        public static Dictionary<string, IDrawTool> Tools = new Dictionary<string, IDrawTool>();

        public const string LineTool = "LineTool";
        public const string BoxTool = "BoxTool";
        public const string TriTool = "TriTool";
        public const string CircleTool = "CircleTool";
        public const string BoxyCircleTool = "BoxyCircleTool";
        public const string EllipseTool = "EllipseTool";

        public List<LineSegment> LineList { get; set; } = new List<LineSegment>();
        private bool _redrawRequired;
        public bool PreviewPointActive = false;

        public IDrawTool? SelectedTool { get; set; }
        private SKImage? _lastImage;
        SKImage? IDataLayer.LastImage => _lastImage;
        bool IDataLayer.DrawInExport => true;

        public LineLayer()
        {
            LineLayer.InitializeTools();
            SelectedTool = Tools[LineTool];
        }

        public static void InitializeTools()
        {
            Tools[LineTool] = new LineTool();
            Tools[BoxTool] = new BoxTool();
            Tools[TriTool] = new TriangleTool();
            Tools[CircleTool] = new CircleTool();
            Tools[BoxyCircleTool] = new BoxyCircleTool(0.5);
            Tools[EllipseTool] = new EllipseTool();
        }

        public void SelectTool(string tool)
        {
            if (tool.Equals("TextTool"))
            {
                SelectedTool = null;
                return;
            }
            SelectedTool = Tools[tool];
        }

        private void AddNewLine(LineSegment l)
        {
            if (!l.StartPointGrid.Equals(l.EndPointGrid) &&
                l.StartPointGrid.X >= 0 && l.StartPointGrid.X <= PageData.Instance.SquaresWide &&
                l.StartPointGrid.Y >= 0 && l.StartPointGrid.Y <= PageData.Instance.SquaresTall &&
                l.EndPointGrid.X >= 0 && l.EndPointGrid.X <= PageData.Instance.SquaresWide &&
                l.EndPointGrid.Y >= 0 && l.EndPointGrid.Y <= PageData.Instance.SquaresTall &&
                !LineList.Contains(l))
            {
                LineList.Add(l);
                ForceRedraw();
            }
        }

        /// <summary>
        /// Adds all lines contained in the provided array of line segments to the canvas.
        /// </summary>
        /// <param name="l">The array of line segments to add to the canvas</param>
        public void AddNewLines(LineSegment[]? l)
        {
            if (l is null || l.Length == 0)
            {
                return;
            }
            foreach (LineSegment line in l)
            {
                if (line.LineColor == null)
                {
                    line.LineColor = LineSegment.DefaultColor.ToString();
                }
                AddNewLine(line);
            }
        }

        public void SelectAllLines()
        {
            foreach (LineSegment l in LineList)
            {
                l.IsSelected = true;
            }
            ForceRedraw();
        }

        public void ClearAllLines()
        {
            LineList.Clear();
            PageHistory.Instance.ClearUndo();
            PageHistory.Instance.ClearRedo();
            ForceRedraw();
        }

        /// <summary>
        /// Goes through the list of all line segments on the canvas, and merges all adjacent lines with the same slopes into single lines. This action is repeated until no more merges can be made.
        /// </summary>
        public void MergeAllLines()
        {
            PageHistory.Instance.CreateUndoPoint(LineList, null, true);
            bool recheck = true;
            while (recheck)
            {
                recheck = false;
                for (int i = 0; i < LineList.Count - 1; i++)
                {
                    for (int j = i + 1; j < LineList.Count; j++)
                    {
                        LineSegment? mergeResult = LineList[i].MergeLines(LineList[j]);
                        if (mergeResult != null)
                        {
                            LineList[i] = mergeResult;
                            LineList.RemoveAt(j);
                            recheck = true;
                        }
                    }
                }
            }
            PageData.Instance.MakeCanvasDirty();
        }

        public int MirrorLines(int direction, int crease, bool destroyOtherSide, bool oddMode)
        {
            const int leftToRight = 0;
            const int rightToLeft = 1;
            const int topToBottom = 2;
            const int bottomToTop = 3;

            const int successStatus = 0;
            const int linesAcrossCreaseStatus = 1;

            List<LineSegment> linesToMirror = new();
            List<LineSegment> linesAcrossCrease = new();

            //Check if any lines cross the crease. If they do, select them and pop up a message.
            foreach (LineSegment l in LineList)
            {
                if (direction == leftToRight || direction == rightToLeft)
                {
                    if ((l.StartPointGrid.X < crease && l.EndPointGrid.X > crease) || (l.StartPointGrid.X > crease && l.EndPointGrid.X < crease))
                    {
                        linesAcrossCrease.Add(l);
                    }
                }
                if (direction == topToBottom || direction == bottomToTop)
                {
                    if ((l.StartPointGrid.Y < crease && l.EndPointGrid.Y > crease) || (l.StartPointGrid.Y > crease && l.EndPointGrid.Y < crease))
                    {
                        linesAcrossCrease.Add(l);
                    }
                }
            }

            //I don't know how the user wants to handle a line crossing the mirror line, so just give up. Select the relevant lines.
            if (linesAcrossCrease.Count > 0)
            {
                foreach (LineSegment l in linesAcrossCrease)
                {
                    l.IsSelected = true;
                }
                ForceRedraw();
                return linesAcrossCreaseStatus;
            }

            foreach (LineSegment l in LineList)
            {
                switch (direction)
                {
                    case leftToRight:
                        if (l.StartPointGrid.X <= crease && l.EndPointGrid.X <= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;

                    case rightToLeft:
                        if (l.StartPointGrid.X >= crease && l.EndPointGrid.X >= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;

                    case topToBottom:
                        if (l.StartPointGrid.Y <= crease && l.EndPointGrid.Y <= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;

                    case bottomToTop:
                        if (l.StartPointGrid.Y >= crease && l.EndPointGrid.Y >= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;
                }
            }

            PageHistory.Instance.CreateUndoPoint(LineList, null, true);

            if (destroyOtherSide)
            {
                ClearAllLines();
                AddNewLines(linesToMirror.ToArray()); //Not undoable because we still have more to go.
            }

            foreach (LineSegment l in linesToMirror)
            {
                if (direction == leftToRight || direction == rightToLeft)
                {
                    AddNewLine(l.MirrorLineSegment(crease, null, oddMode));
                }
                else
                {
                    AddNewLine(l.MirrorLineSegment(null, crease, oddMode));
                }
            }
            PageData.Instance.MakeCanvasDirty();
            ForceRedraw();
            return successStatus;
        }

        public LineSegment[] GetSelectedLines()
        {
            List<LineSegment> selectedLines = new List<LineSegment>();
            foreach (LineSegment l in LineList)
            {
                if (l.IsSelected)
                {
                    selectedLines.Add(l);
                }
            }
            return selectedLines.ToArray();
        }

        public void DeselectLines()
        {
            foreach (LineSegment l in LineList)
            {
                l.IsSelected = false;
            }
            ForceRedraw();
        }

        public bool DeleteSelectedLines()
        {
            bool linesDeleted = false;

            for (int i = LineList.Count - 1; i >= 0; i--)
            {
                if (LineList[i].IsSelected)
                {
                    if (!linesDeleted)
                    {
                        linesDeleted = true;
                        PageHistory.Instance.CreateUndoPoint(LineList, null, true);
                        PageData.Instance.MakeCanvasDirty();
                    }
                    LineList.RemoveAt(i);
                    ForceRedraw();
                }
            }
            return linesDeleted;
        }

        public void MoveSelectedLines(int x, int y)
        {
            LineSegment[] targetLines = GetSelectedLines();
            if (targetLines.Length == 0)
            {
                return;
            }
            bool moveValid = true;
            foreach (LineSegment l in GetSelectedLines())
            {
                int targetStartX = l.StartPointGrid.X + x;
                int targetStartY = l.StartPointGrid.Y + y;
                int targetEndX = l.EndPointGrid.X + x;
                int targetEndY = l.EndPointGrid.Y + y;

                moveValid = (Math.Min(targetStartX, targetEndX) >= 0) &&
                            (Math.Min(targetStartY, targetEndY) >= 0) &&
                            (Math.Max(targetStartX, targetEndX) <= PageData.Instance.SquaresWide) &&
                            (Math.Max(targetStartY, targetEndY) <= PageData.Instance.SquaresTall);
                if (!moveValid)
                {
                    return;
                }
            }
            foreach (LineSegment l in GetSelectedLines())
            {
                l.StartPointGrid = new SKPointI(l.StartPointGrid.X + x, l.StartPointGrid.Y + y);
                l.EndPointGrid = new SKPointI(l.EndPointGrid.X + x, l.EndPointGrid.Y + y);
            }
            PageData.Instance.MakeCanvasDirty();
            ForceRedraw();
        }

        public bool HandleSelectionClick(Point point, bool maintainSelection)
        {
            //This function does NOT handle box selections. Because of that, we're looking for one line that was clicked on.
            //As soon as we find that line, we return true. If we go through the whole list and find nothing, return false.
            if (PageData.Instance.IsEyedropperActive)
            {
                DeselectLines();
            }

            if (!maintainSelection)
            {
                DeselectLines();
            }
            ForceRedraw();
            foreach (LineSegment l in LineList)
            {
                double dist = l.LinePointDistance(point);
                if (dist < LineSegment.SelectRadius)
                {
                    bool staySelected = l.IsSelected && maintainSelection;
                    if (staySelected && !l.WasLineSelected(dist, point))
                    {
                        l.IsSelected = true;
                    }
                    if (l.WasLineSelected(dist, point))
                    {
                        l.IsSelected = !l.IsSelected;
                        if (PageData.Instance.IsEyedropperActive)
                        {
                            PageData.Instance.CurrentLineColor = SKColor.Parse(l.LineColor);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void HandleBoxSelect(SKRect boundingBox, bool maintainSelection)
        {
            if (!maintainSelection)
            {
                DeselectLines();
            }
            ForceRedraw();
            foreach (LineSegment l in LineList)
            {
                bool staySelected = l.IsSelected && maintainSelection;
                if (l.WasLineSelected(boundingBox) || staySelected)
                {
                    l.IsSelected = true;
                }
            }
        }

        public bool IsRedrawRequired()
        {
            return _redrawRequired || LineList.Count == 0;
        }

        public SKPointI GetRenderPoint()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();

            foreach (LineSegment l in LineList)
            {
                foreach (SKPointI p in l.GetCanvasPoints())
                {
                    if (p.X < minX)
                    {
                        minX = p.X;
                    }
                    if (p.Y < minY)
                    {
                        minY = p.Y;
                    }
                }
            }

            return new SKPointI(minX - drawRadius, minY - drawRadius);
        }

        private SKRectI GetLayerSize()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            int minX = PageData.Instance.GetTotalWidth();
            int minY = PageData.Instance.GetTotalHeight();
            int maxX = 0;
            int maxY = 0;
            foreach (LineSegment l in LineList)
            {
                foreach (SKPointI p in l.GetCanvasPoints())
                {
                    minX = Math.Min(p.X, minX);
                    minY = Math.Min(p.Y, minY);
                    maxX = Math.Max(p.X, maxX);
                    maxY = Math.Max(p.Y, maxY);
                }
            }
            return new SKRectI(minX - drawRadius, minY - drawRadius, maxX + drawRadius, maxY + drawRadius);
        }

        public void ForceRedraw()
        {
            _redrawRequired = true;
        }

        public SKImage? GenerateLayerImage()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            if (_lastImage == null || IsRedrawRequired())
            {
                _redrawRequired = false;
                SKRectI layerSize = GetLayerSize();
                int canvasWidth = layerSize.Width;
                int canvasHeight = layerSize.Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }
                SKSurface drawingSurface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
                var drawingCanvas = drawingSurface.Canvas;

                //Disposables

                SKPaint selectedBrush = new SKPaint { Style = SKPaintStyle.StrokeAndFill, StrokeWidth = (float)(drawRadius + LineSegment.SelectRadius), Color = ConfigOptions.Instance.LineHighlightColor, IsAntialias = true };
                SKPaint standardBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Blue, IsAntialias = true };

                SKPointI topLeft = GetRenderPoint();
                foreach (LineSegment line in LineList)
                {
                    SKColor lineColor = LineSegment.DefaultColor;
                    SKColor.TryParse(line.LineColor, out lineColor);
                    standardBrush.Color = lineColor;
                    SKPointI[] canvasPoints = line.GetCanvasPoints();
                    canvasPoints[LineSegment.Start].X -= topLeft.X;
                    canvasPoints[LineSegment.Start].Y -= topLeft.Y;
                    canvasPoints[LineSegment.End].X -= topLeft.X;
                    canvasPoints[LineSegment.End].Y -= topLeft.Y;
                    if (line.IsSelected)
                    {
                        drawingCanvas.DrawLine(canvasPoints[LineSegment.Start], canvasPoints[LineSegment.End], selectedBrush);
                    }
                    drawingCanvas.DrawLine(canvasPoints[LineSegment.Start], canvasPoints[LineSegment.End], standardBrush);
                }
                //Dispose of them.
                if (_lastImage != null)
                {
                    _lastImage.Dispose();
                }
                _lastImage = drawingSurface.Snapshot();
                drawingSurface.Dispose();
                selectedBrush.Dispose();
                standardBrush.Dispose();
                _redrawRequired = false;
            }
            return _lastImage;
        }
    }
}
