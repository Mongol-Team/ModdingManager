using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ModdingManager
{
    public partial class CountryCreator : Form
    {
        public CountryCreator()
        {
            InitializeComponent();
        }

        public static void SaveCountryFlagVariants(System.Drawing.Image originalImage, string stringPath, string countryTag)
        {
            // Создаем директорий
            string flagsPath = Path.Combine(stringPath, "flags");
            string mediumPath = Path.Combine(flagsPath, "medium");
            string smallPath = Path.Combine(flagsPath, "small");

            Directory.CreateDirectory(flagsPath);
            Directory.CreateDirectory(mediumPath);
            Directory.CreateDirectory(smallPath);

            // Путь для файлов
            string originalFilePath = Path.Combine(flagsPath, $"{countryTag}_neutrality.tga");
            string mediumFilePath = Path.Combine(mediumPath, $"{countryTag}_neutrality.tga");
            string smallFilePath = Path.Combine(smallPath, $"{countryTag}_neutrality.tga");

            // Конвертируем System.Drawing.Image в ImageSharp.Image
            using (SixLabors.ImageSharp.Image image = ConvertToImageSharp(originalImage))
            {
                // Сохраняем оригинальное изображение
                SaveImageAsTga(image, originalFilePath);

                // Создаём и сохраняем уменьшенные версии
                using (SixLabors.ImageSharp.Image mediumImage = ResizeImage(image, 41, 26))
                {
                    SaveImageAsTga(mediumImage, mediumFilePath);
                }

                using (SixLabors.ImageSharp.Image smallImage = ResizeImage(image, 10, 7))
                {
                    SaveImageAsTga(smallImage, smallFilePath);
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
            //// Сохраняем изображение в формате TGA
            //image.Save(filePath, new TgaEncoder());
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

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (StabBox.Text.Length == 3)
                {
                    SaveCountryFlagVariants(CountryNeutralFlagPanel.BackgroundImage, TagBox.Text, StabBox.Text);
                }
                else
                {
                    MessageBox.Show("Ты пьянь?", "Вопрос");
                }
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (StabBox.Text.Length == 3)
                {
                    SaveCountryFlagVariants(CountryNeutralFlagPanel.BackgroundImage, TagBox.Text, StabBox.Text);
                }
                else
                {
                    MessageBox.Show("Ты пьянь?", "Вопрос");
                }
            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                // Получаем путь к первому файлу
                string filePath = files[0];

                // Проверяем, является ли файл изображением (.jpg или .png)
                if (Path.GetExtension(filePath).ToLower() == ".jpg" || Path.GetExtension(filePath).ToLower() == ".png")
                {
                    try
                    {
                        // Загружаем изображение из файла
                        System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);

                        // Устанавливаем изображение как фон панели
                        CountryNeutralFlagPanel.BackgroundImage = image;

                        // При необходимости, можно настроить параметры отображения (например, растягивание):
                        CountryNeutralFlagPanel.BackgroundImageLayout = ImageLayout.Stretch; // Растягивание изображения на панель
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, перетащите изображение в формате JPG или PNG.");
                }
            }
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Получаем массив перетаскиваемых файлов
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Проверяем, что хотя бы один файл имеет расширение .jpg или .png
                if (files.Length > 0 && (Path.GetExtension(files[0]).ToLower() == ".jpg" || Path.GetExtension(files[0]).ToLower() == ".png"))
                {
                    e.Effect = DragDropEffects.Copy; // Разрешаем копирование
                }
                else
                {
                    e.Effect = DragDropEffects.None; // Отклоняем перетаскивание, если файл не jpg или png
                }
            }
            else
            {
                e.Effect = DragDropEffects.None; // Если перетаскиваются не файлы
            }
        }
    }
}
