using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Processing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModdingManager.managers.utils;
using ModdingManager.classes.gfx;

namespace ModdingManager
{
    public partial class FlagCreator : Form
    {
        public FlagCreator()
        {
            InitializeComponent();
        }


        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox2.Text.Length == 3)
                {
                    ImageManager.SaveCountryFlags(panel2.BackgroundImage, panel1.BackgroundImage, panel3.BackgroundImage, panel4.BackgroundImage ,textBox1.Text, textBox2.Text);
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
                if (textBox2.Text.Length == 3)
                {
                    ImageManager.SaveCountryFlags(panel2.BackgroundImage, panel1.BackgroundImage, panel3.BackgroundImage, panel4.BackgroundImage, textBox1.Text, textBox2.Text);
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
                        Panel panel = sender as Panel;
                        // Загружаем изображение из файла
                        System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);

                        // Устанавливаем изображение как фон панели
                        panel.BackgroundImage = image;

                        // При необходимости, можно настроить параметры отображения (например, растягивание):
                        panel.BackgroundImageLayout = ImageLayout.Stretch; // Растягивание изображения на панель
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
