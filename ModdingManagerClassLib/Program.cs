using ModdingManager.managers.@base;
using ModdingManagerClassLib.Composers;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.Properties;
using System.Drawing;

namespace ModdingManagerDataManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Bitmap bitmap = BitmapExtensions.LoadFromDDS("C:\\Users\\Acer\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\gfx\\interface\\goals\\annex_bya.dds");
            BitmapExtensions.SaveAsDDS(bitmap, "E:\\dada\\мем\\syka.dds");
        }
    }
}
