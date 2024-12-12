using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class TreeLoc : Form
    {
        public TreeLoc()
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
                    GenerateLocalizationFile(textBox1.Text, textBox2.Text);
                }
            }
        }
        public static void GenerateLocalizationFile(string inputFilePath, string countryTag)
        {
            // Проверяем существование файла
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine("Файл не найден: " + inputFilePath);
                return;
            }

            // Читаем содержимое файла
            string[] lines = File.ReadAllLines(inputFilePath);

            // Регулярное выражение для поиска фокусов с заданным тегом
            string pattern = $@"\b{countryTag}_[a-zA-Z0-9_]+";
            Regex regex = new Regex(pattern);

            // Строка для сборки файла локализации
            StringBuilder localizationBuilder = new StringBuilder();
            localizationBuilder.AppendLine("l_russian:");

            foreach (string line in lines)
            {
                Match match = regex.Match(line);
                if (match.Success)
                {
                    string focusId = match.Value;
                    localizationBuilder.AppendLine($" {focusId}: \"\"");
                    localizationBuilder.AppendLine($" {focusId}_desc: \"\"");
                }
            }

            // Формируем путь для файла локализации
            string outputFilePath = Path.Combine(
                Path.GetDirectoryName(inputFilePath),
                $"{countryTag}_localization.yml");

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
                    GenerateLocalizationFile(textBox1.Text, textBox2.Text);
                }
            }
        }
    }
}
