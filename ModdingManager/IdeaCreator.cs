using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            // Путь к файлу tag.gfx

            string gfxFilePath = Path.Combine(ModManager.Directory, "interface", $"{tag}.gfx");

            // Стандартное начало файла, если он создаётся с нуля
            string defaultHeader = "spriteTypes = {\n";
            string defaultFooter = "}\n";

            // Формат нового SpriteType
            string newEntry = $"\tSpriteType = {{\n" +
                             $"\t\tname = \"GFX_idea_{ideaId}\"\n" +
                             $"\t\ttexturefile = \"gfx/interface/ideas/MEM/{ideaId}.dds\"\n" +
                             $"\t}}\n";

            // Если файл не существует, создаём его с новым ентри
            if (!File.Exists(gfxFilePath))
            {
                File.WriteAllText(gfxFilePath, defaultHeader + newEntry + defaultFooter);
                return;
            }

            // Если файл существует, проверяем, не добавлен ли уже такой SpriteType
            string currentContent = File.ReadAllText(gfxFilePath);

            if (currentContent.Contains($"GFX_idea_{ideaId}"))
            {
                MessageBox.Show($"Ентри '{ideaId}' уже есть в файле!", "ШВАЙНЕ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ищем закрывающую скобку `spriteTypes` и вставляем новый ентри перед ней
            int lastBraceIndex = currentContent.LastIndexOf('}');
            if (lastBraceIndex == -1)
            {
                // Если структура файла нарушена, пересоздаём его
                File.WriteAllText(gfxFilePath, defaultHeader + newEntry + defaultFooter);
                return;
            }

            // Вставляем новый ентри перед последней `}`
            string updatedContent = currentContent.Substring(0, lastBraceIndex) +
                                   newEntry +
                                   currentContent.Substring(lastBraceIndex);

            File.WriteAllText(gfxFilePath, updatedContent);
            MessageBox.Show("Файл успешно создан", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void GenerateLocalizationFiles(string tag, string ideaId, string name, string description)
        {
            // Пути к файлам локализации
            string englishFilePath = Path.Combine(ModManager.Directory, "localisation", "english", $"{tag}_ideas_l_english.yml");
            string russianFilePath = Path.Combine(ModManager.Directory, "localisation", "russian", $"{tag}_ideas_l_russian.yml");

            // Содержимое для английского файла (пустые кавычки после :0)
            string englishContent = $"l_english:\n" +
                                   $" {ideaId}:0 \"\"\n" +
                                   $" {ideaId}_desc:0 \"\"\n";

            // Содержимое для русского файла (значения из NameBox и DescBox)
            string russianContent = $"l_russian:\n" +
                                    $" {ideaId}:0 \"{name}\"\n" +
                                    $" {ideaId}_desc:0 \"{description}\"\n";

            // Создаём директории, если их нет
            Directory.CreateDirectory(Path.GetDirectoryName(englishFilePath));
            Directory.CreateDirectory(Path.GetDirectoryName(russianFilePath));

            // Записываем файлы в UTF-8-BOM
            WriteFileWithBOM(englishFilePath, englishContent);
            WriteFileWithBOM(russianFilePath, russianContent);
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
            //CreateCountryIdea();
            //GenerateOrUpdateIdeaGFX(IdBox.Text, TagBox.Text);
            //GenerateLocalizationFiles(TagBox.Text, IdBox.Text, NameBox.Text, DescBox.Text);
            if (ImagePanel.BackgroundImage != null)
            {
                ModManager.SaveIdeaGFX(ImagePanel.BackgroundImage, ModManager.Directory, IdBox.Text, TagBox.Text);
            }
            else
            {
                MessageBox.Show("Картинку добавь алкаш", "алкаш", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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
    }
}
