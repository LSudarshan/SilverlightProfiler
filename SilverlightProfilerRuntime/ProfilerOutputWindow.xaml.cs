using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SilverlightProfilerRuntime
{
    public partial class ProfilerOutputWindow : ChildWindow
    {
        private readonly Call call;

        public ProfilerOutputWindow(Call call)
        {
            this.call = call;
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
                                        {
                                            DefaultExt = "txt",
                                            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                                            FilterIndex = 1

                                        };
            dialog.ShowDialog();
            using(Stream stream = dialog.OpenFile())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    StringBuilder builder = new StringBuilder();
                    call.Dump(builder);
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }
    }
}