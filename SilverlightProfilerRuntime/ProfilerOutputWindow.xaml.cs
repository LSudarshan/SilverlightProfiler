using System.Windows;
using System.Windows.Controls;

namespace SilverlightProfilerRuntime
{
    public partial class ProfilerOutputWindow : ChildWindow
    {
        public ProfilerOutputWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}