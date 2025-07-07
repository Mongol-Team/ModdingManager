using System.Windows.Documents;

namespace ModdingManager.classes.extentions
{
    public static class TextBoxExtensions
    {
        public static List<string> GetLines(this System.Windows.Controls.RichTextBox rtb)
        {
            var lines = new List<string>();
            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string fullText = textRange.Text;

            // Разбиваем по строкам (учитывая \r\n, \n и \r)
            var split = fullText.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);
            foreach (var line in split)
            {
                lines.Add(line);
            }

            return lines;
        }

        public static List<string> GetLines(this RichTextBox rtb)
        {
            var lines = new List<string>();
            foreach (var line in rtb.Lines)
            {
                lines.Add(line);
            }
            return lines;
        }
    }
}
