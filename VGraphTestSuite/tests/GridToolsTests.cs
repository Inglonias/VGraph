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

        [TestMethod]
        public void MoveLinesTest()
        {
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            SKPointI start = new(0, 0);
            SKPointI end = new(10, 10);
            LineSegment[] lineArray = new LineTool().DrawWithTool(start, end);
            lLines.AddNewLines(lineArray);
            lLines.SelectAllLines();

            lLines.MoveSelectedLines(1, 0);
            Assert.AreEqual(lineArray[0].StartPointGrid.X, 1);
            Assert.AreEqual(lineArray[0].StartPointGrid.Y, 0);
            Assert.AreEqual(lineArray[0].EndPointGrid.X, 11);
            Assert.AreEqual(lineArray[0].EndPointGrid.Y, 10);

            lLines.MoveSelectedLines(0, 1);
            Assert.AreEqual(lineArray[0].StartPointGrid.X, 1);
            Assert.AreEqual(lineArray[0].StartPointGrid.Y, 1);
            Assert.AreEqual(lineArray[0].EndPointGrid.X, 11);
            Assert.AreEqual(lineArray[0].EndPointGrid.Y, 11);

            lLines.MoveSelectedLines(-1, 0);
            Assert.AreEqual(lineArray[0].StartPointGrid.X, 0);
            Assert.AreEqual(lineArray[0].StartPointGrid.Y, 1);
            Assert.AreEqual(lineArray[0].EndPointGrid.X, 10);
            Assert.AreEqual(lineArray[0].EndPointGrid.Y, 11);

            lLines.MoveSelectedLines(0, -1);
            Assert.AreEqual(lineArray[0].StartPointGrid.X, 0);
            Assert.AreEqual(lineArray[0].StartPointGrid.Y, 0);
            Assert.AreEqual(lineArray[0].EndPointGrid.X, 10);
            Assert.AreEqual(lineArray[0].EndPointGrid.Y, 10);
        }
    }
}
