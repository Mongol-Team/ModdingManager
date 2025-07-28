using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Label = System.Windows.Controls.Label;
using RichTextBox = System.Windows.Controls.RichTextBox;
using TextBox = System.Windows.Controls.TextBox;


namespace ModdingManager.classes.utils.fonts
{
    public class FontManager
    {

        public static void CollectUniqueFonts(DependencyObject parent, HashSet<FontSignature> uniqueFonts)
        {
            if (parent == null) return;

            // Проверяем текущий элемент
            switch (parent)
            {
                case RichTextBox rtb:
                    uniqueFonts.Add(new FontSignature(
                        rtb.FontFamily.Source,
                        (int)rtb.FontSize,
                        (rtb.Foreground as SolidColorBrush)?.Color ?? Colors.Black
                    ));
                    break;

                case TextBlock tb:
                    uniqueFonts.Add(new FontSignature(
                        tb.FontFamily.Source,
                        (int)tb.FontSize,
                        (tb.Foreground as SolidColorBrush)?.Color ?? Colors.Black
                    ));
                    break;

                case TextBox txb:
                    uniqueFonts.Add(new FontSignature(
                        txb.FontFamily.Source,
                        (int)txb.FontSize,
                        (txb.Foreground as SolidColorBrush)?.Color ?? Colors.Black
                    ));
                    break;

                case Label lbl:
                    uniqueFonts.Add(new FontSignature(
                        lbl.FontFamily.Source,
                        (int)lbl.FontSize,
                        (lbl.Foreground as SolidColorBrush)?.Color ?? Colors.Black
                    ));
                    break;
            }

            // Рекурсивно проверяем дочерние элементы
            if (parent is Canvas || parent is System.Windows.Controls.Panel || parent is ContentControl)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childrenCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    CollectUniqueFonts(child, uniqueFonts);
                }
            }
        }

    }
}
