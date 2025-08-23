using ModdingManagerClassLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using RichTextBox = System.Windows.Controls.RichTextBox;
namespace ModdingManager.WPFExtensions
{
    public static class DebuggerExtentions
    {
        public static void AttachToWindow(this Debugger debugger, Window window, RichTextBox outputBox)
        {
            debugger.OutputHandler = log =>
            {
                outputBox.Dispatcher.Invoke(() =>
                {
                    outputBox.Document.Blocks.Clear();
                    outputBox.Document.Blocks.Add(new Paragraph(new Run(log)));
                    outputBox.ScrollToEnd();
                });
            };
        }
    }
}
