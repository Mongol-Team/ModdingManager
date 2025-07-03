using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TeximpNet.DDS;
using TeximpNet;
using Pfim;
using System.IO;
using ModdingManager.configs;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ModdingManager.classes.extentions;
using ModdingManager.managers.utils;
namespace ModdingManager.classes.gfx
{
    public class DDSManager : ImageManager
    {
        public DDSManager() { }

        public static void SaveIdeaGFXAsDDS(System.Drawing.Image image, string dir, string id, string tag)
        {
            var path = Path.Combine(dir, "gfx", "interface", "ideas", tag);
            Directory.CreateDirectory(path);
            image.SaveAsDDS(path, id, 64, 64);
        }

        

        public static void SaveAllTechIconsAsDDS(TechTreeConfig techTree)
        {
            Bitmap ConvertImageSourceToBitmap(ImageSource imageSource)
            {
                if (imageSource is BitmapSource bitmapSource)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(memoryStream);

                        using (var tempBitmap = new Bitmap(memoryStream))
                        {
                            return new Bitmap(tempBitmap);
                        }
                    }
                }

                throw new ArgumentException("Невозможно преобразовать ImageSource в Bitmap — неподдерживаемый формат.");
            }
            string techIconDir = Path.Combine(ModManager.Directory, "gfx", "interface", "technologies");
            Directory.CreateDirectory(techIconDir);

            foreach (var item in techTree.Items)
            {
                if (item.Image == null || string.IsNullOrWhiteSpace(item.Id))
                    continue;

                try
                {
                    using (var bmp = ConvertImageSourceToBitmap(item.Image))
                    {
                        bmp.SaveAsDDS(techIconDir, item.Id, 64, 64);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при сохранении иконки технологии {item.Id}: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }

            System.Windows.MessageBox.Show("Сохранение иконок технологий завершено!", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public static void SaveFolderTabIcon(TechTreeCreator window)
        {

            Bitmap ConvertImageSourceToBitmap(ImageSource imageSource)
            {
                if (imageSource is BitmapSource bitmapSource)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(memoryStream);

                        using (var tempBitmap = new Bitmap(memoryStream))
                        {
                            return new Bitmap(tempBitmap);
                        }
                    }
                }

                throw new ArgumentException("Невозможно преобразовать ImageSource в Bitmap — неподдерживаемый формат.");
            }
            string techIconDir = Path.Combine(ModManager.Directory, "gfx", "interface", "techtree");
            Directory.CreateDirectory(techIconDir);
            var firstCopy = window.TabFolderFirstImage.Source.Clone();
            var secondCopy = window.TabFolderFirstImage.Source.Clone();
            var bgCopy = window.TabFolderBackgroundImage.Source.Clone();
            var shadowingCopy = window.TabFolderSecondImage.Source.Clone();

            var temp2 = GetCombinedTechImage(shadowingCopy, bgCopy, 3);
            var img = GetCombinedTechImage(temp2, shadowingCopy, 3);


            using (var bmp = ConvertImageSourceToBitmap(temp2))
            {
                bmp.SaveAsDDS(techIconDir, $"techtree_{window.CurrentTechTree.Name}_tab", 182, 55);
            }
        }

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
                    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    return new Bitmap(image.Width, image.Height, image.Stride, format, ptr);
                }
                finally
                {
                    handle.Free();
                }
            }
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
