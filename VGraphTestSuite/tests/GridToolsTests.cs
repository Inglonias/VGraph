using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using VGraph.src.config;
using VGraph.src.dataLayers;
using VGraph.src.drawTools;
using VGraph.src.objects;
using VGraphTestSuite.util;

namespace VGraphTestSuite
{
    [TestClass]
    public class GridToolsTests
    {

        [TestInitialize]
        public void TestInit()
        {
            TestUtils.AssignPageData();
        }

        [TestMethod]
        public void MergeLineTest()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\GridMergeTest.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            Assert.AreEqual(lLines.GetSelectedLines().Length, 7);
            lLines.DeselectLines();
            lLines.MergeAllLines();
            lLines.SelectAllLines();
            Assert.AreEqual(lLines.GetSelectedLines().Length, 4);
        }

        [TestMethod]
        public void MirrorLineTest()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\GridMirrorTest.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.MirrorLines(0, 5, false);
            lLines.SelectAllLines();
            Assert.AreEqual(lLines.GetSelectedLines().Length, 4);
            SKPointI intersection = new(5, 5);
            foreach (LineSegment l in lLines.GetSelectedLines())
            {
                Assert.IsTrue(intersection.Equals(l.StartPointGrid) || intersection.Equals(l.EndPointGrid));
            }
        }
    }
}
