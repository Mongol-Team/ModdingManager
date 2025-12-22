using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
namespace Application.Extentions
{
    public static class RichTextBoxExtensions
    {
        
        public static List<string> GetRichTextBoxLines(this RichTextBox richTextBox)
        {
            var lines = new List<string>();
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string text = textRange.Text;

            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        lines.Add(line.Trim());
                }
            }

            return lines;
        }

        public static void SetRichTextBoxLines(this RichTextBox richTextBox, List<string> lines)
        {
            richTextBox.Document.Blocks.Clear();
            if (lines == null || lines.Count == 0) return;

            var paragraph = new Paragraph();
            foreach (string line in lines)
            {
                paragraph.Inlines.Add(new Run(line));
                paragraph.Inlines.Add(new LineBreak());
            }

            richTextBox.Document.Blocks.Add(paragraph);
        }

        public static string GetRichTextBoxText(this RichTextBox richTextBox)
        {
            var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            return textRange.Text.Trim();
        }
        public static void SetRichTextBoxText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

     
    }
}
