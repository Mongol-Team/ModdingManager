using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class IdeaLoc : Form
    {
        public IdeaLoc()
        {
            InitializeComponent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox2.Text.Length > 3)
                {
                    MessageBox.Show("Вы еблан?", "Непрафф");
                }
                else
                {
                    GenerateIdeaLocalizationFile(textBox1.Text, textBox2.Text);
                }
            }
        }

        public static void GenerateIdeaLocalizationFile(string inputFilePath, string countryTag, string outputDirectory = null)
        {
            // Проверяем существование файла
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine("Файл не найден: " + inputFilePath);
                return;
            }

            // Читаем содержимое файла
            string[] lines = File.ReadAllLines(inputFilePath);

            // Регулярное выражение для поиска идей с заданным тегом
            string pattern = $@"\b{countryTag}_[a-zA-Z0-9_]+";
            Regex regex = new Regex(pattern);

            // Словарь для хранения найденных уникальных ключей
            HashSet<string> foundKeys = new HashSet<string>();

            // Строка для сборки файла локализации
            StringBuilder localizationBuilder = new StringBuilder();
            localizationBuilder.AppendLine("l_russian:");

            foreach (string line in lines)
            {
                Match match = regex.Match(line);
                if (match.Success)
                {
                    string ideaKey = match.Value;
                    if (!foundKeys.Contains(ideaKey))
                    {
                        foundKeys.Add(ideaKey);
                        localizationBuilder.AppendLine($" {ideaKey}: \"\"");
                        localizationBuilder.AppendLine($" {ideaKey}_desc: \"\"");
                    }
                }
            }

            // Определяем путь для файла локализации
            string outputFilePath = Path.Combine(
                outputDirectory ?? Path.GetDirectoryName(inputFilePath),
                $"{countryTag}_ideas_localization.yml");

            // Создаем выходную директорию, если она указана и не существует
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Записываем в файл с кодировкой UTF-8-BOM
            File.WriteAllText(outputFilePath, localizationBuilder.ToString(), Encoding.UTF8);

            Console.WriteLine("Файл локализации успешно создан: " + outputFilePath);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox2.Text.Length > 3)
                {
                    MessageBox.Show("Вы еблан?", "Непрафф");
                }
                else
                {
                    GenerateIdeaLocalizationFile(textBox1.Text, textBox2.Text);
                }
            }
        }
    }
}
