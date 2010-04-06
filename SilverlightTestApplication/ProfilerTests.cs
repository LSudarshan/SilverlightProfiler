using System.Linq;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilverlightProfilerRuntime;

namespace SilverlightTestApplication
{
    [TestClass]
    public class ProfilerTests : SilverlightTest
    {
        protected Call Root
        {
            get
            {
                return Profiler.stacksPerThread.Values.ToList()[0].Peek();
            }
        }

        [TestMethod]
        [Asynchronous]
        public void ShouldStartAndStopProfilingCorrectly()
        {
            StartProfiling();
            StopProfiling();
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public void ShouldProfileMethodsCorrectly()
        {
            StartProfiling();
            EnqueueCallback(() => new SomeClass().A());
            EnqueueCallback(() => Assert.AreEqual(2, Root.Children.Count, "There should be 2 calls"));
            EnqueueCallback(
                () =>
                Assert.AreEqual("SilverlightTestApplication.SomeClass..ctor", Root.Children[0].FullName,
                                "First call should be for constructor"));
            EnqueueCallback(() => Assert.AreEqual("SilverlightTestApplication.SomeClass.A", Root.Children[1].FullName));
            EnqueueCallback(() => Assert.AreEqual(1, Root.Children[1].Children.Count));
            EnqueueCallback(
                () => Assert.AreEqual("SilverlightTestApplication.SomeClass.B", Root.Children[1].Children[0].FullName));
            StopProfiling();
            EnqueueTestComplete();
        }
        
        [TestMethod]
        [Asynchronous]
        public void ShouldProfileMethodsWhichThrowExceptionsCorrectly()
        {
            StartProfiling();
            EnqueueCallback(() => SomeClass.E());
            EnqueueCallback(() => Assert.AreEqual(1, Root.Children.Count));
            EnqueueCallback(
                () =>
                Assert.AreEqual("SilverlightTestApplication.SomeClass.E", Root.Children[0].FullName));
            EnqueueCallback(() => Assert.AreEqual(1, Root.Children[0].Children.Count));
            EnqueueCallback(
                () => Assert.AreEqual("SilverlightTestApplication.SomeClass.ThrowsException", Root.Children[0].Children[0].FullName));
            StopProfiling();
            EnqueueTestComplete();
        }

        private void StopProfiling()
        {
            EnqueueCallback(() => Profiler.StopProfiling());
            EnqueueConditional(() => Profiler.ProfilerOutputWindow.Visibility == Visibility.Visible);
            EnqueueCallback(() => Profiler.ProfilerOutputWindow.Close());
        }

        private void StartProfiling()
        {
            EnqueueCallback(() => Profiler.StartProfiling());
            EnqueueConditional(() => Profiler.StartWindow.Visibility == Visibility.Visible);
            EnqueueCallback(() => Profiler.StartWindow.Close());
        }
    }
}