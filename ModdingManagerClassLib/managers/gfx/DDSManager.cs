using ModdingManagerClassLib.Extentions;
using ModdingManager.managers.@base;
using ModdingManagerModels;
using Pfim;
using System.Drawing;
using System.Runtime.InteropServices;
namespace ModdingManager.classes.managers.gfx
{
    public static class DDSManager
    {

        public static Bitmap LoadDDSAsBitmap(string path)
        {
            using (var image = Pfimage.FromFile(path))
            {
                System.Drawing.Imaging.PixelFormat format = image.Format switch
                {
                    Pfim.ImageFormat.Rgba32 => System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                    Pfim.ImageFormat.Rgb24 => System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                    Pfim.ImageFormat.R5g5b5 => System.Drawing.Imaging.PixelFormat.Format16bppRgb555,
                    Pfim.ImageFormat.R5g6b5 => System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                    Pfim.ImageFormat.Rgba16 => System.Drawing.Imaging.PixelFormat.Format64bppArgb,
                    _ => throw new NotSupportedException($"Формат изображения {image.Format} не поддерживается.")
                };

                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    return new Bitmap(image.Width, image.Height, image.Stride, format, ptr);
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        public static void SaveAllTechIconsAsDDS(TechTreeConfig treeConfig)
        {

            string techIconDir = Path.Combine(ModManager.ModDirectory, "gfx", "interface", "technologies");
            Directory.CreateDirectory(techIconDir);

            foreach (var item in treeConfig.Items)
            {
                if (item.Image == null || item.Id == null)
                    continue;

                try
                {
                    using (var bmp = item.Image)
                    {
                        bmp.SaveAsDDS(techIconDir, item.Id.AsString(), 64, 64);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при сохранении иконки технологии {item.Id}: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }

            System.Windows.MessageBox.Show("Сохранение иконок технологий завершено!", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        public static List<Bitmap> LoadAllDDSFromDirectory(string directory)
        {
            var bitmaps = new List<Bitmap>();
            if (!Directory.Exists(directory))
                return bitmaps;

            var files = Directory.GetFiles(directory, "*.dds", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                try
                {
                    bitmaps.Add(LoadDDSAsBitmap(file));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке {file}: {ex.Message}");
                }
            }

            return bitmaps;
        }

    }
}
