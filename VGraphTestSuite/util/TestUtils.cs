using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGraph.src.config;
using VGraph.src.dataLayers;

namespace VGraphTestSuite.util
{
    public class TestUtils
    {
        public static void AssignPageData()
        {
            GridBackgroundLayer lGrid = new();
            LineLayer lLines = new();
            CursorLayer lCursor = new();
            PreviewLayer lPreview = new();

            PageData.Instance.GetDataLayers()[PageData.GRID_LAYER] = lGrid;
            PageData.Instance.GetDataLayers()[PageData.LINE_LAYER] = lLines;
            PageData.Instance.GetDataLayers()[PageData.PREVIEW_LAYER] = lPreview;
            PageData.Instance.GetDataLayers()[PageData.CURSOR_LAYER] = lCursor;
        }
    }
}
