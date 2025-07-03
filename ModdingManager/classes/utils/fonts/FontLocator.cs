using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.utils.fonts
{
    public static class FontLocator
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern int GetFontResourceInfo(string lpszFilename, ref int cbBuffer, StringBuilder lpBuffer, uint dwQueryType);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        private const uint FR_PRIVATE = 0x10;
        private const uint GFRI_DESCRIPTION = 1;
        private const uint GFRI_ISTRUETYPE = 3;

        public static string? FindFontFilePath(string fontFamilyName)
        {
            var fonts = new InstalledFontCollection();
            foreach (var font in fonts.Families)
            {
                if (font.Name.Equals(fontFamilyName, StringComparison.OrdinalIgnoreCase))
                {
                    string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                    foreach (var file in Directory.GetFiles(fontsFolder, "*.ttf"))
                    {
                        int size = 256;
                        var buffer = new StringBuilder(size);
                        int result = GetFontResourceInfo(file, ref size, buffer, GFRI_DESCRIPTION);
                        if (result != 0 && buffer.ToString().Equals(font.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return file;
                        }
                    }
                }
            }
            return null;
        }
    }
}
