using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace SilverlightProfilerRuntime
{
    public class Profiler
    {
        private static bool shouldProfile;

        public static void EnteringMethod()
        {
            if (!shouldProfile) return;
        }

        public static void ExitingMethod()
        {
            if (!shouldProfile) return;
        }

        public static void Init(UIElement rootVisual)
        {
            rootVisual.KeyDown += (sender, args) => OnKeyDown(args);
        }

        private static void OnKeyDown(KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.F1)
            {
                StartProfiling();
            }
            else if (keyEventArgs.Key == Key.F2)
            {
                StopProfiling();
            }
        }

        private static void StopProfiling()
        {
            shouldProfile = false;
            RadWindow.Confirm("F2", Closed);
        }

        private static void Closed(object sender, WindowClosedEventArgs args)
        {
        }

        private static void StartProfiling()
        {
            shouldProfile = true;
            RadWindow.Confirm("F1", Closed);
        }
    }
}