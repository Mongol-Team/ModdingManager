using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TeximpNet.DDS;
using TeximpNet;
using System.IO;
using ModdingManager.configs;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ModdingManager.managers;

namespace ModdingManager.classes.gfx
{
    public class DDSManager : ImageManager
    {
        public DDSManager() { }

        public static void SaveIdeaGFXAsDDS(Image image, string dir, string id, string tag)
        {
            var path = Path.Combine(dir, "gfx", "interface", "ideas", tag);
            Directory.CreateDirectory(path);
            SaveAsDDS(image, path, id, 64, 64);
        }

        public static void SaveAsDDS(Image image, string directory, string filename, int width, int height)
        {
            Directory.CreateDirectory(directory);

            using (var imageSharp = ConvertToImageSharp(image))
            using (var resized = ResizeStretch(imageSharp, width, height))
            {
                string outputPath = Path.Combine(directory, $"{filename}.dds");

                byte[] pixelData = new byte[resized.Width * resized.Height * 4];
                resized.CopyPixelDataTo(pixelData);

                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    byte r = pixelData[i];
                    byte b = pixelData[i + 2];
                    pixelData[i] = b;
                    pixelData[i + 2] = r;
                }

                using (var surface = new Surface(resized.Width, resized.Height))
                {
                    Marshal.Copy(pixelData, 0, surface.DataPtr, pixelData.Length);
                    DDSFile.Write(outputPath, surface, TextureDimension.Two, DDSFlags.None);
                }
            }
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
                        SaveAsDDS(bmp, techIconDir, item.Id, 64, 64);
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
                SaveAsDDS(bmp, techIconDir, $"techtree_{window.CurrentTechTree.Name}_tab", 182, 55);
            }
        }
    }
}
