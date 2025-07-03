using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
namespace ModdingManager.classes.extentions
{
    public static class RichTextBoxExtensions
    {
        public static string GetTextFromRichTextBox(this System.Windows.Controls.RichTextBox richTextBox) 
        {
            TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return textRange.Text;
        }
    }
}
