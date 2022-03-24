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
        private SKBitmap LastBitmap;
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
        /// <param name="isUndoable">If this is true, the state of the canvas before this occurs is added to the undo history. This is false when a file is loaded.</param>
        public void AddNewLines(LineSegment[] l, bool isUndoable)
        {
            if (isUndoable)
            {
                LineSegment[] gridState = LineList.ToArray();
                UndoHistory.Push(gridState);
                RedoHistory.Clear();
            }
            foreach (LineSegment line in l)
            {
                AddNewLine(line);
            }
        }

        /// <summary>
        /// Undoes the last user action by restoring the last state contained in "UndoHistory" to the canvas.
        /// </summary>
        public void UndoLastAction()
        {
            LineSegment[] currentState = LineList.ToArray();
            LineSegment[] undoState;
            try
            {
                undoState = UndoHistory.Pop();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            RedoHistory.Push(currentState);
            List<LineSegment> l = new List<LineSegment>(undoState);
            LineList = l;
            ForceRedraw();
        }

        public void SelectAllLines()
        {
            foreach (LineSegment l in LineList)
            {
                l.IsSelected = true;
            }
            ForceRedraw();
        }

        /// <summary>
        /// Redoes the last user action by restoring the last state contained in "RedoHistory" to the canvas.
        /// </summary>
        public void RedoLastAction()
        {
            LineSegment[] currentState = LineList.ToArray();
            LineSegment[] redoState;
            try
            {
                redoState = RedoHistory.Pop();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            UndoHistory.Push(currentState);
            List<LineSegment> l = new List<LineSegment>(redoState);
            LineList = l;
            ForceRedraw();
        }

        public bool CanUndo()
        {
            return UndoHistory.Count > 0;
        }

        public bool CanRedo()
        {
            return RedoHistory.Count > 0;
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
            UndoHistory.Push(LineList.ToArray());
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

            UndoHistory.Push(LineList.ToArray());

            if (destroyOtherSide)
            {
                ClearAllLines();
                AddNewLines(linesToMirror.ToArray(), false); //Not undoable because we still have more to go.
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
            foreach (LineSegment l in LineList)
            {
                if (l.IsSelected)
                {
                    int targetStartX = l.StartPointGrid.X + x;
                    int targetStartY = l.StartPointGrid.Y + y;
                    int targetEndX = l.EndPointGrid.X + x;
                    int targetEndY = l.EndPointGrid.Y + y;

                    bool moveValid = (Math.Min(targetStartX, targetEndX) >= 0) &&
                                (Math.Min(targetStartY, targetEndY) >= 0) &&
                                (Math.Max(targetStartX, targetEndX) <= PageData.Instance.SquaresWide) &&
                                (Math.Max(targetStartY, targetEndY) <= PageData.Instance.SquaresTall);

                    if (moveValid)
                    {
                        l.StartPointGrid = new SKPointI(l.StartPointGrid.X + x, l.StartPointGrid.Y + y);
                        l.EndPointGrid = new SKPointI(l.EndPointGrid.X + x, l.EndPointGrid.Y + y);
                    }

                }
            }
            ForceRedraw();
        }

        public void HandleSelectionClick(Point point, bool maintainSelection)
        {
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
                    if (l.WasLineSelected(dist, point) || staySelected)
                    {
                        l.IsSelected = true;
                        return;
                    }
                }
            }
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
            if (LastBitmap == null || IsRedrawRequired())
            {
                RedrawRequired = false;
                SKBitmap bitmap = new SKBitmap(PageData.Instance.GetTotalWidth(), PageData.Instance.GetTotalHeight());

                //Disposables
                SKCanvas canvas = new SKCanvas(bitmap);

                SKPaint selectedBrush = new SKPaint { Style = SKPaintStyle.StrokeAndFill, StrokeWidth = (float)(drawRadius + LineSegment.SELECT_RADIUS), Color = SKColors.Black, IsAntialias = true };
                SKPaint standardBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Blue, IsAntialias = true };

                foreach (LineSegment line in LineList)
                {
                    standardBrush.Color = line.LineColor;
                    SKPointI[] canvasPoints = line.GetCanvasPoints();
                    if (line.IsSelected)
                    {
                        canvas.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], selectedBrush);
                    }
                    canvas.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], standardBrush);
                }

                //Dispose of them.
                canvas.Dispose();
                selectedBrush.Dispose();
                standardBrush.Dispose();

                if (LastBitmap != null)
                {
                    LastBitmap.Dispose();
                }
                RedrawRequired = false;
                LastBitmap = bitmap;
            }

            return LastBitmap;
        }

        public bool IsRedrawRequired()
        {
            return RedrawRequired;
        }

        public SKPoint GetRenderPoint()
        {
            return new SKPointI(0, 0);
        }

        public void ForceRedraw()
        {
            RedrawRequired = true;
        }
    }
}
