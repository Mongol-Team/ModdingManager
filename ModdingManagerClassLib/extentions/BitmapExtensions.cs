using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Pfim;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TeximpNet;
using TeximpNet.DDS;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ModdingManagerClassLib.Extentions
{
    public static class BitmapExtensions
    {
        public static void SaveAsDDS(this Bitmap image, string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directory);

            using (var resized = new Bitmap(image, image.Width, image.Height))
            {
                var pixelData = new byte[resized.Width * resized.Height * 4];

                int index = 0;
                for (int y = 0; y < resized.Height; y++)
                {
                    for (int x = 0; x < resized.Width; x++)
                    {
                        System.Drawing.Color pixel = resized.GetPixel(x, y);
                        pixelData[index++] = pixel.B; // R <- B
                        pixelData[index++] = pixel.G;
                        pixelData[index++] = pixel.R; // B <- R
                        pixelData[index++] = pixel.A;
                    }
                }

                using (var surface = new Surface(resized.Width, resized.Height))
                {
                    Marshal.Copy(pixelData, 0, surface.DataPtr, pixelData.Length);
                    DDSFile.Write(fullPath, surface, TextureDimension.Two, DDSFlags.None);
                }
            }
        }
        public static Bitmap LoadFromTGA(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            // Загружаем изображение в ImageSharp как Rgba32
            using Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

            int width = image.Width;
            int height = image.Height;
            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Подготовим буфер и скопируем пиксели из ImageSharp
            int pixelCount = width * height;
            int bytesCount = pixelCount * 4;
            byte[] src = new byte[bytesCount]; // в ImageSharp порядок R, G, B, A
            image.CopyPixelDataTo(src);

            // Преобразуем порядок в B, G, R, A для Format32bppArgb
            byte[] dst = new byte[bytesCount];
            for (int s = 0, d = 0; s < bytesCount; s += 4, d += 4)
            {
                // src: [R, G, B, A]
                // dst: [B, G, R, A]
                dst[d + 0] = src[s + 2]; // B
                dst[d + 1] = src[s + 1]; // G
                dst[d + 2] = src[s + 0]; // R
                dst[d + 3] = src[s + 3]; // A
            }

            // Копируем байты в Bitmap через LockBits
            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            try
            {
                // Если stride == width*4 — прямое копирование,
                // но stride может быть выровнен, поэтому учитываем его.
                int stride = Math.Abs(bmpData.Stride);
                if (stride == width * 4)
                {
                    // Прямое копирование всего буфера
                    Marshal.Copy(dst, 0, bmpData.Scan0, bytesCount);
                }
                else
                {
                    IntPtr scan = bmpData.Scan0;
                    int srcOffset = 0;
                    for (int y = 0; y < height; y++)
                    {
                        IntPtr rowPtr = IntPtr.Add(scan, y * bmpData.Stride);
                        Marshal.Copy(dst, srcOffset, rowPtr, width * 4);
                        srcOffset += width * 4;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return bmp;
        }
        public static void SaveAsTGA(this Bitmap image, string path)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            int width = image.Width;
            int height = image.Height;

            using var img = new Image<Rgba32>(width, height);

            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            var bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

            try
            {
                int stride = bmpData.Stride;
                int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;

                if (bytesPerPixel < 1)
                    throw new NotSupportedException($"Unsupported pixel format: {image.PixelFormat}");

                byte[] buffer = new byte[stride * height];
                Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);

                for (int y = 0; y < height; y++)
                {
                    int offset = y * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int i = offset + x * bytesPerPixel;

                        byte r, g, b, a;

                        switch (image.PixelFormat)
                        {
                            case PixelFormat.Format32bppArgb:
                            case PixelFormat.Format32bppPArgb:
                                b = buffer[i + 0];
                                g = buffer[i + 1];
                                r = buffer[i + 2];
                                a = buffer[i + 3];
                                break;

                            case PixelFormat.Format32bppRgb:
                                b = buffer[i + 0];
                                g = buffer[i + 1];
                                r = buffer[i + 2];
                                a = 255;
                                break;

                            case PixelFormat.Format24bppRgb:
                                b = buffer[i + 0];
                                g = buffer[i + 1];
                                r = buffer[i + 2];
                                a = 255;
                                break;

                            case PixelFormat.Format8bppIndexed:
                                var c = image.Palette.Entries[buffer[i]];
                                r = c.R;
                                g = c.G;
                                b = c.B;
                                a = c.A;
                                break;

                            default:
                                throw new NotSupportedException($"Unsupported pixel format: {image.PixelFormat}");
                        }

                        img[x, y] = new Rgba32(r, g, b, a);
                    }
                }
            }
            finally
            {
                image.UnlockBits(bmpData);
            }

            var encoder = new TgaEncoder
            {
                BitsPerPixel = TgaBitsPerPixel.Pixel32,
                Compression = TgaCompression.None
            };

            img.Save(path, encoder);
        }
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // важно для освобождения потока
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // делает потокобезопасным

                return bitmapImage;
            }
        }

        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            var hBitmap = bitmap.GetHbitmap(); // Получаем дескриптор HBitmap

            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                bitmapSource.Freeze(); // Делаем потокобезопасным
                return bitmapSource;
            }
            finally
            {
                // Освобождаем ресурсы GDI
                NativeMethods.DeleteObject(hBitmap);
            }
        }
        public static Mat ToMat(this Bitmap bitmap)
        {
            return BitmapConverter.ToMat(bitmap);
        }
        public static System.Drawing.Image ToDrawingImage(this BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                return null;

            using (var outStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder(); // <-- используем PNG, не BMP
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(outStream);
                outStream.Position = 0;

                return System.Drawing.Image.FromStream(outStream, true, true);
            }
        }
        public static Bitmap? LoadResourceRealativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Относительный путь не может быть пустым.", nameof(relativePath));
            relativePath = relativePath.Replace("/", "\\");
            // Комбинируем с директориями
            string modPath = Path.Combine(ModManager.ModDirectory, relativePath);
            string gamePath = Path.Combine(ModManager.GameDirectory, relativePath);

            string? fullPath = null;

            if (File.Exists(modPath))
                fullPath = modPath;
            else if (File.Exists(gamePath))
                fullPath = gamePath;

            if (fullPath == null)
                return null;

            string ext = Path.GetExtension(fullPath).ToLowerInvariant();

            return ext switch
            {
                ".dds" => LoadFromDDS(fullPath),
                ".tga" => LoadFromTGA(fullPath),
                ".png" => LoadFromPNG(fullPath),
                _ => throw new NotSupportedException($"Файлы с расширением {ext} не поддерживаются.")
            };
        }

        public static Bitmap LoadResourceFullPath(string pathWithFileName)
        {
            if (string.IsNullOrWhiteSpace(pathWithFileName))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(pathWithFileName));
            pathWithFileName = pathWithFileName.Replace("/", "\\");
            if (!File.Exists(pathWithFileName))
                throw new FileNotFoundException("Файл не найден.", pathWithFileName);

            string ext = Path.GetExtension(pathWithFileName).ToLowerInvariant();

            switch (ext)
            {
                case ".dds":
                    return LoadFromDDS(pathWithFileName);

                case ".tga":
                    return LoadFromTGA(pathWithFileName);
                case ".png":
                    return LoadFromPNG(pathWithFileName);
                default:
                    throw new NotSupportedException($"Файлы с расширением {ext} не поддерживаются.");
            }
        }
        public static Bitmap LoadFromPNG(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден.", path);

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return new Bitmap(stream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не удалось загрузить PNG-файл: {path}", ex);
            }
        }

        public static Bitmap LoadFromDDS(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Logger.AddDbgLog($"Файл для загрузки DDS картинки не найден: {path}");
                return null;
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    Logger.AddDbgLog($"Начало загрузки DDS: {path}");

                    // Парсим DDS header для детальной информации о формате
                    stream.Position = 0;
                    var reader = new BinaryReader(stream);
                    string magic = new string(reader.ReadChars(4));
                    if (magic != "DDS ")
                    {
                        Logger.AddDbgLog($"Неверная магия DDS: {magic} для {path}. Fallback to PNG.");
                        return LoadFromPNG(path);
                    }

                    uint headerSize = reader.ReadUInt32();
                    if (headerSize != 124)
                    {
                        Logger.AddDbgLog($"Неверный размер заголовка DDS: {headerSize} для {path}");
                        return null;
                    }

                    uint flags = reader.ReadUInt32();
                    int height = (int)reader.ReadUInt32();
                    int width = (int)reader.ReadUInt32();
                    int pitch = (int)reader.ReadUInt32();
                    uint depth = reader.ReadUInt32();
                    uint mipMapCount = reader.ReadUInt32();
                    reader.ReadBytes(44); // reserved1

                    uint pfSize = reader.ReadUInt32();
                    if (pfSize != 32)
                    {
                        Logger.AddDbgLog($"Неверный размер pixel format: {pfSize} для {path}");
                        return null;
                    }

                    uint pfFlags = reader.ReadUInt32();
                    uint fourCC = reader.ReadUInt32();
                    uint bitCount = reader.ReadUInt32();
                    uint rMask = reader.ReadUInt32();
                    uint gMask = reader.ReadUInt32();
                    uint bMask = reader.ReadUInt32();
                    uint aMask = reader.ReadUInt32();

                    // Логируем детальную информацию о заголовке
                    Logger.AddDbgLog($"DDS header для {path}: FourCC={fourCC}, BitsPerPixel={bitCount}, RedMask={rMask:X}, GreenMask={gMask:X}, BlueMask={bMask:X}, AlphaMask={aMask:X}, Pitch={pitch}, Width={width}, Height={height}, MipMaps={mipMapCount}");

                    byte[] raw;
                    Pfim.ImageFormat pfimFormat = Pfim.ImageFormat.Rgba32; // По умолчанию для fallback
                    int stride;

                    if (fourCC == 63)
                    {
                        // Специальная обработка для FourCC 63 (Q8W8V8U8 signed)
                        Logger.AddDbgLog($"Обработка FourCC 63 (Q8W8V8U8) для {path}");

                        reader.ReadBytes(20); // caps1, caps2, caps3, caps4, reserved2

                        if (bitCount != 32 || (pfFlags & 0x4) == 0)
                        {
                            Logger.AddDbgLog($"Неподдерживаемый формат в FourCC 63: Bits={bitCount}, Flags={pfFlags & 0x4} для {path}");
                            return null;
                        }

                        // Проверяем стандартные маски для A8R8G8B8
                        if (rMask != 0x00FF0000 || gMask != 0x0000FF00 || bMask != 0x000000FF || aMask != 0xFF000000)
                        {
                            Logger.AddDbgLog($"Неожиданные битовые маски в DDS: {path}");
                            return null;
                        }

                        if (pitch == 0)
                        {
                            pitch = width * 4;
                        }

                        raw = new byte[pitch * height];
                        int bytesRead = stream.Read(raw, 0, raw.Length);
                        if (bytesRead != raw.Length)
                        {
                            Logger.AddDbgLog($"Неполное чтение данных в DDS: {path}");
                            return null;
                        }

                        stride = pitch;

                        // Remap signed (-128..127) to unsigned (0..255)
                        byte[] remapped = new byte[raw.Length];
                        for (int i = 0; i < raw.Length; i++)
                        {
                            remapped[i] = (byte)((sbyte)raw[i] + 128);
                        }
                        // Принудительно устанавливаем альфа-канал = 255 (opaque)
                        for (int i = 3; i < remapped.Length; i += 4)
                        {
                            remapped[i] = 255;
                        }
                        raw = remapped;
                        Logger.AddDbgLog($"Remapped signed data for {path}");
                    }
                    else
                    {
                        // Сбрасываем позицию потока для Pfim
                        stream.Position = 0;

                        using Pfim.IImage dds = Pfim.Dds.Create(stream, new PfimConfig());
                        if (dds.Width != width || dds.Height != height)
                        {
                            Logger.AddDbgLog($"Несоответствие размеров в Pfim и header: Pfim={dds.Width}x{dds.Height}, Header={width}x{height} для {path}");
                        }

                        raw = dds.Data;
                        pfimFormat = dds.Format;
                        stride = dds.Stride;

                        // Логируем формат после декомпрессии Pfim
                        Logger.AddDbgLog($"Формат после Pfim: {dds.Format}, DataLength={raw.Length}, Stride={dds.Stride} для {path}");
                    }

                    // Определяем формат после декомпрессии или fallback
                    PixelFormat sysFmt = pfimFormat switch
                    {
                        Pfim.ImageFormat.Rgba32 => PixelFormat.Format32bppArgb,
                        Pfim.ImageFormat.Rgb24 => PixelFormat.Format24bppRgb,
                        Pfim.ImageFormat.R5g5b5 => PixelFormat.Format16bppRgb555,
                        Pfim.ImageFormat.R5g6b5 => PixelFormat.Format16bppRgb565,
                        Pfim.ImageFormat.Rgba16 => PixelFormat.Format16bppArgb1555,
                        _ => throw new NotSupportedException($"DDS формат {pfimFormat} не поддерживается")
                    };

                    // Определяем описание формата
                    string formatDescription = "Неизвестный";
                    if ((pfFlags & 0x4) != 0) // DDPF_FOURCC
                    {
                        formatDescription = fourCC switch
                        {
                            0x31545844 => "DXT1 (BC1)",
                            0x33545844 => "DXT3 (BC2)",
                            0x35545844 => "DXT5 (BC3)",
                            0x20433142 => "BC1",
                            0x20433242 => "BC2",
                            0x20433342 => "BC3",
                            0x20433442 => "BC4",
                            0x20433542 => "BC5",
                            0x20433642 => "BC6H",
                            0x20433742 => "BC7",
                            63 => "Q8W8V8U8 (signed quaternion)",
                            _ => $"Неизвестный FourCC {fourCC}"
                        };
                    }
                    else
                    {
                        // Uncompressed форматы на основе масок
                        if (bitCount == 32 && rMask == 0x00FF0000 && gMask == 0x0000FF00 && bMask == 0x000000FF && aMask == 0xFF000000)
                        {
                            formatDescription = "A8R8G8B8 (uncompressed)";
                        }
                    }

                    Logger.AddDbgLog($"Определённый формат DDS: {formatDescription} для {path}");

                    // Проверяем на normal map
                    bool isNormalMap = false;
                    bool forceOpaque = formatDescription.Contains("normal map") || formatDescription.Contains("Q8W8V8U8");
                    if (formatDescription.Contains("DXT5") || formatDescription.Contains("BC3") || formatDescription.Contains("Q8W8V8U8"))
                    {
                        string fileName = Path.GetFileName(path).ToLowerInvariant();
                        if (fileName.Contains("_n") || fileName.Contains("normal") || fileName.Contains("bump"))
                        {
                            isNormalMap = true;
                            formatDescription += " (normal map by filename)";
                        }
                        else if (IsLikelyNormalMap(raw, width, height))
                        {
                            isNormalMap = true;
                            formatDescription += " (normal map by data analysis)";
                        }
                    }

                    byte[] dataToCopy = raw;
                    if (isNormalMap)
                    {
                        // Для normal maps в BC3/DXT5: часто X в alpha (R), Y в green (G), reconstruct blue (Z)
                        Logger.AddDbgLog($"Обработка normal map: reconstruct Z для {path}");
                        byte[] remapped = new byte[raw.Length];
                        for (int i = 0; i < raw.Length; i += 4)
                        {
                            byte a = raw[i + 3]; // X (red in normal)
                            byte g = raw[i + 1]; // Y (green)
                                                 // Z = sqrt(1 - X^2 - Y^2), нормализовать 0-255 в -1..1
                            float x = (a / 255f * 2f) - 1f;
                            float y = (g / 255f * 2f) - 1f;
                            float z = (float)Math.Sqrt(Math.Max(0f, 1f - (x * x + y * y)));
                            byte b = (byte)((z + 1f) / 2f * 255f);

                            remapped[i + 0] = b; // B = Z (blue)
                            remapped[i + 1] = g; // G = Y
                            remapped[i + 2] = a; // R = X
                            remapped[i + 3] = 255; // A = 255 (opaque)
                        }
                        dataToCopy = remapped;
                    }
                    else if (forceOpaque)
                    {
                        Logger.AddDbgLog($"Принудительная установка альфы в 255 для opaque в {path}");
                        byte[] remapped = new byte[raw.Length];
                        Array.Copy(raw, remapped, raw.Length);
                        for (int i = 3; i < remapped.Length; i += 4)
                        {
                            remapped[i] = 255;
                        }
                        dataToCopy = remapped;
                    }

                    var bmp = new Bitmap(width, height, sysFmt);
                    var rect = new System.Drawing.Rectangle(0, 0, width, height);
                    var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, sysFmt);

                    try
                    {
                        int bytesPerPixel = pfimFormat switch
                        {
                            Pfim.ImageFormat.Rgba32 => 4,
                            Pfim.ImageFormat.Rgb24 => 3,
                            _ => 2 // для 16-битных форматов
                        };

                        int rowBytes = width * bytesPerPixel;

                        // Логируем перед копированием
                        Logger.AddDbgLog($"Копирование данных: BytesPerPixel={bytesPerPixel}, RowBytes={rowBytes}, Stride={stride}, BMP Stride={bmpData.Stride} для {path}");

                        for (int y = 0; y < height; y++)
                        {
                            Marshal.Copy(dataToCopy, y * stride, bmpData.Scan0 + y * bmpData.Stride, rowBytes);
                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(bmpData);
                    }

                    bmp.SetResolution(96, 96);  // DPI по умолчанию для WPF

                    // Общее логирование успешной загрузки
                    Logger.AddDbgLog($"Загружен DDS {path} ({width}x{height}, формат: {formatDescription})");

                    return bmp;
                }
            }
            catch (Exception ex)
            {
                Logger.AddDbgLog($"Ошибка загрузки DDS: {path}. Ошибка: {ex.Message}, StackTrace: {ex.StackTrace}. Fallback to PNG.");
                return LoadFromPNG(path);
            }
        }

        private static bool IsLikelyNormalMap(byte[] raw, int width, int height)
        {
            if (raw.Length < 4 * width * height) return false;
            byte sampleR = raw[2];  // Первый пиксель R
            byte sampleB = raw[0];  // B
            bool rConstant = true, bConstant = true;
            for (int i = 0; i < raw.Length; i += 4)
            {
                if (raw[i + 2] != sampleR) rConstant = false;
                if (raw[i + 0] != sampleB) bConstant = false;
                if (!rConstant && !bConstant) break;
            }
            Logger.AddDbgLog($"Normal map check: R constant={rConstant}, B constant={bConstant}");
            return rConstant && bConstant;  // Если R и B константны — вероятно normal (AG format)
        }
        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
        }
    }
}
