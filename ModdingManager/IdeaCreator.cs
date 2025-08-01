using ModdingManager.classes.extentions;
using ModdingManager.classes.managers.gfx;
using ModdingManager.configs;
using ModdingManager.managers.@base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class IdeaCreator : Form
    {
        public IdeaCreator()
        {
            InitializeComponent();
        }

        public void CreateCountryIdea()
        {
            try
            {
                string countryTag = TagBox.Text;
                // Формируем путь к файлу в директории мода
                string ideasPath = Path.Combine(ModManager.Directory, "common", "ideas", $"{countryTag}.txt");
                string directoryPath = Path.GetDirectoryName(ideasPath);

                // Создаем директорию если не существует
                Directory.CreateDirectory(directoryPath);

                // Проверяем обязательные поля
                if (string.IsNullOrWhiteSpace(IdBox.Text))
                {
                    MessageBox.Show("ID идеи обязателен для заполнения!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Формируем содержимое новой идеи
                StringBuilder ideaBuilder = new StringBuilder();
                ideaBuilder.AppendLine($"\t{IdBox.Text.Trim()} = {{");
                ideaBuilder.AppendLine($"\t\tname = {IdBox.Text.Trim()}");  // Используем ID для name
                ideaBuilder.AppendLine($"\t\tpicture = {IdBox.Text.Trim()}"); // Используем ID для picture

                // Блок allowed (только строки с :)
                AppendFilteredBlockContent(ideaBuilder, "allowed", AvaibleBox);

                // Блок allowed_civil_war (только строки с :)
                AppendFilteredBlockContent(ideaBuilder, "allowed_civil_war", AvaibleCivBox);

                // Removal cost (с проверкой числового значения)
                ideaBuilder.AppendLine($"\t\tremoval_cost = {(int.TryParse(RemovalCostBox.Text, out int cost) ? cost : -1)}");

                // Блок on_add (только строки с :)
                AppendFilteredBlockContent(ideaBuilder, "on_add", OnAddBox);

                // Блок modifier (только строки с :)
                ideaBuilder.AppendLine("\t\tmodifier = {");
                foreach (string line in ModifBox.Lines.Where(l => !string.IsNullOrWhiteSpace(l) && l.Contains(':') && l.Split(':').Length > 1 && !string.IsNullOrWhiteSpace(l.Split(':')[1])))
                {
                    string processedLine = ProcessModifierLine(line);
                    ideaBuilder.AppendLine($"\t\t\t{processedLine}");
                }
                ideaBuilder.AppendLine("\t\t}");

                ideaBuilder.AppendLine("\t}");

                // Обрабатываем существующий файл или создаем новый
                ProcessIdeasFile(ideasPath, ideaBuilder.ToString());

                MessageBox.Show($"Идея успешно сохранена в:\n{ideasPath}", "Успех",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении идеи:\n{ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AppendFilteredBlockContent(StringBuilder builder, string blockName, RichTextBox textBox)
        {
            builder.AppendLine($"\t\t{blockName} = {{");
            foreach (string line in textBox.Lines.Where(l => !string.IsNullOrWhiteSpace(l) && l.Contains(':') && l.Split(':').Length > 1 && !string.IsNullOrWhiteSpace(l.Split(':')[1])))
            {
                builder.AppendLine($"\t\t\t{ProcessModifierLine(line)}");
            }
            builder.AppendLine("\t\t}");
        }

        private string ProcessModifierLine(string line)
        {
            var parts = line.Split(':');
            if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                return $"{parts[0].Trim()} = {parts[1].Trim()}";
            }
            return line.Trim();
        }

        private void ProcessIdeasFile(string filePath, string newIdeaContent)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                if (!content.Contains("ideas = {"))
                {
                    content = $"ideas = {{\n\tcountry = {{\n{newIdeaContent}\n\t}}\n}}";
                }
                else if (content.Contains("country = {"))
                {
                    int lastCountryBraceIndex = content.LastIndexOf("\t}", StringComparison.Ordinal);
                    if (lastCountryBraceIndex >= 0)
                    {
                        content = content.Insert(lastCountryBraceIndex, $"\n\t{newIdeaContent}");
                    }
                }
                File.WriteAllText(filePath, content);
            }
            else
            {
                File.WriteAllText(filePath, $"ideas = {{\n\tcountry = {{\n{newIdeaContent}\n\t}}\n}}");
            }
        }
        public static void GenerateOrUpdateIdeaGFX(string ideaId, string tag)
        {

            string gfxFilePath = Path.Combine(ModManager.Directory, "interface", $"{tag}.gfx");

            string defaultHeader = "spriteTypes = {\n";
            string defaultFooter = "}\n";

            string newEntry = $"\tSpriteType = {{\n" +
                             $"\t\tname = \"GFX_idea_{ideaId}\"\n" +
                             $"\t\ttexturefile = \"gfx/interface/ideas/{tag}/{ideaId}.dds\"\n" +
                             $"\t}}\n";

            if (!File.Exists(gfxFilePath))
            {
                File.WriteAllText(gfxFilePath, defaultHeader + newEntry + defaultFooter);
                return;
            }

            string currentContent = File.ReadAllText(gfxFilePath);

            if (currentContent.Contains($"GFX_idea_{ideaId}"))
            {
                MessageBox.Show($"Ентри '{ideaId}' уже есть в файле!", "ШВАЙНЕ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int lastBraceIndex = currentContent.LastIndexOf('}');
            if (lastBraceIndex == -1)
            {
                File.WriteAllText(gfxFilePath, defaultHeader + newEntry + defaultFooter);
                return;
            }

            string updatedContent = currentContent.Substring(0, lastBraceIndex) +
                                   newEntry +
                                   currentContent.Substring(lastBraceIndex);

            File.WriteAllText(gfxFilePath, updatedContent);
            MessageBox.Show("Файл успешно создан", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void GenerateLocalizationFiles(string tag, string ideaId, string name, string description)
        {
            string englishFilePath = Path.Combine(ModManager.Directory, "localisation", "english", $"{tag}_ideas_l_english.yml");
            string russianFilePath = Path.Combine(ModManager.Directory, "localisation", "russian", $"{tag}_ideas_l_russian.yml");

            // Форматируем новые строки для добавления
            string englishEntry = $"{ideaId}:0 \"\"\n{ideaId}_desc:0 \"\"\n";
            string russianEntry = $"{ideaId}:0 \"{name}\"\n{ideaId}_desc:0 \"{description}\"\n";

            // Обрабатываем английский файл
            UpdateLocalizationFile(englishFilePath, "l_english", englishEntry, ideaId);

            // Обрабатываем русский файл
            UpdateLocalizationFile(russianFilePath, "l_russian", russianEntry, ideaId);
        }

        private static void UpdateLocalizationFile(string filePath, string languageHeader, string newEntry, string ideaId)
        {
            // Создаём директорию, если её нет
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            StringBuilder content = new StringBuilder();

            // Если файл существует, читаем его и проверяем дубли
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                // Проверяем, есть ли уже такой ID в файле
                bool entryExists = lines.Any(line => line.TrimStart().StartsWith($"{ideaId}:"));
                if (entryExists)
                {
                    Console.WriteLine($"Локализация для '{ideaId}' уже существует в {filePath}!");
                    return;
                }

                // Добавляем все старые строки (кроме закрывающих пробелов/пустых строк)
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        content.AppendLine(line.TrimEnd());
                }

                // Добавляем новый ентри
                content.AppendLine(newEntry.TrimEnd());
            }
            else
            {
                // Если файла нет, создаём новый с языковым заголовком и сразу добавляем ентри
                content.AppendLine($"{languageHeader}:");
                content.AppendLine(newEntry.TrimEnd());
            }

            // Сохраняем файл в UTF-8-BOM
            WriteFileWithBOM(filePath, content.ToString());
        }
        private static void WriteFileWithBOM(string filePath, string content)
        {
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
            {
                writer.Write(content);
            }
        }
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (!(IdBox.Text.Contains(" ")))
            { 
                CreateCountryIdea();
                GenerateOrUpdateIdeaGFX(IdBox.Text, TagBox.Text);
                GenerateLocalizationFiles(TagBox.Text, IdBox.Text, NameBox.Text, DescBox.Text);
                if (ImagePanel.BackgroundImage != null)
                {
                    static void SaveIdeaGFXAsDDS(System.Drawing.Image image, string dir, string id, string tag)
                    {
                        var path = Path.Combine(dir, "gfx", "interface", "ideas", tag);
                        Directory.CreateDirectory(path);
                        image.SaveAsDDS(path, id, 64, 64);
                    }
                    SaveIdeaGFXAsDDS(ImagePanel.BackgroundImage, ModManager.Directory, IdBox.Text, TagBox.Text);
                }
                else
                {
                    MessageBox.Show("Картинку добавь алкаш", "алкаш", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Пробела в айди не должно быть", "алкаш", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImagePanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                string filePath = files[0];

                if (Path.GetExtension(filePath).ToLower() == ".jpg" || Path.GetExtension(filePath).ToLower() == ".png")
                {
                    try
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);

                        ImagePanel.BackgroundImage = image;

                        ImagePanel.BackgroundImageLayout = ImageLayout.Stretch;
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

        private void ImagePanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0 && (Path.GetExtension(files[0]).ToLower() == ".jpg" || Path.GetExtension(files[0]).ToLower() == ".png"))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void SaveConfigButton_Click(object sender, EventArgs e)
        {
            WPFConfigManager.SaveConfigWrapper(this);
        }

        private void ConfigLoadButton_Click(object sender, EventArgs e)
        {
            WPFConfigManager.LoadConfigWrapper(this);
        }

        private void AddIdeaButtn_Click(object sender, EventArgs e)
        {
            AddIdeaToCountryFile();
        }

        public void AddIdeaToCountryFile()
        {
            try
            {

                string historyCountriesPath = Path.Combine(ModManager.Directory, "history", "countries");

                if (!Directory.Exists(historyCountriesPath))
                {
                    MessageBox.Show("Папка history/countries не найдена!");
                    return;
                }

                string[] countryFiles = Directory.GetFiles(historyCountriesPath, $"*{AddToTagBox.Text}*");
                if (countryFiles.Length == 0)
                {
                    MessageBox.Show($"Файл страны с тегом {AddToTagBox.Text} не найден!");
                    return;
                }

                string countryFile = countryFiles[0]; 
                string fileContent = File.ReadAllText(countryFile, Encoding.UTF8);

                string pattern = @"add_ideas\s*=\s*\{([^}]*)\}";
                Match match = Regex.Match(fileContent, pattern);

                if (!match.Success)
                {
                    MessageBox.Show("Блок add_ideas не найден в файле страны!");
                    return;
                }
                string ideasContent = match.Groups[1].Value;
                ideasContent += $"\n\t{IdBox.Text}";
                string newIdeasBlock = $"add_ideas = {{{ideasContent}\n}}";
                string newContent = Regex.Replace(fileContent, pattern, newIdeasBlock);

                File.WriteAllText(countryFile, newContent, Encoding.UTF8);

                MessageBox.Show($"Идея {IdBox.Text} успешно добавлена в файл {Path.GetFileName(countryFile)}!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
