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

        private void panel1_DragDrop(object sender, DragEventArgs e)
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

                        CountryNeutralFlagPanel.BackgroundImage = image;

                        CountryNeutralFlagPanel.BackgroundImageLayout = ImageLayout.Stretch;
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
        public static void CreateLocalizationFiles(string modPath, string countryTag)
        {
            try
            {
                string ruLocPath = Path.Combine(modPath, "localisation", "russian");
                string enLocPath = Path.Combine(modPath, "localisation", "english");
                Directory.CreateDirectory(ruLocPath);
                Directory.CreateDirectory(enLocPath);

                string ruContent = GenerateLocalizationContent(countryTag, "l_russian");
                string enContent = GenerateLocalizationContent(countryTag, "l_english");

                string ruFilePath = Path.Combine(ruLocPath, $"{countryTag}_history_l_russian.yml");
                string enFilePath = Path.Combine(enLocPath, $"{countryTag}_history_l_english.yml");

                File.WriteAllText(ruFilePath, ruContent, new UTF8Encoding(true));
                File.WriteAllText(enFilePath, enContent, new UTF8Encoding(true));

                MessageBox.Show("Файлы локализации успешно созданы!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания файлов локализации: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GenerateLocalizationContent(string tag, string languageKey)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{languageKey}:");

            sb.AppendLine($" {tag}_fascism: \"\"");
            sb.AppendLine($" {tag}_fascism_DEF: \"\"");
            sb.AppendLine($" {tag}_democratic: \"\"");
            sb.AppendLine($" {tag}_democratic_DEF: \"\"");
            sb.AppendLine($" {tag}_neutrality: \"\"");
            sb.AppendLine($" {tag}_neutrality_DEF: \"\"");
            sb.AppendLine($" {tag}_communism: \"\"");
            sb.AppendLine($" {tag}_communism_DEF: \"\"");
            sb.AppendLine($" {tag}_fascism_ADJ: \"\"");
            sb.AppendLine($" {tag}_democratic_ADJ: \"\"");
            sb.AppendLine($" {tag}_neutrality_ADJ: \"\"");
            sb.AppendLine($" {tag}_communism_ADJ: \"\"");
            sb.AppendLine($" {tag}: \"\"");
            sb.AppendLine($" {tag}_DEF: \"\"");
            sb.AppendLine($" {tag}_ADJ: \"\"");

            return sb.ToString();
        }
        private void AddCountryTag()
        {
            if (TagBox.Text.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tagsDir = Path.Combine(ModManager.directory, "common", "country_tags");
            string countryTag = TagBox.Text;
            string countryFileName = $"{countryTag}_{CapitalBox.Text}.txt";

            try
            {
                // Находим первый файл в директории country_tags
                var tagFiles = Directory.GetFiles(tagsDir, "*.txt");
                if (tagFiles.Length == 0)
                {
                    // Создаем новый файл, если нет существующих
                    string newTagFile = Path.Combine(tagsDir, "00_countries.txt");
                    Directory.CreateDirectory(tagsDir);
                    File.WriteAllText(newTagFile, $"{countryTag} = \"countries/{countryFileName}\"", Encoding.UTF8);
                }
                else
                {
                    // Добавляем в первый найденный файл тегов
                    string tagFile = tagFiles[0];
                    string newEntry = $"{countryTag} = \"countries/{countryFileName}\"";

                    // Проверяем, нет ли уже такого тега
                    string content = File.ReadAllText(tagFile);
                    if (content.Contains($"{countryTag} ="))
                    {
                        MessageBox.Show($"Тег {countryTag} уже существует в файле тегов!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Добавляем новую запись в конец файла
                    using (StreamWriter writer = File.AppendText(tagFile))
                    {
                        writer.WriteLine(newEntry);
                    }
                }

                MessageBox.Show($"Тег страны {countryTag} успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении тега страны: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateStateOwnership()
        {
            if (string.IsNullOrWhiteSpace(CountryStatesBox.Text) || TagBox.Text.Length != 3)
            {
                MessageBox.Show("Не указаны стейты или тег страны неверный!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string statesDir = Path.Combine(ModManager.directory, "history", "states");
            if (!Directory.Exists(statesDir))
            {
                MessageBox.Show($"Директория штатов не найдена: {statesDir}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string countryTag = TagBox.Text;
            string[] stateEntries = CountryStatesBox.Text.Split(new[] { "\r\n", "\r", "\n", "\v" }, StringSplitOptions.RemoveEmptyEntries);

            // Создаем кодировку UTF-8 без BOM
            Encoding utf8WithoutBom = new UTF8Encoding(false);

            foreach (string entry in stateEntries)
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 2) continue;

                string stateId = parts[0].Trim();
                bool isCore = parts[1].Trim() == "1";

                try
                {
                    string[] stateFiles = Directory.GetFiles(statesDir, $"*{stateId}*.txt");
                    if (stateFiles.Length == 0)
                    {
                        MessageBox.Show($"Файл штата с ID {stateId} не найден", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }

                    foreach (string filePath in stateFiles)
                    {
                        string[] lines = File.ReadAllLines(filePath, utf8WithoutBom);
                        bool modified = false;
                        bool hasCore = false;

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            string trimmedLine = line.Trim();

                            if (trimmedLine.StartsWith("owner ="))
                            {
                                // Сохраняем оригинальные отступы
                                string indent = new string(line.TakeWhile(c => char.IsWhiteSpace(c)).ToArray());
                                lines[i] = $"{indent}owner = {countryTag}";
                                modified = true;

                                // Проверяем следующий элемент на add_core_of
                                if (isCore && i + 1 < lines.Length && !lines[i + 1].Trim().StartsWith($"add_core_of = {countryTag}"))
                                {
                                    // Вставляем add_core_of после owner
                                    var newLines = lines.ToList();
                                    newLines.Insert(i + 1, $"{indent}add_core_of = {countryTag}");
                                    lines = newLines.ToArray();
                                    hasCore = true;
                                    modified = true;
                                }
                            }
                            else if (trimmedLine.StartsWith($"add_core_of = {countryTag}"))
                            {
                                if (!isCore)
                                {
                                    // Удаляем core если не нужно
                                    var newLines = lines.ToList();
                                    newLines.RemoveAt(i);
                                    lines = newLines.ToArray();
                                    modified = true;
                                    i--; // Корректируем индекс после удаления
                                }
                                else
                                {
                                    hasCore = true;
                                }
                            }
                        }

                        // Добавляем core если нужно и его еще нет
                        if (isCore && !hasCore)
                        {
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].Trim().StartsWith("owner ="))
                                {
                                    string indent = new string(lines[i].TakeWhile(c => char.IsWhiteSpace(c)).ToArray());
                                    var newLines = lines.ToList();
                                    newLines.Insert(i + 1, $"{indent}add_core_of = {countryTag}");
                                    lines = newLines.ToArray();
                                    modified = true;
                                    break;
                                }
                            }
                        }

                        if (modified)
                        {
                            File.WriteAllLines(filePath, lines, utf8WithoutBom);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке штата {stateId}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("Обновление владельцев штатов завершено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void CreateCountryHistoryFile()
        {
            if (TagBox.Text.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов (например, ANG).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (RullingPartyBox.SelectedItem == null || string.IsNullOrEmpty(LastElectionBox.Text))
            {
                MessageBox.Show("не достаточно инфы " + RullingPartyBox.SelectedItem + LastElectionBox.Text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName = $"{TagBox.Text} - {CountryNameBox.Text}.txt";
            string filePath = Path.Combine(ModManager.directory, "history", "countries", fileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (StreamWriter writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
                {
                    // Столица
                    if (!string.IsNullOrWhiteSpace(CapitalBox.Text))
                    {
                        writer.WriteLine($"capital = {CapitalBox.Text}");
                        writer.WriteLine();
                    }

                    // OOB
                    if (!string.IsNullOrWhiteSpace(StartOOBBox.Text))
                    {
                        writer.WriteLine($"oob = \"{StartOOBBox.Text}\"");
                        writer.WriteLine();
                    }

                    // Технологии
                    if (!string.IsNullOrWhiteSpace(TechBox.Text))
                    {
                        writer.WriteLine("# Starting tech");
                        writer.WriteLine("set_technology = {");

                        string[] techLines = TechBox.Text.Split(new[] { "\r\n", "\r", "\n", "\v" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string techLine in techLines)
                        {
                            string trimmedLine = techLine.Trim();
                            if (!string.IsNullOrEmpty(trimmedLine))
                            {
                                string formattedTech = trimmedLine.Contains(":")
                                    ? trimmedLine.Replace(":", " = ")
                                    : trimmedLine;
                                writer.WriteLine($"\t{formattedTech}");
                            }
                        }

                        writer.WriteLine("}");
                        writer.WriteLine();
                    }

                    // Конвои
                    if (!string.IsNullOrWhiteSpace(ConvoyBox.Text))
                    {
                        writer.WriteLine($"set_convoys = {ConvoyBox.Text}");
                        writer.WriteLine();
                    }

                    // Слоты исследований
                    if (!string.IsNullOrWhiteSpace(ResearchSlotBox.Text))
                    {
                        writer.WriteLine($"set_research_slots = {ResearchSlotBox.Text}");
                        writer.WriteLine();
                    }

                    // Стабильность и поддержка войны
                    if (!string.IsNullOrWhiteSpace(StabBox.Text))
                    {
                        // Добавляем "0." только если число не содержит десятичной точки
                        string stabilityValue = StabBox.Text.Contains(".") ? StabBox.Text : $"0.{StabBox.Text}";
                        writer.WriteLine($"set_stability = {stabilityValue}");
                    }

                    if (!string.IsNullOrWhiteSpace(WarSupportBox.Text))
                    {
                        string warSupportValue = WarSupportBox.Text.Contains(".") ? WarSupportBox.Text : $"0.{WarSupportBox.Text}";
                        writer.WriteLine($"set_war_support = {warSupportValue}");
                        writer.WriteLine();
                    }

                    // Политика
                    writer.WriteLine("set_politics = {");
                    if (RullingPartyBox.SelectedItem != null)
                    {
                        writer.WriteLine($"\truling_party = {RullingPartyBox.SelectedItem}");
                    }

                    writer.WriteLine($"\tlast_election = \"{LastElectionBox.Text}\"");
                    writer.WriteLine($"\telection_frequency = {ElectionFreqBox.Text}");
                    writer.WriteLine($"\telections_allowed = {(IsElectionAllowedBox.Checked ? "yes" : "no")}");
                    writer.WriteLine("}");
                    writer.WriteLine();

                    // Популярность партий
                    writer.WriteLine("set_popularities = {");
                    writer.WriteLine($"\tdemocratic = {DemocraticPopularBox.Text ?? "0"}");
                    writer.WriteLine($"\tfascism = {FascismPopularBox.Text ?? "0"}");
                    writer.WriteLine($"\tcommunism = {CommunismPopularBox.Text ?? "0"}");
                    writer.WriteLine($"\tneutrality = {NeutralPopularBox.Text ?? "100"}");
                    writer.WriteLine("}");
                    writer.WriteLine();

                    // Стартовые идеи
                    writer.WriteLine("# Starting ideas");
                    writer.WriteLine("add_ideas = volunteer_only");
                    writer.WriteLine("add_ideas = civilian_economy");

                    if (!string.IsNullOrWhiteSpace(StartIdeasBox.Text))
                    {
                        string[] ideas = StartIdeasBox.Text.Split(new[] { "\r\n", "\r", "\n", "\v" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string idea in ideas)
                        {
                            string trimmedIdea = idea.Trim();
                            if (!string.IsNullOrEmpty(trimmedIdea))
                            {
                                writer.WriteLine($"add_ideas = {trimmedIdea}");
                            }
                        }
                    }
                    writer.WriteLine();

                    // Персонажи
                    if (!string.IsNullOrWhiteSpace(RecruitBox.Text))
                    {
                        string[] characters = RecruitBox.Text.Split(new[] { "\r\n", "\r", "\n", "\v" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string character in characters)
                        {
                            string trimmedChar = character.Trim();
                            if (!string.IsNullOrEmpty(trimmedChar))
                            {
                                writer.WriteLine($"recruit_character = {trimmedChar}");
                            }
                        }
                    }
                }

                MessageBox.Show($"Файл истории страны успешно создан: {filePath}", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CreateCommonCountriesFile()
        {
            if (TagBox.Text.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(GrapficalCultureBox.Text))
            {
                MessageBox.Show("не выбрана граф культура", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string fileName = $"{CountryNameBox.Text}.txt";
            string filePath = Path.Combine(ModManager.directory, "common", "countries", fileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Графическая культура
                    if (!string.IsNullOrWhiteSpace(GrapficalCultureBox.Text))
                    {
                        writer.WriteLine($"graphical_culture = {GrapficalCultureBox.Text}_gfx");
                        writer.WriteLine($"graphical_culture_2d = {GrapficalCultureBox.Text}_2d");
                        writer.WriteLine();
                    }

                    // Цвет страны
                    if (CountryColorDialog.Color != Color.Empty)
                    {
                        Color color = CountryColorDialog.Color;
                        writer.WriteLine($"color = rgb {{ {color.R} {color.G} {color.B} }}");
                    }

                    MessageBox.Show($"Файл страны успешно создан: {filePath}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CreateCountryFlags()
        {
            if (TagBox.Text.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string flagsDir = Path.Combine(ModManager.directory, "gfx", "flags");
            string countryTag = TagBox.Text;

            try
            {
                // Получаем изображения из панелей
                Image fascismFlag = GetImageFromPanel(CountryFascismFlagPanel);
                Image neutralityFlag = GetImageFromPanel(CountryNeutralFlagPanel);
                Image communismFlag = GetImageFromPanel(CountryCommunismFlagPanel);
                Image democraticFlag = GetImageFromPanel(CountryDecmocraticFlagPanel);

                // Создаем флаги
                ModManager.SaveCountryFlag(
                    fascismFlag,
                    neutralityFlag,
                    communismFlag,
                    democraticFlag,
                    flagsDir,
                    countryTag
                );

                MessageBox.Show($"Флаги страны {countryTag} успешно созданы в {flagsDir}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании флагов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Image GetImageFromPanel(Panel panel)
        {
            if (panel.BackgroundImage != null)
            {
                return panel.BackgroundImage;
            }

            // Создаем пустое изображение, если флаг не установлен
            Bitmap emptyFlag = new Bitmap(82, 52);
            using (Graphics g = Graphics.FromImage(emptyFlag))
            {
                g.Clear(Color.Magenta); // Прозрачный цвет (HOI4 использует magenta для прозрачности)
            }
            return emptyFlag;
        }
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            CreateCountryHistoryFile();
            UpdateStateOwnership();
            CreateCommonCountriesFile();
            CreateCountryFlags();
            AddCountryTag();
            CreateLocalizationFiles(ModManager.directory, TagBox.Text);
        }

        private void ColorPickerButton_Click(object sender, EventArgs e)
        {
            CountryColorDialog.ShowDialog(this);

            if (CountryColorDialog.ShowDialog() == DialogResult.OK)
            {
                ColorPickerButton.BackColor = CountryColorDialog.Color;
            }
        }

        private void CountryFascismFlagPanel_DragDrop(object sender, DragEventArgs e)
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

                        CountryFascismFlagPanel.BackgroundImage = image;

                        CountryFascismFlagPanel.BackgroundImageLayout = ImageLayout.Stretch;
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

        private void CountryFascismFlagPanel_DragEnter(object sender, DragEventArgs e)
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

        private void CountryCommunismFlagPanel_DragDrop(object sender, DragEventArgs e)
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

                        CountryCommunismFlagPanel.BackgroundImage = image;

                        CountryCommunismFlagPanel.BackgroundImageLayout = ImageLayout.Stretch;
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

        private void CountryCommunismFlagPanel_DragEnter(object sender, DragEventArgs e)
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

        private void CountryDecmocraticFlagPanel_DragDrop(object sender, DragEventArgs e)
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

                        CountryDecmocraticFlagPanel.BackgroundImage = image;

                        CountryDecmocraticFlagPanel.BackgroundImageLayout = ImageLayout.Stretch;
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

        private void CountryDecmocraticFlagPanel_DragEnter(object sender, DragEventArgs e)
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

        private void ConfigLoadButton_Click(object sender, EventArgs e)
        {
            string configName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя конфигурации:", "Загрузка конфигурации");
            if (!string.IsNullOrEmpty(configName))
            {
                ConfigManager.LoadConfigToForm(this, configName);
            }
        }

        private void SaveConfigButton_Click(object sender, EventArgs e)
        {
            string configName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя конфигурации:", "Сохранение конфигурации");
            if (!string.IsNullOrEmpty(configName))
            {
                ConfigManager.SaveCurrentConfig(this, configName);
            }
        }
    }
}
