using System;
using System.Collections.Generic;

namespace VGraph.src.objects
{
    //Singleton class containing the undo and redo histories for the current page.
    public class PageHistory
    {
        public struct PageState
        {
            List<LineSegment> Lines;
            List<TextLabel> Labels;
        }

        private const int historyCapacity = 20;
        public History<PageState> UndoHistory = new History<PageState>(historyCapacity);

        public static PageHistory Instance { get; } = new PageHistory();
    }
}
