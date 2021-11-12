using System;
using System.Collections.Generic;
using System.Windows;
using SkiaSharp;

using VGraph.src.config;
using System.Text.Json.Serialization;
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
        public const string CIRCLE_TOOL = "Circle_Tool";
        public const string BOXY_CIRCLE_TOOL = "Boxy_Circle_Tool";

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
            Tools[CIRCLE_TOOL] = new CircleTool();
            CircleTool boxyTool = new CircleTool
            {
                FuzzRating = 1.0
            };
            Tools[BOXY_CIRCLE_TOOL] = boxyTool;
        }
        public void SelectTool(string tool)
        {
            SelectedTool = Tools[tool];
        }

        //public void AddNewLine(SKPointI start, SKPointI end, bool needToConvert)
        //{
        //    if (start.Equals(end))
        //    {
        //        return;
        //    }
        //    LineList.Add(new LineSegment(start, end, needToConvert));
        //    ForceRedraw();
        //}

        private void AddNewLine(LineSegment l)
        {
            if (!l.StartPointGrid.Equals(l.EndPointGrid) &&
                l.StartPointGrid.X >= 0 && l.StartPointGrid.X <= PageData.Instance.SquaresWide &&
                l.StartPointGrid.Y >= 0 && l.StartPointGrid.Y <= PageData.Instance.SquaresTall &&
                l.EndPointGrid.X   >= 0 && l.EndPointGrid.X   <= PageData.Instance.SquaresWide &&
                l.EndPointGrid.Y   >= 0 && l.EndPointGrid.Y   <= PageData.Instance.SquaresTall)
            {
                LineList.Add(l);
                ForceRedraw();
            }
        }

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

        public void MergeAllLines()
        {
            UndoHistory.Push(LineList.ToArray());
            List<LineSegment> finalList = new List<LineSegment>();
            for (int i = 0; i < LineList.Count - 1; i++)
            {
                for (int j = i + 1; j < LineList.Count; j++)
                {
                    LineSegment mergeResult = LineList[i].MergeLines(LineList[j]);
                    if (mergeResult != null)
                    {
                        LineList[i] = mergeResult;
                        LineList.RemoveAt(j);
                    }
                }
                finalList.Add(LineList[i]);
            }
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

        public void HandleSelectionClick(Point point)
        {
            DeselectLines();
            ForceRedraw();
            for (int i = 0; i < LineList.Count; i++)
            {
                double dist = LineList[i].LinePointDistance(point);
                if (dist < LineSegment.SELECT_RADIUS)
                {
                    if (LineList[i].WasLineSelected(dist, point))
                    {
                        LineList[i].IsSelected = true;
                        return;
                    }
                }
            }
        }

        public void HandleBoxSelect(SKRect boundingBox)
        {
            DeselectLines();
            ForceRedraw();
            foreach (LineSegment l in LineList)
            {
                l.IsSelected = false;
                if (l.WasLineSelected(boundingBox))
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

                SKPaint selectedBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Red, IsAntialias = true };
                SKPaint standardBrush = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = drawRadius, Color = SKColors.Blue, IsAntialias = true };

                foreach (LineSegment line in LineList)
                {
                    SKPointI[] canvasPoints = line.GetCanvasPoints();
                    if (line.IsSelected)
                    {
                        canvas.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], selectedBrush);
                    }
                    else
                    {
                        canvas.DrawLine(canvasPoints[LineSegment.START], canvasPoints[LineSegment.END], standardBrush);
                    }
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
            if (PreviewPointActive)
            {
                return true; //Preview lines must always be drawn.
            }
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
