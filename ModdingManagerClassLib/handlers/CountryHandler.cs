
using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.Extentions;
using ModdingManagerModels;
using System.Globalization;
using System.Text;
using System.Windows;
using MessageBox = System.Windows.MessageBox;


public class CountryHandler
{
    public CountryConfig Config { get; set; }
    public void CreateLocalizationFiles()
    {
        try
        {
            string ruLocPath = Path.Combine(ModManager.ModDirectory, "localisation", "russian");
            string enLocPath = Path.Combine(ModManager.ModDirectory, "localisation", "english");
            Directory.CreateDirectory(ruLocPath);
            Directory.CreateDirectory(enLocPath);

            string ruContent = GenerateLocalizationContent(this.Config.Tag, "l_russian", this.Config.Localisation.NameValue, this.Config.RulingParty);
            string enContent = GenerateLocalizationContent(this.Config.Tag, "l_english", this.Config.Localisation.NameValue, this.Config.RulingParty);

            string ruFilePath = Path.Combine(ruLocPath, $"{this.Config.Tag}_history_l_russian.yml");
            string enFilePath = Path.Combine(enLocPath, $"{this.Config.Tag}_history_l_english.yml");

            File.WriteAllText(ruFilePath, ruContent, new UTF8Encoding(true));
            File.WriteAllText(enFilePath, enContent, new UTF8Encoding(true));

            MessageBox.Show("Файлы локализации успешно созданы!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка создания файлов локализации: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string GenerateLocalizationContent(string tag, string languageKey, string name, string party)
    {
        var sb = new StringBuilder();
        List<string> lines = new List<string>();
        sb.AppendLine($"{languageKey}:");
        foreach (var i in ModConfig.Instance.Ideologies)
        {
            sb.AppendLine($" {tag}_{i.Id}: \"\"");
            sb.AppendLine($" {tag}_{i.Id}_DEF: \"\"");
        }

        sb.AppendLine($" {tag}: \"{name}\"");
        sb.AppendLine($" {tag}_DEF: \"\"");
        sb.AppendLine($" {tag}_ADJ: \"\"");

        return sb.ToString();
    }
    public void AddCountryTag()
    {
        if (this.Config.Tag.Length != 3)
        {
            MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string tagsDir = Path.Combine(ModManager.ModDirectory, "common", "country_tags");
        string countryTag = this.Config.Tag;

        string countryFileName = this.Config.CountryFileName;
        if (string.IsNullOrWhiteSpace(countryFileName))
        {
            countryFileName = $"{countryTag} - {Config.Localisation.NameValue}.txt";
        }

        string newEntry = $"{countryTag} = \"countries/{countryTag} - {Config.Localisation.NameValue}.txt\"";

        try
        {
            if (!Directory.Exists(tagsDir))
            {
                Directory.CreateDirectory(tagsDir);
            }

            string[] tagFiles;

            // Если задан конкретный файл — использовать его
            string specificTagFilePath = !string.IsNullOrWhiteSpace(this.Config.CountryFileName)
                ? Path.Combine(tagsDir, this.Config.CountryFileName)
                : null;

            if (!string.IsNullOrWhiteSpace(specificTagFilePath) && File.Exists(specificTagFilePath))
            {
                string content = File.ReadAllText(specificTagFilePath);
                if (content.Contains($"{countryTag} ="))
                {
                    MessageBox.Show($"Тег {countryTag} уже существует в файле {Path.GetFileName(specificTagFilePath)}!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (StreamWriter writer = File.AppendText(specificTagFilePath))
                {
                    writer.WriteLine(newEntry);
                }
            }
            else
            {
                tagFiles = Directory.GetFiles(tagsDir, "*.txt");

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
                        MessageBox.Show($"Тег {countryTag} уже существует в файле тегов!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    using (StreamWriter writer = File.AppendText(tagFile))
                    {
                        writer.WriteLine(newEntry);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при добавлении тега страны:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void UpdateStateOwnership()
    {
        if (string.IsNullOrWhiteSpace(this.Config.Tag) || this.Config.Tag.Length != 3)
        {
            MessageBox.Show("Не указаны стейты или тег страны неверный!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string modStatesDir = Path.Combine(ModManager.ModDirectory, "history", "states");
        string gameStatesDir = Path.Combine(ModManager.GameDirectory, "history", "states");

        if (!Directory.Exists(modStatesDir))
        {
            Directory.CreateDirectory(modStatesDir);
        }

        string countryTag = this.Config.Tag;
        var stateEntries = this.Config.StateCores;
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
                    MessageBox.Show($"Файл штата с ID {stateId} не найден ни в моде, ни в игре.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show($"Ошибка при обработке штата {stateId}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        MessageBox.Show("Обновление владельцев штатов завершено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void CreateCountryHistoryFile()
    {
        if (this.Config.Tag.Length != 3)
        {
            MessageBox.Show("Тег страны должен состоять из 3 символов (например, ANG).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (string.IsNullOrEmpty(this.Config.RulingParty))
        {
            MessageBox.Show($"Не достаточно информации про страну: Правящая партия = {this.Config.RulingParty}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string fileName = $"{Config.Tag} - {Config.Localisation.NameValue}.txt";
        string filePath = Path.Combine(ModManager.ModDirectory, "history", "countries", fileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (StreamWriter writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
            {
                if (int.IsPositive(this.Config.Capital ?? 0))
                {
                    writer.WriteLine($"capital = {this.Config.Capital}");
                    writer.WriteLine();
                }
                if (!string.IsNullOrWhiteSpace(this.Config.OOB))
                {
                    writer.WriteLine($"oob = \"{this.Config.OOB}\"");
                    writer.WriteLine();
                }

                if (this.Config.Technologies.Count > 0)
                {
                    writer.WriteLine("# Starting tech");
                    writer.WriteLine("set_technology = {");

                    foreach (var techLine in this.Config.Technologies)
                    {

                        writer.WriteLine($"\t{techLine.Key} = {techLine.Value}");

                    }

                    writer.WriteLine("}");
                    writer.WriteLine();
                }

                if (this.Config.Convoys >= 0)
                {
                    writer.WriteLine($"set_convoys = {this.Config.Convoys}");
                    writer.WriteLine();
                }

                if (this.Config.ResearchSlots >= 0)
                {
                    writer.WriteLine($"set_research_slots = {this.Config.ResearchSlots}");
                    writer.WriteLine();
                }

                if (!double.IsNegative(this.Config.Stab ?? 0))
                {
                    writer.WriteLine($"set_stability = {(this.Config.Stab ?? 0).ToString(CultureInfo.InvariantCulture)}");
                }

                if (!double.IsNegative(this.Config.WarSup ?? 0))
                {
                    writer.WriteLine($"set_war_support = {(this.Config.WarSup ?? 0).ToString(CultureInfo.InvariantCulture)}");
                }

                writer.WriteLine("set_politics = {");
                if (this.Config.RulingParty != null)
                {
                    writer.WriteLine($"\truling_party = {this.Config.RulingParty}");
                }

                writer.WriteLine($"\tlast_election = {this.Config.LastElection?.ToString("yyyy.MM.dd") ?? "no"}");
                writer.WriteLine($"\telection_frequency = {this.Config.ElectionFrequency}");
                writer.WriteLine($"\telections_allowed = {(this.Config.ElectionsAllowed == true ? "yes" : "no")}");
                writer.WriteLine("}");
                writer.WriteLine();

                // Популярность партий
                writer.WriteLine("set_popularities = {");
                foreach (var item in this.Config.PartyPopularities)
                {
                    writer.WriteLine($"{item.Key} = {item.Value}");
                }
                writer.WriteLine("}");
                writer.WriteLine();

                // Стартовые идеи
                writer.WriteLine("# Starting ideas");
                writer.WriteLine("add_ideas = volunteer_only");
                writer.WriteLine("add_ideas = civilian_economy");

                if (this.Config.Ideas.Count > 0)
                {
                    foreach (string idea in this.Config.Ideas)
                    {
                        string trimmedIdea = idea.Trim();
                        if (!string.IsNullOrEmpty(trimmedIdea))
                        {
                            writer.WriteLine($"add_ideas = {trimmedIdea}");
                        }
                    }
                }
                writer.WriteLine();

                if (this.Config.Characters.Count > 0)
                {
                    foreach (string character in this.Config.Characters)
                    {
                        string trimmedChar = character.Trim();
                        if (!string.IsNullOrEmpty(trimmedChar))
                        {
                            writer.WriteLine($"recruit_character = {trimmedChar}");
                        }
                    }
                }
            }

            MessageBox.Show($"Файл истории страны успешно создан: {filePath}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    public void CreateCommonCountriesFile()
    {
        if (this.Config.Tag.Length != 3)
        {
            MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (string.IsNullOrEmpty(this.Config.GraphicalCulture))
        {
            MessageBox.Show("не выбрана граф культура", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        string fileName = $"{this.CurrentConfig.Tag} - {this.CurrentConfig.Name}.txt";
        string filePath = Path.Combine(ModManager.ModDirectory, "common", "countries", fileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                if (!string.IsNullOrWhiteSpace(this.Config.GraphicalCulture))
                {
                    writer.WriteLine($"graphical_culture = {this.Config.GraphicalCulture}_gfx");
                    writer.WriteLine($"graphical_culture_2d = {this.Config.GraphicalCulture}_2d");
                    writer.WriteLine();
                }

                if (this.Config.Color != System.Drawing.Color.FromArgb(0, 0, 2))
                {
                    System.Windows.Media.Color color = (this.Config.Color ?? System.Drawing.Color.FromArgb(0, 0, 2)).ToMediaColor();
                    writer.WriteLine($"color = rgb {{ {color.R} {color.G} {color.B} }}");
                }

                MessageBox.Show($"Файл страны успешно создан: {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    public void CreateCountryFlags()
    {
        if (this.Config.Tag.Length != 3)
        {
            MessageBox.Show("Тег страны должен состоять из 3 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string flagsDir = Path.Combine(ModManager.ModDirectory, "gfx", "flags");
        string countryTag = this.Config.Tag;

        try
        {
            Directory.CreateDirectory(flagsDir);

            //OTM
            //foreach (var pair in this.Config.CountryFlags)
            //{
            //    string ideologySubdir = Path.Combine(flagsDir, $"{countryTag}_{pair.Key}.png");
            //    string ideologyDir = Path.GetDirectoryName(ideologySubdir);
            //    if (!string.IsNullOrEmpty(ideologyDir))
            //        Directory.CreateDirectory(ideologyDir);

            //    pair.Values.ToDrawingDotImage().SaveFlagSet(flagsDir, countryTag, pair.Key);
            //}

            MessageBox.Show($"Флаги страны {countryTag} успешно созданы в {flagsDir}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при создании флагов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


}

