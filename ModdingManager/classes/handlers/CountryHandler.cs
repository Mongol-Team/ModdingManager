using ModdingManager.classes.extentions;
using ModdingManager.classes.gfx;
using ModdingManager.configs;
using ModdingManager.managers.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ModdingManager.classes.handlers
{
    public class CountryHandler
    {
        public CountryConfig CurrentConfig { get;  set; }
        public  void CreateLocalizationFiles()
        {
            try
            {
                string ruLocPath = Path.Combine(ModManager.Directory, "localisation", "russian");
                string enLocPath = Path.Combine(ModManager.Directory, "localisation", "english");
                Directory.CreateDirectory(ruLocPath);
                Directory.CreateDirectory(enLocPath);

                string ruContent = GenerateLocalizationContent(this.CurrentConfig.Tag, "l_russian");
                string enContent = GenerateLocalizationContent(this.CurrentConfig.Tag, "l_english");

                string ruFilePath = Path.Combine(ruLocPath, $"{this.CurrentConfig.Tag}_history_l_russian.yml");
                string enFilePath = Path.Combine(enLocPath, $"{this.CurrentConfig.Tag}_history_l_english.yml");

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
        public void AddCountryTag()
        {
            if (this.CurrentConfig.Tag.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string tagsDir = Path.Combine(ModManager.Directory, "common", "country_tags");
            string countryTag = this.CurrentConfig.Tag;
            string countryFileName = $"{countryTag} - {this.CurrentConfig.Name}.txt";

            try
            {
                if (!Directory.Exists(tagsDir))
                {
                    Directory.CreateDirectory(tagsDir);
                }

                var tagFiles = Directory.GetFiles(tagsDir, "*.txt");

                string newEntry = $"{countryTag} = \"countries/{countryFileName}\"";

                if (tagFiles.Length == 0)
                {
                    string newTagFile = Path.Combine(tagsDir, "00_countries.txt");
                    File.WriteAllText(newTagFile, newEntry, new UTF8Encoding(false));
                }
                else
                {
                    string tagFile = tagFiles[0];
                    string content = File.ReadAllText(tagFile);
                    if (content.Contains($"{countryTag} ="))
                    {
                        MessageBox.Show($"Тег {countryTag} уже существует в файле тегов!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (StreamWriter writer = File.AppendText(tagFile))
                    {
                        writer.WriteLine(newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении тега страны:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void UpdateStateOwnership()
        {
            if (string.IsNullOrWhiteSpace(this.CurrentConfig.Tag) || this.CurrentConfig.Tag.Length != 3)
            {
                MessageBox.Show("Не указаны стейты или тег страны неверный!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string modStatesDir = Path.Combine(ModManager.Directory, "history", "states");
            string gameStatesDir = Path.Combine(ModManager.GameDirectory, "history", "states");

            if (!Directory.Exists(modStatesDir))
            {
                Directory.CreateDirectory(modStatesDir);
            }

            string countryTag = this.CurrentConfig.Tag;
            var stateEntries = this.CurrentConfig.States;
            Encoding utf8WithoutBom = new UTF8Encoding(false);

            foreach (var entry in stateEntries)
            {
                int stateId = entry.Key;
                bool isCore = entry.Value;
                string[] stateFiles = Directory.GetFiles(modStatesDir, $"{stateId}-*.txt");

                if (stateFiles.Length == 0)
                {
                    string[] gameStateFiles = Directory.GetFiles(gameStatesDir, $"{stateId}-*.txt");
                    if (gameStateFiles.Length > 0)
                    {
                        foreach (string gameStateFile in gameStateFiles)
                        {
                            string fileName = Path.GetFileName(gameStateFile);
                            string destinationPath = Path.Combine(modStatesDir, fileName);
                            File.Copy(gameStateFile, destinationPath, true);
                        }

                        stateFiles = Directory.GetFiles(modStatesDir, $"{stateId}-*.txt");
                    }
                    else
                    {
                        MessageBox.Show($"Файл штата с ID {stateId} не найден ни в моде, ни в игре.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        continue;
                    }
                }

                foreach (string filePath in stateFiles)
                {
                    try
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
                                string indent = new string(line.TakeWhile(char.IsWhiteSpace).ToArray());
                                lines[i] = $"{indent}owner = {countryTag}";
                                modified = true;

                                if (isCore && i + 1 < lines.Length && !lines[i + 1].Trim().StartsWith($"add_core_of = {countryTag}"))
                                {
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
                                    var newLines = lines.ToList();
                                    newLines.RemoveAt(i);
                                    lines = newLines.ToArray();
                                    modified = true;
                                    i--;
                                }
                                else
                                {
                                    hasCore = true;
                                }
                            }
                        }

                        if (isCore && !hasCore)
                        {
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].Trim().StartsWith("owner ="))
                                {
                                    string indent = new string(lines[i].TakeWhile(char.IsWhiteSpace).ToArray());
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
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обработке штата {stateId}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            MessageBox.Show("Обновление владельцев штатов завершено!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void CreateCountryHistoryFile()
        {
            if (this.CurrentConfig.Tag.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов (например, ANG).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(this.CurrentConfig.RulingParty))
            {
                MessageBox.Show($"Не достаточно информации про страну: Правящая партия = {this.CurrentConfig.RulingParty}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fileName = $"{this.CurrentConfig.Tag} - {this.CurrentConfig.Name}.txt";
            string filePath = Path.Combine(ModManager.Directory, "history", "countries", fileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (StreamWriter writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
                {
                    // Столица
                    if (!int.IsPositive(this.CurrentConfig.Capital ?? 0))
                    {
                        writer.WriteLine($"capital = {this.CurrentConfig.Capital}");
                        writer.WriteLine();
                    }

                    // OOB
                    if (!string.IsNullOrWhiteSpace(this.CurrentConfig.OOB))
                    {
                        writer.WriteLine($"oob = \"{this.CurrentConfig.OOB}\"");
                        writer.WriteLine();
                    }

                    // Технологии
                    if (this.CurrentConfig.Technologies.Count > 0)
                    {
                        writer.WriteLine("# Starting tech");
                        writer.WriteLine("set_technology = {");

                        foreach (var techLine in this.CurrentConfig.Technologies)
                        {
                           
                                writer.WriteLine($"\t{techLine.Key} = {techLine.Value}");

                        }

                        writer.WriteLine("}");
                        writer.WriteLine();
                    }

                    // Конвои
                    if (this.CurrentConfig.Convoys >= 0)
                    {
                        writer.WriteLine($"set_convoys = {this.CurrentConfig.Convoys}");
                        writer.WriteLine();
                    }

                    // Слоты исследований
                    if (this.CurrentConfig.ResearchSlots >= 0)
                    {
                        writer.WriteLine($"set_research_slots = {this.CurrentConfig.ResearchSlots}");
                        writer.WriteLine();
                    }

                    // Стабильность и поддержка войны
                    if (this.CurrentConfig.Stab >= 0)
                    {
                        
                        writer.WriteLine($"set_stability = {this.CurrentConfig.Stab}");
                    }

                    if (this.CurrentConfig.WarSup >= 0)
                    {
                        writer.WriteLine($"set_war_support = {this.CurrentConfig.WarSup}");
                    }

                    // Политика
                    writer.WriteLine("set_politics = {");
                    if (this.CurrentConfig.RulingParty != null)
                    {
                        writer.WriteLine($"\truling_party = {this.CurrentConfig.RulingParty}");
                    }

                    writer.WriteLine($"\tlast_election = \"{this.CurrentConfig.LastElection}\"");
                    writer.WriteLine($"\telection_frequency = {this.CurrentConfig.ElectionFrequency}");
                    writer.WriteLine($"\telections_allowed = {this.CurrentConfig.ElectionsAllowed}");
                    writer.WriteLine("}");
                    writer.WriteLine();

                    // Популярность партий
                    writer.WriteLine("set_popularities = {");
                    foreach (var item in this.CurrentConfig.PartyPopularities)
                    {
                        writer.WriteLine($"{item.Key} = {item.Value}");
                    }
                    writer.WriteLine("}");
                    writer.WriteLine();

                    // Стартовые идеи
                    writer.WriteLine("# Starting ideas");
                    writer.WriteLine("add_ideas = volunteer_only");
                    writer.WriteLine("add_ideas = civilian_economy");

                    if (this.CurrentConfig.Ideas.Count > 0)
                    {
                        foreach (string idea in this.CurrentConfig.Ideas)
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
                    if (this.CurrentConfig.Characters.Count > 0)
                    {
                        foreach (string character in this.CurrentConfig.Characters)
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
        public void CreateCommonCountriesFile()
        {
            if (this.CurrentConfig.Tag.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(this.CurrentConfig.GraphicalCulture))
            {
                MessageBox.Show("не выбрана граф культура", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string fileName = $"{this.CurrentConfig.Tag} - {this.CurrentConfig.Name}.txt";
            string filePath = Path.Combine(ModManager.Directory, "common", "countries", fileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    if (!string.IsNullOrWhiteSpace(this.CurrentConfig.GraphicalCulture))
                    {
                        writer.WriteLine($"graphical_culture = {this.CurrentConfig.GraphicalCulture}_gfx");
                        writer.WriteLine($"graphical_culture_2d = {this.CurrentConfig.GraphicalCulture}_2d");
                        writer.WriteLine();
                    }

                    if (this.CurrentConfig.Color != System.Windows.Media.Color.FromRgb(0, 0, 2))
                    {
                        System.Windows.Media.Color color = this.CurrentConfig.Color ?? System.Windows.Media.Color.FromRgb(0, 0, 2);
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
        public void CreateCountryFlags()
        {
            if (this.CurrentConfig.Tag.Length != 3)
            {
                MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string flagsDir = Path.Combine(ModManager.Directory, "gfx", "flags");
            string countryTag = this.CurrentConfig.Tag;

            try
            {
                // Создаём директорию, если её нет
                Directory.CreateDirectory(flagsDir);

                foreach (var pair in this.CurrentConfig.CountryFlags)
                {
                    // Создаём поддиректорию для флагов конкретной идеологии, если нужно
                    string ideologySubdir = Path.Combine(flagsDir, $"{countryTag}_{pair.Key}.png");
                    string ideologyDir = Path.GetDirectoryName(ideologySubdir);
                    if (!string.IsNullOrEmpty(ideologyDir))
                        Directory.CreateDirectory(ideologyDir);

                    // Сохраняем флаг
                    pair.Value.ToDrawingDotImage().SaveFlagSet(flagsDir, countryTag, pair.Key);
                }

                MessageBox.Show($"Флаги страны {countryTag} успешно созданы в {flagsDir}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании флагов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
