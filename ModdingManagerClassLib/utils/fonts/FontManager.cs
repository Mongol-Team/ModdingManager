using HarfBuzzSharp;
using ModdingManager.classes.extentions;
using ModdingManagerModels.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Text;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using static ModdingManagerModels.SuperEventModels.SuperEventGuiElements;
using HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment;
using SystemFonts = SixLabors.Fonts.SystemFonts;
using VerticalAlignment = SixLabors.Fonts.VerticalAlignment;


namespace ModdingManager.classes.utils.fonts
{
    public class FontManager
    {
        public enum CoverageResult { Full, MissingCyrillic, MissingLatin }

        public static CoverageResult ValidateCoverage(string fontPath)
        {
            byte[] fontData = File.ReadAllBytes(fontPath);
            var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            try
            {
                using var blob = new HarfBuzzSharp.Blob(ptr, fontData.Length, MemoryMode.ReadOnly, () => handle.Free());
                using var face = new Face(blob, 0);
                using var hbFont = new HarfBuzzSharp.Font(face);
                if (!HasAnyGlyph(hbFont, 0x0020, 0x007E))
                    return CoverageResult.MissingLatin;
                if (!HasAnyGlyph(hbFont, 0x0400, 0x052F))
                    return CoverageResult.MissingCyrillic;

                return CoverageResult.Full;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        private static bool HasAnyGlyph(HarfBuzzSharp.Font hbFont, int from, int to)
        {
            for (int cp = from; cp <= to; cp++)
            {
                using var buf = new HarfBuzzSharp.Buffer();
                buf.AddUtf8(char.ConvertFromUtf32(cp));
                buf.GuessSegmentProperties();
                hbFont.Shape(buf, null);

                var infos = buf.GlyphInfos;
                if (infos.Length > 0 && infos[0].Codepoint != 0)
                    return true;
            }
            return false;
        }

        public static void HandleFontFolderWithChecks(string outputPath, FontSignature font)
        {
            string fontPath = FindFontPath(font.Family);

            switch (ValidateCoverage(fontPath))
            {
                case CoverageResult.MissingLatin:
                    ShowMissingLatinAndAbort();
                    throw new OperationCanceledException();      // прерываем всю цепочку

                case CoverageResult.MissingCyrillic:
                    if (!AskContinueOnMissingCyrillic())
                        throw new OperationCanceledException();      // пользователь отменил

                    break;
            }

            HandleFontFolder(outputPath, font);
        }

        public static bool AskContinueOnMissingCyrillic()
        {
            var msg = "В шрифте отсутствуют кириллические символы.\nПродолжить без кириллицы?";
            var res = System.Windows.MessageBox.Show(msg, "Предупреждение",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Warning);
            return res == MessageBoxResult.Yes;
        }

        public static void ShowMissingLatinAndAbort()
        {
            System.Windows.MessageBox.Show(
                "В шрифте отсутствуют даже латинские символы.\nОперация прервана.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private static string FindFontPath(string familyName)
        {
            // 1. Прямое получение пути через GlyphTypeface (работает для всех системных шрифтов)
            var fontFamily = Fonts.SystemFontFamilies
                .FirstOrDefault(f => string.Equals(f.Source, familyName, StringComparison.OrdinalIgnoreCase));

            if (fontFamily != null)
            {
                foreach (var typeface in fontFamily.GetTypefaces())
                {
                    if (typeface.TryGetGlyphTypeface(out var glyph))
                    {
                        string path = glyph.FontUri.LocalPath;
                        if (File.Exists(path)) return path;
                    }
                }
            }

            // 2. Резервный поиск по метаданным в папке Fonts
            string fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            foreach (var file in Directory.GetFiles(fontsDir, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    // Пропускаем не-шрифтовые файлы
                    var ext = Path.GetExtension(file).ToLower();
                    if (!ext.EndsWith(".ttf") && !ext.EndsWith(".otf")) continue;

                    // Анализируем метаданные шрифта
                    var collection = new PrivateFontCollection();
                    collection.AddFontFile(file);

                    if (collection.Families.Length > 0)
                    {
                        string fontFamilyName = collection.Families[0].Name;
                        if (string.Equals(fontFamilyName, familyName, StringComparison.OrdinalIgnoreCase))
                        {
                            return file;
                        }
                    }
                }
                catch
                {
                    // Игнорируем поврежденные файлы
                }
            }

            return null;
        }

        public static string GenerateFontName(FontSignature font)
        {
            string colorHex = font.Color.ToString().Replace("#", "");
            string fontName = $"{CleanName(font.Family)}_{font.Size}";
            return CleanFontName(fontName);
        }

        private static string CleanName(string name)
        {
            return name.Replace(" ", "_").ToLowerInvariant();
        }

        public static string CleanFontName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRe = string.Format(@"[{0}]+", invalidChars);
            string cleaned = Regex.Replace(name, invalidRe, "_");
            return Regex.Replace(cleaned, @"_+", "_").Trim('_');
        }


        public static void HandleFontFolder(string outputPath, FontSignature font, int textureWidth = 1024, int textureHeight = 1024, int padding = 3)
        {
            string faceName = string.IsNullOrEmpty(font.Family) ? "Unknown" : font.Family;

            // 2. Загрузка метрик через HarfBuzzSharp (поддержка TTF/OTF)
            byte[] fontData = File.ReadAllBytes(font.Path);
            var handle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();

            using var blob = new HarfBuzzSharp.Blob(ptr, fontData.Length, MemoryMode.ReadOnly, () => handle.Free());
            using var hbFace = new Face(blob, 0);
            using var hbFont = new HarfBuzzSharp.Font(hbFace);

            int upem = hbFace.UnitsPerEm; // Исправлено на правильное свойство
            hbFont.SetScale(upem, upem);

            FontExtents fontExtents;
            if (!hbFont.TryGetHorizontalFontExtents(out fontExtents)) // Исправлено на TryGet
            {
                fontExtents = new FontExtents { Ascender = upem, Descender = 0, LineGap = 0 };
            }

            float scale = font.Size / (float)upem;
            int ascender = fontExtents.Ascender;
            int descender = fontExtents.Descender;
            int lineGap = fontExtents.LineGap;
            int lineHeight = (int)Math.Ceiling((ascender - descender + lineGap) * scale);
            int baseLine = font.Size;

            // 3. Подготовка SixLabors.Font (поддержка OTF/TTF)
            SixLabors.Fonts.Font sysFont;
            try
            {
                var fontCollection = new SixLabors.Fonts.FontCollection();
                SixLabors.Fonts.FontFamily family = fontCollection.Add(font.Path);
                sysFont = family.CreateFont(font.Size);
            }
            catch
            {
                sysFont = SystemFonts.TryGet(faceName, out var family)
                    ? family.CreateFont(font.Size)
                    : SystemFonts.Families.First().CreateFont(font.Size);
            }

            var characters = new List<char> { (char)1, (char)2, (char)3 };
            characters.AddRange(Enumerable.Range(32, 224).Select(i => (char)i));
            for (int c = 0x0400; c <= 0x04FF; c++) characters.Add((char)c);
            for (int c = 0x0500; c <= 0x052F; c++) characters.Add((char)c);
            using SixLabors.ImageSharp.Image<Rgba32> img = new SixLabors.ImageSharp.Image<Rgba32>(textureWidth, textureHeight);
            img.Mutate(ctx => ctx.Clear(SixLabors.ImageSharp.Color.Transparent));

            var sb = new StringBuilder()
                .AppendLine($"info face=\"{faceName}\" size={font.Size} bold=0 italic=0 charset=\"\" unicode=1 stretchH=100 smooth=1 aa=2 padding={padding},{padding},{padding},{padding} spacing=1,1 outline=0")
                .AppendLine($"common lineHeight={lineHeight} base={baseLine} scaleW={textureWidth} scaleH={textureHeight} pages=1 packed=0 alphaChnl=1 redChnl=0 greenChnl=0 blueChnl=0")
                .AppendLine($"page id=0 file=\"{GenerateFontName(font)}.dds\"")
                .AppendLine($"chars count={characters.Count}");

            int x = padding, y = padding;

            /// 6. Растеризация каждого символа
            foreach (char c in characters)
            {
                // 6.1 Измерение границ с использованием правильного API
                var measureOptions = new SixLabors.Fonts.TextOptions(sysFont)
                {
                    Dpi = 72,
                    HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Left,
                    VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Top,
                    WrappingLength = float.MaxValue
                };

                var bounds = SixLabors.Fonts.TextMeasurer.MeasureBounds(c.ToString(), measureOptions);

                int gw = (int)Math.Ceiling(bounds.Width);
                int gh = lineHeight;
                int ox = (int)Math.Floor(bounds.Left);
                int oy = (int)Math.Floor(bounds.Top);

                if (x + gw + padding > textureWidth)
                {
                    x = padding;
                    y += lineHeight + padding;
                }

                var drawOptions = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        Antialias = true
                    }
                };

                // Создаем RichTextOptions с нужными параметрами
                var richTextOptions = new RichTextOptions(sysFont)
                {
                    Dpi = 72,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    WrappingLength = float.MaxValue,
                    Origin = new Vector2(x - ox, y - oy)
                };

                // Создаем кисть из цвета
                var brush = new SolidBrush(SixLabors.ImageSharp.Color.White);

                img.Mutate(ctx => ctx.DrawText(
                    drawingOptions: drawOptions,
                    textOptions: richTextOptions,
                    text: c.ToString(),
                    brush: brush,
                    pen: null
                ));

                // 6.3 Расчет advance через HarfBuzz
                using var singleBuf = new HarfBuzzSharp.Buffer();
                singleBuf.AddUtf8(c.ToString());
                singleBuf.GuessSegmentProperties();
                hbFont.Shape(singleBuf, null);

                var singlePos = singleBuf.GlyphPositions;
                int rawAdvUnits = singlePos.Length > 0 ? singlePos[0].XAdvance : 0;
                int xAdvance = (int)Math.Round(rawAdvUnits * scale);

                sb.AppendLine(
                    $"char id={(int)c} x={x} y={y} width={gw} height={gh} " +
                    $"xoffset={ox} yoffset={oy} xadvance={xAdvance} page=0 chnl=15"
                );

                x += gw + padding;
            }
            // 7. Кернинг
            var kernings = new List<string>();
            foreach (char lc in characters)
            {
                foreach (char rc in characters)
                {
                    string duo = new string(new[] { lc, rc });

                    using var buf = new HarfBuzzSharp.Buffer();
                    buf.AddUtf8(duo);
                    buf.GuessSegmentProperties();
                    hbFont.Shape(buf, null);

                    var pos = buf.GlyphPositions;
                    if (pos.Length < 2) continue;

                    int kernUnits = pos[1].XOffset;
                    if (kernUnits == 0) continue;

                    int amount = (int)Math.Round(kernUnits * scale);
                    kernings.Add($"kerning first={(int)lc} second={(int)rc} amount={amount}");
                }
            }

            if (kernings.Count > 0)
            {
                sb.AppendLine($"kernings count={kernings.Count}");
                foreach (var k in kernings) sb.AppendLine(k);
            }

            Directory.CreateDirectory(outputPath);
            string basePath = Path.Combine(outputPath, GenerateFontName(font));
            File.WriteAllText($"{basePath}.fnt", sb.ToString());
            img.SaveAsDDS($"{basePath}.dds");
            img.SaveAsTGA($"{basePath}.tga");
        }

        public static HashSet<FontSignature> CollectUniqueFonts(GuiDocument parent)
        {
            if (parent == null) return null;
            HashSet<FontSignature> result = new HashSet<FontSignature>();
            foreach (var child in parent.Containers)
            {
                foreach (var text in child.Texts)
                {
                    result.Add(text.Font);
                }
                foreach (var button in child.Buttons)
                {
                    result.Add(button.Font);
                }
            }
            return result;
        }
    }
}
