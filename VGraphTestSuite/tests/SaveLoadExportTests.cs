using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    public class SaveLoadExportTests
    {

        [TestInitialize]
        public void TestInit()
        {
            TestUtils.AssignPageData();
        }

        [TestMethod]
        public void CreateAndSaveABlankGrid()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\blankGridTest.vgp");
            string referencePath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\blankGrid.vgp");
            Assert.IsTrue(PageData.Instance.FileSave(testPath));
            SHA256 hash = SHA256.Create();
            FileStream referenceStream = File.OpenRead(referencePath);
            FileStream testStream = File.OpenRead(testPath);
            Assert.AreEqual(Encoding.UTF8.GetString(hash.ComputeHash(referenceStream)),
                            Encoding.UTF8.GetString(hash.ComputeHash(testStream)));
        }

        [TestMethod]
        public void CreateAndExportABlankGrid()
        {
            string testPath = Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\images\\blankGridTest.png");
            string referencePath = Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + "\\..\\..\\..\\images\\blankGrid.png");
            Assert.IsTrue(PageData.Instance.FileExport(testPath));
            SHA256 hash = SHA256.Create();
            FileStream referenceStream = File.OpenRead(referencePath);
            FileStream testStream = File.OpenRead(testPath);
            Assert.AreEqual(Encoding.UTF8.GetString(hash.ComputeHash(referenceStream)),
                            Encoding.UTF8.GetString(hash.ComputeHash(testStream)));
        }

        [TestMethod]
        public void CreateAndSaveAGridWithLines()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\lineGridTest.vgp");
            string referencePath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\lineGrid.vgp");
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.AddNewLines(new LineTool().DrawWithTool(new SKPointI(0, 0), new SKPointI(1, 1)));
            Assert.IsTrue(PageData.Instance.FileSave(testPath));
            SHA256 hash = SHA256.Create();
            FileStream referenceStream = File.OpenRead(referencePath);
            FileStream testStream = File.OpenRead(testPath);
            Assert.AreEqual(Encoding.UTF8.GetString(hash.ComputeHash(referenceStream)),
                            Encoding.UTF8.GetString(hash.ComputeHash(testStream)));
        }

        [TestMethod]
        public void CreateAndSaveAGridWithLabels()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\labelGridTest.vgp");
            string referencePath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\labelGrid.vgp");
            TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            TextLabel l = new TextLabel(new SKPointI(0,0), "Test", "#ff000000", "Times New Roman", 12, TextLabel.ALIGN_CENTER_CENTER, false);
            TextLabel[] al = { l };
            lText.AddNewLabels(al);
            Assert.IsTrue(PageData.Instance.FileSave(testPath));
            SHA256 hash = SHA256.Create();
            FileStream referenceStream = File.OpenRead(referencePath);
            FileStream testStream = File.OpenRead(testPath);
            Assert.AreEqual(Encoding.UTF8.GetString(hash.ComputeHash(referenceStream)),
                            Encoding.UTF8.GetString(hash.ComputeHash(testStream)));
        }

        [TestMethod]
        public void CreateAndExportAGridWithLines()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\images\\lineGridTest.png");
            string referencePath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\images\\lineGrid.png");
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.AddNewLines(new LineTool().DrawWithTool(new SKPointI(0, 0), new SKPointI(1, 1)));
            Assert.IsTrue(PageData.Instance.FileExport(testPath));
            SHA256 hash = SHA256.Create();
            FileStream referenceStream = File.OpenRead(referencePath);
            FileStream testStream = File.OpenRead(testPath);
            Assert.AreEqual(Encoding.UTF8.GetString(hash.ComputeHash(referenceStream)),
                            Encoding.UTF8.GetString(hash.ComputeHash(testStream)));
        }

        [TestMethod]
        public void CreateAndExportAGridWithLabels()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\images\\labelGridTest.png");
            string referencePath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\images\\labelGrid.png");
            TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            TextLabel l = new TextLabel(new SKPointI(0, 0), "Test", "#ff000000", "Times New Roman", 12, TextLabel.ALIGN_CENTER_CENTER, false);
            TextLabel[] al = { l };
            lText.AddNewLabels(al);
            Assert.IsTrue(PageData.Instance.FileExport(testPath));
            SHA256 hash = SHA256.Create();
            FileStream referenceStream = File.OpenRead(referencePath);
            FileStream testStream = File.OpenRead(testPath);
            Assert.AreEqual(Encoding.UTF8.GetString(hash.ComputeHash(referenceStream)),
                            Encoding.UTF8.GetString(hash.ComputeHash(testStream)));
        }

        [TestMethod]
        public void LoadABlankGrid()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\blankGridTest.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
        }

        [TestMethod]
        public void LoadAGridWithLines()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\lineGridTest.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            Assert.IsTrue(lLines.GetSelectedLines().Length > 0);
        }

        [TestMethod]
        public void LoadAGridWithLabels()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\labelGridTest.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            TextLayer lText = (TextLayer)PageData.Instance.GetDataLayer(PageData.TEXT_LAYER);
            lText.SelectAllLabels();
            Assert.IsTrue(lText.GetSelectedLabels().Length > 0);
        }

        [TestMethod]
        public void LoadLegacyVgpFile()
        {
            string testPath = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\..\\..\\..\\vgps\\legacyLoadTest.vgp");
            Assert.IsTrue(PageData.Instance.FileOpen(testPath));
            LineLayer lLines = (LineLayer)PageData.Instance.GetDataLayer(PageData.LINE_LAYER);
            lLines.SelectAllLines();
            Assert.AreEqual(lLines.GetSelectedLines().Length, 46);
        }
    }
}
