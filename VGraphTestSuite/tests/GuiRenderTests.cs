using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VGraph.src.ui;

namespace VGraphTestSuite
{
    [TestClass]
    public class GuiRenderTests
    {
        //Class blatantly stolen from https://getyourbitstogether.com/wpf-and-mstest/
        //The above link was blatantly stolen from https://www.meziantou.net/mstest-v2-customize-test-execution.htm
        private class WpfTestMethodAttribute : TestMethodAttribute
        {
            public override TestResult[] Execute(ITestMethod testMethod)
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                    return Invoke(testMethod);

                TestResult[] result = null;
                var thread = new Thread(() => result = Invoke(testMethod));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                return result;
            }

            private TestResult[] Invoke(ITestMethod testMethod)
            {
                return new[] { testMethod.Invoke(null) };
            }
        }

        [WpfTestMethod]
        public void TestMainWindow()
        {
            MainWindow mw = new MainWindow();
            mw.Show();
        }

        [WpfTestMethod]
        public void TestNewGridWindow()
        {
            NewGridWindow ngw = new NewGridWindow(false);
            ngw.MainWindowParent = new MainWindow();
            ngw.Show();
        }

        [WpfTestMethod]
        public void TestMirrorGridWindow()
        {
            MirrorToolWindow mtw = new MirrorToolWindow();
            mtw.Show();
        }

        [WpfTestMethod]
        public void TestConfigWindow()
        {
            ConfigOptionsWindow cow = new ConfigOptionsWindow();
            //Moooooooooo
            cow.Show();
        }

        [WpfTestMethod]
        public void TestLabelWindow()
        {
            LabelPropertiesWindow lpw = new LabelPropertiesWindow();
            lpw.Show();
        }
    }
}
