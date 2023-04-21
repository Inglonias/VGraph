using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGraph.src.objects
{
    //Singleton class containing the undo and redo histories for the current page.
    public class PageHistory
    {
        //I only expect one of these lists to be non-null at any given time, because we're undoing single operations here.
        //That said, if both are not null, we need to account for that.
        //A null value represents no change.
        public struct PageState
        {
            public List<LineSegment>? Lines { get; set; }
            public List<TextLabel>? Labels { get; set; }
        }

        private const int HistoryCapacity = 20;
        public History<PageState> UndoHistory = new History<PageState>(HistoryCapacity);
        public History<PageState> RedoHistory = new History<PageState>(HistoryCapacity);

        public static PageHistory Instance { get; } = new PageHistory();

        public bool CanUndo()
        {
            return UndoHistory.Count > 0;
        }

        public bool CanRedo()
        {
            return RedoHistory.Count > 0;
        }

        public void CreateUndoPoint(List<LineSegment>? lineList, List<TextLabel>? labelList, bool clearRedo)
        {
            PageState undoPoint = new PageState();
            if (lineList != null)
            {
                undoPoint.Lines = DeepCopyList(lineList);
            }
            if (labelList != null)
            {
                undoPoint.Labels = DeepCopyList(labelList);
            }
            UndoHistory.Push(undoPoint);
            if (clearRedo)
            {
                RedoHistory.Clear();
            }
        }

        public void CreateUndoPoint (PageState ps)
        {
            UndoHistory.Push(ps);
            RedoHistory.Clear();
        }
        
        public void CreateRedoPoint(List<LineSegment>? lineList, List<TextLabel>? labelList)
        {
            PageState redoPoint = new PageState();
            if (lineList != null)
            {
                redoPoint.Lines = DeepCopyList(lineList);
            }
            if (labelList != null)
            {
                redoPoint.Labels = DeepCopyList(labelList);
            }
            RedoHistory.Push(redoPoint);
        }

        public void CreateRedoPoint(PageState ps)
        {
            RedoHistory.Push(ps);
        }

        public PageState PopUndoAction()
        {
            return UndoHistory.Pop();
        }

        public PageState PopRedoAction()
        {
            return RedoHistory.Pop();
        }

        public void ClearUndo()
        {
            UndoHistory.Clear();
        }

        public void ClearRedo()
        {
            RedoHistory.Clear();
        }

        public void MergeLineLabelUndo()
        {
            PageState ps1 = UndoHistory.Pop();
            PageState ps2 = UndoHistory.Pop();

            if (ps1.Labels == null && ps2.Lines == null)
            {
                UndoHistory.Push(new PageState
                {
                    Lines = ps1.Lines,
                    Labels = ps2.Labels
                });
            }
            else if (ps1.Lines == null && ps2.Labels == null)
            {
                UndoHistory.Push(new PageState
                {
                    Lines = ps2.Lines,
                    Labels = ps1.Labels
                });
            }
        }

        private List<T> DeepCopyList<T>(List<T> source)
        {
            List<T> rVal = new List<T>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                rVal.Add(source[i]);
            }

            return rVal;
        }
    }
}
