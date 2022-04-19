using System;
using System.Collections.Generic;
using System.Windows;
using SkiaSharp;

using VGraph.src.config;
using VGraph.src.drawTools;
using VGraph.src.objects;

namespace VGraph.src.dataLayers
{

    public class LineLayer : IDataLayer
    {
        bool IDataLayer.DrawInExport => true;

        public static Dictionary<string, IDrawTool> Tools = new Dictionary<string, IDrawTool>();

        public const string LINE_TOOL = "Line_Tool";
        public const string BOX_TOOL = "Box_Tool";
        public const string TRI_TOOL = "Tri_Tool";
        public const string CIRCLE_TOOL = "Circle_Tool";
        public const string BOXY_CIRCLE_TOOL = "Boxy_Circle_Tool";
        public const string ELLIPSE_TOOL = "Ellipse_Tool";

        public List<LineSegment> LineList { get; private set; } = new List<LineSegment>();
        private SKBitmap LastImage;
        private bool RedrawRequired;
        public bool PreviewPointActive = false;

        private const int historyCapacity = 20;
        private readonly History<LineSegment[]> UndoHistory = new History<LineSegment[]>(historyCapacity);
        private readonly History<LineSegment[]> RedoHistory = new History<LineSegment[]>(historyCapacity);

        public IDrawTool SelectedTool { get; set; }

        public LineLayer()
        {
            LineLayer.InitializeTools();
            SelectedTool = Tools[LINE_TOOL];
        }

        public static void InitializeTools()
        {
            Tools[LINE_TOOL] = new LineTool();
            Tools[BOX_TOOL] = new BoxTool();
            Tools[TRI_TOOL] = new TriangleTool();
            Tools[CIRCLE_TOOL] = new CircleTool();
            CircleTool boxyTool = new CircleTool
            {
                FuzzRating = 1.0
            };
            Tools[BOXY_CIRCLE_TOOL] = boxyTool;
            Tools[ELLIPSE_TOOL] = new EllipseTool();
        }

        public void SelectTool(string tool)
        {
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
        public void AddNewLines(LineSegment[] l)
        {
            foreach (LineSegment line in l)
            {
                if (line.LineColor == null)
                {
                    line.LineColor = LineSegment.DEFAULT_COLOR.ToString();
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
            UndoHistory.Clear();
            RedoHistory.Clear();
            ForceRedraw();
        }

        /// <summary>
        /// Goes through the list of all line segments on the canvas, and merges all adjacent lines with the same slopes into single lines. This action is repeated until no more merges can be made.
        /// </summary>
        public void MergeAllLines()
        {
            CreateUndoPoint(DeepCopyLineSegmentArray(LineList.ToArray()));
            List<LineSegment> finalList = new List<LineSegment>();
            bool recheck = true;
            while (recheck)
            {
                recheck = false;
                for (int i = 0; i < LineList.Count - 1; i++)
                {
                    for (int j = i + 1; j < LineList.Count; j++)
                    {
                        LineSegment mergeResult = LineList[i].MergeLines(LineList[j]);
                        if (mergeResult != null)
                        {
                            LineList[i] = mergeResult;
                            LineList.RemoveAt(j);
                            recheck = true;
                        }
                    }
                    finalList.Add(LineList[i]);
                }
            }
        }

        public int MirrorLines(int direction, int crease, bool destroyOtherSide)
        {
            const int LEFT_TO_RIGHT = 0;
            const int RIGHT_TO_LEFT = 1;
            const int TOP_TO_BOTTOM = 2;
            const int BOTTOM_TO_TOP = 3;

            const int SUCCESS = 0;
            const int LINES_ACROSS_CREASE = 1;

            List<LineSegment> linesToMirror = new List<LineSegment>();
            List<LineSegment> linesAcrossCrease = new List<LineSegment>();

            //Check if any lines cross the crease. If they do, select them and pop up a message.
            foreach (LineSegment l in LineList)
            {
                if (direction == LEFT_TO_RIGHT || direction == RIGHT_TO_LEFT)
                {
                    if ((l.StartPointGrid.X < crease && l.EndPointGrid.X > crease) || (l.StartPointGrid.X > crease && l.EndPointGrid.X < crease))
                    {
                        linesAcrossCrease.Add(l);
                    }
                }
                if (direction == TOP_TO_BOTTOM || direction == BOTTOM_TO_TOP)
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
                return LINES_ACROSS_CREASE;
            }

            foreach (LineSegment l in LineList)
            {
                switch (direction)
                {
                    case LEFT_TO_RIGHT:
                        if (l.StartPointGrid.X <= crease && l.EndPointGrid.X <= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;

                    case RIGHT_TO_LEFT:
                        if (l.StartPointGrid.X >= crease && l.EndPointGrid.X >= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;

                    case TOP_TO_BOTTOM:
                        if (l.StartPointGrid.Y <= crease && l.EndPointGrid.Y <= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;

                    case BOTTOM_TO_TOP:
                        if (l.StartPointGrid.Y >= crease && l.EndPointGrid.Y >= crease)
                        {
                            linesToMirror.Add(l);
                        }
                        break;
                }
            }

            CreateUndoPoint(DeepCopyLineSegmentArray(LineList.ToArray()));

            if (destroyOtherSide)
            {
                ClearAllLines();
                AddNewLines(linesToMirror.ToArray()); //Not undoable because we still have more to go.
            }

            foreach (LineSegment l in linesToMirror)
            {
                if (direction == LEFT_TO_RIGHT || direction == RIGHT_TO_LEFT)
                {
                    AddNewLine(l.MirrorLineSegment(crease, null));
                }
                else
                {
                    AddNewLine(l.MirrorLineSegment(null, crease));
                }
            }

            ForceRedraw();
            return SUCCESS;
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

        public void DeleteSelectedLines()
        {
            CreateUndoPoint();
            for (int i = LineList.Count - 1; i >= 0; i--)
            {
                if (LineList[i].IsSelected)
                {
                    LineList.RemoveAt(i);
                    ForceRedraw();
                }
            }
        }

        public void MoveSelectedLines(int x, int y)
        {
            bool moveValid = true;
            foreach (LineSegment l in LineList)
            {
                if (l.IsSelected)
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
            }
            foreach (LineSegment l in LineList)
            {
                if (l.IsSelected)
                {
                    l.StartPointGrid = new SKPointI(l.StartPointGrid.X + x, l.StartPointGrid.Y + y);
                    l.EndPointGrid = new SKPointI(l.EndPointGrid.X + x, l.EndPointGrid.Y + y);
                }
            }
            ForceRedraw();
        }

        /// <summary>
        /// Undoes the last user action by restoring the last state contained in "UndoHistory" to the canvas.
        /// </summary>
        public void UndoLastAction()
        {
            LineSegment[] undoState;
            try
            {
                undoState = UndoHistory.Pop();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            CreateRedoPoint(DeepCopyLineSegmentArray(LineList.ToArray()));
            List<LineSegment> l = new List<LineSegment>(undoState);
            LineList = l;
            ForceRedraw();
        }

        public bool CanUndo()
        {
            return UndoHistory.Count > 0;
        }

        public void CreateUndoPoint()
        {
            CreateUndoPoint(DeepCopyLineSegmentArray(LineList.ToArray()));
            RedoHistory.Clear();
        }

        private void CreateUndoPoint(LineSegment[] target)
        {
            UndoHistory.Push(target);
        }

        /// <summary>
        /// Redoes the last user action by restoring the last state contained in "RedoHistory" to the canvas.
        /// </summary>
        public void RedoLastAction()
        {
            LineSegment[] redoState;
            try
            {
                redoState = RedoHistory.Pop();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            CreateUndoPoint(DeepCopyLineSegmentArray(LineList.ToArray()));
            List<LineSegment> l = new List<LineSegment>(redoState);
            LineList = l;
            ForceRedraw();
        }

        public bool CanRedo()
        {
            return RedoHistory.Count > 0;
        }


        public void CreateRedoPoint()
        {
            CreateRedoPoint(DeepCopyLineSegmentArray(LineList.ToArray()));
        }

        private void CreateRedoPoint(LineSegment[] target)
        {
            RedoHistory.Push(target);
        }

        private LineSegment[] DeepCopyLineSegmentArray(LineSegment[] source)
        {
            LineSegment[] rVal = new LineSegment[source.Length];

            for (int i = 0; i < rVal.Length; i++)
            {
                rVal[i] = new LineSegment(source[i].StartPointGrid, source[i].EndPointGrid, source[i].LineColor);
            }

            return rVal;
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
                if (dist < LineSegment.SELECT_RADIUS)
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

        public SKBitmap GenerateLayerBitmap()
        {
            int drawRadius = Math.Max(0, PageData.Instance.SquareSize / 6);
            if (LastImage == null || IsRedrawRequired())
            {
                RedrawRequired = false;
                SKRectI layerSize = GetLayerSize();
                int canvasWidth = layerSize.Width;
                int canvasHeight = layerSize.Height;
                if (canvasWidth < 1 || canvasHeight < 1)
                {
                    return null;
                }

                //Disposables
                SKBitmap image = new SKBitmap(new SKImageInfo(canvasWidth, canvasHeight));
                SKCanvas drawingSurface = new SKCanvas(image);
                SKPaint selectedBrush = new SKPaint { Style = SKPaintStyle.StrokeAndFill, StrokeWidth = (float)(drawRadius + LineSegment.SELECT_RADIUS), Color = SKColors.Black, IsAntialias = true };
                SKPaint standardBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Blue, IsAntialias = true };

                SKPointI topLeft = GetRenderPoint();
                foreach (LineSegment line in LineList)
                {
                    SKColor lineColor = LineSegment.DEFAULT_COLOR;
                    SKColor.TryParse(line.LineColor, out lineColor);
                    standardBrush.Color = lineColor;
                    SKPointI[] canvasPoints = line.GetCanvasPoints();
                    canvasPoints[LineSegment.START].X -= topLeft.X;
                    canvasPoints[LineSegment.START].Y -= topLeft.Y;
                    canvasPoints[LineSegment.END].X -= topLeft.X;
                    canvasPoints[LineSegment.END].Y -= topLeft.Y;
                    if (line.IsSelected)
                    {
                        drawingSurface.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], selectedBrush);
                    }
                    drawingSurface.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], standardBrush);
                }
                if (LastImage != null)
                {
                    LastImage.Dispose();
                }
                LastImage = image;
                //Dispose of them.
                drawingSurface.Dispose();
                selectedBrush.Dispose();
                standardBrush.Dispose();
                RedrawRequired = false;                
            }

            return LastImage;
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired || LineList.Count == 0;
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
                    if (p.X < minX)
                    {
                        minX = p.X;
                    }
                    if (p.Y < minY)
                    {
                        minY = p.Y;
                    }
                    if (p.X > maxX)
                    {
                        maxX = p.X;
                    }
                    if (p.Y > maxY)
                    {
                        maxY = p.Y;
                    }
                }
            }
            return new SKRectI(minX - drawRadius, minY - drawRadius, maxX + drawRadius, maxY + drawRadius);
        }

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }
    }
}
