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
    public class DrawToolsTests
    {

        [TestInitialize]
        public void TestInit()
        {
            TestUtils.AssignPageData();
        }

        [TestMethod]
        public void TestLineTool()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\toolTestLine.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            SKPointI start = new(0, 0);
            SKPointI end = new(10, 10);
            LineSegment[] referenceLines = lLines.GetSelectedLines();
            LineSegment[] testLines = new LineTool().DrawWithTool(start, end);
            Assert.AreEqual(referenceLines.Length, testLines.Length);
            for (int i = 0; i < testLines.Length; i++)
            {
                if (!referenceLines[i].Equals(testLines[i]))
                {
                    Assert.Fail("Tool does not match reference image");
                }
            }
        }

        [TestMethod]
        public void TestBoxTool()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\toolTestBox.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            LineSegment[] referenceLines = lLines.GetSelectedLines();
            SKPointI start = new(0, 0);
            SKPointI end = new(10, 10);
            LineSegment[] testLines = new BoxTool().DrawWithTool(start, end);
            Assert.AreEqual(referenceLines.Length, testLines.Length);
            for (int i = 0; i < testLines.Length; i++)
            {
                if (!referenceLines[i].Equals(testLines[i]))
                {
                    Assert.Fail("Tool does not match reference image");
                }
            }
        }

        [TestMethod]
        public void TestTriangleTool()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\toolTestTriangle.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            LineSegment[] referenceLines = lLines.GetSelectedLines();
            SKPointI start = new(0, 0);
            SKPointI end = new(10, 10);
            LineSegment[] testLines = new TriangleTool().DrawWithTool(start, end);
            Assert.AreEqual(referenceLines.Length, testLines.Length);
            for (int i = 0; i < testLines.Length; i++)
            {
                if (!referenceLines[i].Equals(testLines[i]))
                {
                    Assert.Fail("Tool does not match reference image");
                }
            }
        }

        [TestMethod]
        public void TestEllipseTool()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\toolTestEllipse.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            LineSegment[] referenceLines = lLines.GetSelectedLines();
            SKPointI start = new(5, 5);
            SKPointI end = new(1, 0);
            LineSegment[] testLines = new EllipseTool().DrawWithTool(start, end);
            Assert.AreEqual(referenceLines.Length, testLines.Length);
            for (int i = 0; i < testLines.Length; i++)
            {
                if (!referenceLines[i].Equals(testLines[i]))
                {
                    Assert.Fail("Tool does not match reference image");
                }
            }
        }

        [TestMethod]
        public void TestEllipseToolOdd()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\toolTestEllipseOdd.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            LineSegment[] referenceLines = lLines.GetSelectedLines();
            SKPointI start = new SKPointI(5, 5);
            SKPointI end = new SKPointI(1, 0);
            LineSegment[] testLines = new EllipseTool().DrawWithToolOdd(start, end);
            Assert.AreEqual(referenceLines.Length, testLines.Length);
            for (int i = 0; i < testLines.Length; i++)
            {
                if (!referenceLines[i].Equals(testLines[i]))
                {
                    Assert.Fail("Tool does not match reference image");
                }
            }
        }
    }
}
