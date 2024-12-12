using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Processing;
namespace ModdingManager
{
    public class ModManager
    {
        public static void SaveCountryFlag(System.Drawing.Image fascismImage, System.Drawing.Image neutralityImage, System.Drawing.Image communismImage, System.Drawing.Image democraticImage, string stringPath, string countryTag)
        {
            string flagsPath = Path.Combine(stringPath, "flags");
            string mediumPath = Path.Combine(flagsPath, "medium");
            string smallPath = Path.Combine(flagsPath, "small");

            Directory.CreateDirectory(flagsPath);
            Directory.CreateDirectory(mediumPath);
            Directory.CreateDirectory(smallPath);

            // Путь для файлов
            string neutralitySrandart = Path.Combine(flagsPath, $"{countryTag}_neutrality.tga");
            string neutralityMedium = Path.Combine(mediumPath, $"{countryTag}_neutrality.tga");
            string neutralitySmall = Path.Combine(smallPath, $"{countryTag}_neutrality.tga");

            string fascismSrandart = Path.Combine(flagsPath, $"{countryTag}_fascism.tga");
            string fascismMedium = Path.Combine(mediumPath, $"{countryTag}_fascism.tga");
            string fascismSmall = Path.Combine(smallPath, $"{countryTag}_fascism.tga");


            string communismSrandart = Path.Combine(flagsPath, $"{countryTag}_communism.tga");
            string communismMedium = Path.Combine(mediumPath, $"{countryTag}_communism.tga");
            string communismSmall = Path.Combine(smallPath, $"{countryTag}_communism.tga");


            string democraticSrandart = Path.Combine(flagsPath, $"{countryTag}_democratic.tga");
            string democraticMedium = Path.Combine(mediumPath, $"{countryTag}_democratic.tga");
            string democraticSmall = Path.Combine(smallPath, $"{countryTag}_democratic.tga");
            // Конвертируем System.Drawing.Image в ImageSharp.Image
            using (SixLabors.ImageSharp.Image image = ConvertToImageSharp(neutralityImage))
            {
                using (SixLabors.ImageSharp.Image normalImage = ResizeImage(image, 82, 52))
                {
                    SaveImageAsTga(image, neutralitySrandart);
                }
                // Создаём и сохраняем уменьшенные версии
                using (SixLabors.ImageSharp.Image mediumImage = ResizeImage(image, 41, 26))
                {
                    SaveImageAsTga(mediumImage, neutralityMedium);
                }

                using (SixLabors.ImageSharp.Image smallImage = ResizeImage(image, 10, 7))
                {
                    SaveImageAsTga(smallImage, neutralitySmall);
                }
            }

            using (SixLabors.ImageSharp.Image image = ConvertToImageSharp(fascismImage))
            {
                using (SixLabors.ImageSharp.Image normalImage = ResizeImage(image, 82, 52))
                {
                    SaveImageAsTga(image, fascismSrandart);
                }
                // Создаём и сохраняем уменьшенные версии
                using (SixLabors.ImageSharp.Image mediumImage = ResizeImage(image, 41, 26))
                {
                    SaveImageAsTga(mediumImage, fascismMedium);
                }

                using (SixLabors.ImageSharp.Image smallImage = ResizeImage(image, 10, 7))
                {
                    SaveImageAsTga(smallImage, fascismSmall);
                }
            }

            using (SixLabors.ImageSharp.Image image = ConvertToImageSharp(communismImage))
            {
                using (SixLabors.ImageSharp.Image normalImage = ResizeImage(image, 82, 52))
                {
                    SaveImageAsTga(image, communismSrandart);
                }
                // Создаём и сохраняем уменьшенные версии
                using (SixLabors.ImageSharp.Image mediumImage = ResizeImage(image, 41, 26))
                {
                    SaveImageAsTga(mediumImage, communismMedium);
                }

                using (SixLabors.ImageSharp.Image smallImage = ResizeImage(image, 10, 7))
                {
                    SaveImageAsTga(smallImage, communismSmall);
                }
            }

            using (SixLabors.ImageSharp.Image image = ConvertToImageSharp(democraticImage))
            {
                using (SixLabors.ImageSharp.Image normalImage = ResizeImage(image, 82, 52))
                {
                    SaveImageAsTga(image, democraticSrandart);
                }
                // Создаём и сохраняем уменьшенные версии
                using (SixLabors.ImageSharp.Image mediumImage = ResizeImage(image, 41, 26))
                {
                    SaveImageAsTga(mediumImage, democraticMedium);
                }

                using (SixLabors.ImageSharp.Image smallImage = ResizeImage(image, 10, 7))
                {
                    SaveImageAsTga(smallImage, democraticSmall);
                }
            }
        }

        private static SixLabors.ImageSharp.Image ConvertToImageSharp(System.Drawing.Image originalImage)
        {
            // Конвертируем System.Drawing.Image в ImageSharp.Image
            using (var ms = new MemoryStream())
            {
                // Сохраняем изображение в поток (в памяти)
                originalImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                // Загружаем изображение в ImageSharp
                return SixLabors.ImageSharp.Image.Load(ms);
            }
        }

        private static void SaveImageAsTga(SixLabors.ImageSharp.Image image, string filePath)
        {
            // Сохраняем изображение в формате TGA
            image.Save(filePath, new TgaEncoder());
        }

        private static SixLabors.ImageSharp.Image ResizeImage(SixLabors.ImageSharp.Image image, int width, int height)
        {
            // Клонируем и изменяем размер изображения
            return image.Clone(context => context.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(width, height),
                Mode = ResizeMode.Stretch
            }));
        }

    }
}
