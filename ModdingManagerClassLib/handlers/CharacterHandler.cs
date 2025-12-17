using ModdingManagerClassLib.Extentions;
using ModdingManager.managers.@base;
using ModdingManagerModels;
using System.Text;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using ModdingManagerClassLib.Settings;

namespace ModdingManager.classes.handlers
{
    public class CharacterHandler
    {
        public CountryCharacterConfig CurrentConfig { get; set; }

        public void HandleGFXFile()
        {
            string guiDirectory = Path.Combine(ModManagerSettings.Instance.ModDirectory, "interface");
            Directory.CreateDirectory(guiDirectory);
            string filePath = Path.Combine(guiDirectory, $"{CurrentConfig.Tag}.gfx");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("spriteTypes = {");
                writer.WriteLine("\tspriteType = {");
                writer.WriteLine($"\t\tname = \"GFX_portrait_{CurrentConfig.Id}_large\"");
                writer.WriteLine($"\t\ttextureFile = gfx/leaders/{CurrentConfig.Tag}/{CurrentConfig.Id}_big.dds");
                writer.WriteLine("\t}");
                writer.WriteLine("\tspriteType = {");
                writer.WriteLine($"\t\tname = \"GFX_portrait_{CurrentConfig.Id}_small\"");
                writer.WriteLine($"\t\ttextureFile = \"gfx/advisors/{CurrentConfig.Tag}/{CurrentConfig.Id}_small.dds\"");
                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }
        public void HandleRecruting()
        {
            string GetFileWithExactCase(string directory, string tag)
            {
                return Directory.EnumerateFiles(directory, "*.txt", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(path => Path.GetFileName(path).Contains(tag, StringComparison.Ordinal));
            }
            string tag = CurrentConfig.Tag;
            string modPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "history", "countries");
            string gamePath = Path.Combine(ModManagerSettings.Instance.GameDirectory, "history", "countries");

            try
            {
                if (!Directory.Exists(modPath))
                    Directory.CreateDirectory(modPath);

                string modFile = GetFileWithExactCase(modPath, tag);

                if (modFile == null)
                {
                    if (!Directory.Exists(gamePath))
                    {
                        MessageBox.Show($"Папка оригинальной игры не найдена: {gamePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string gameFile = GetFileWithExactCase(gamePath, tag);

                    if (gameFile == null)
                    {
                        MessageBox.Show($"Файл с тегом '{tag}' не найден ни в моде, ни в оригинальной игре.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string fileName = Path.GetFileName(gameFile);
                    modFile = Path.Combine(modPath, fileName);
                    File.Copy(gameFile, modFile, overwrite: true);

                    MessageBox.Show($"Файл с тегом '{tag}' не найден в моде. Использован файл из оригинальной игры: {fileName}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                string recruitLine = $"recruit_character = {CurrentConfig.Id}";
                var lines = File.ReadAllLines(modFile).ToList();

                int lastRecruitIndex = lines.FindLastIndex(line => line.TrimStart().StartsWith("recruit_character ="));

                if (lastRecruitIndex >= 0)
                {
                    lines.Insert(lastRecruitIndex + 1, recruitLine);
                }
                else
                {
                    lines.Add(recruitLine);
                }

                File.WriteAllLines(modFile, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при обработке рекрутинга:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateCharacterLocalizationFiles()
        {
            try
            {
                string ruLocPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "localisation", "russian");
                string enLocPath = Path.Combine(ModManagerSettings.Instance.ModDirectory, "localisation", "english");
                Directory.CreateDirectory(ruLocPath);
                Directory.CreateDirectory(enLocPath);

                string ruFilePath = Path.Combine(ruLocPath, $"{CurrentConfig.Tag}_characters_l_russian.yml");
                string enFilePath = Path.Combine(enLocPath, $"{CurrentConfig.Tag}_characters_l_english.yml");

                string ruEntries = GenerateLocalizationEntries(
                    CurrentConfig.Id.ToString(), CurrentConfig.Localisation.Data.GetValueSafe(CurrentConfig.Id.ToString()), CurrentConfig.Localisation.Data.GetValueSafe($"{CurrentConfig.Id.ToString()}_desc"),
                    CurrentConfig.Types.Contains("country_leader"), false);

                string enEntries = GenerateLocalizationEntries(
                    CurrentConfig.Id.ToString(), "", "",
                    CurrentConfig.Types.Contains("country_leader"), true);

                ProcessLocalizationFile(ruFilePath, "l_russian", ruEntries);
                ProcessLocalizationFile(enFilePath, "l_english", enEntries);

                MessageBox.Show("Файлы локализации созданы успешно!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания локализации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static string GenerateLocalizationEntries(string charId, string name, string desc, bool isCountryLeader, bool isEnglish)
        {
            var sb = new StringBuilder();

            // Для английской версии всегда пустые значения
            string nameValue = isEnglish ? "\"\"" : $"\"{name}\"";
            string descValue = isEnglish ? "\"\"" : $"\"{desc}\"";

            sb.AppendLine($" {charId}:0 {nameValue}");

            if (isCountryLeader)
            {
                sb.AppendLine($" {charId}_desc:0 {descValue}");
            }

            return sb.ToString();
        }

        private static void ProcessLocalizationFile(string filePath, string languageKey, string newEntries)
        {
            string finalContent;
            Encoding utf8WithBom = new UTF8Encoding(true);

            if (File.Exists(filePath))
            {
                string existingContent = File.ReadAllText(filePath, utf8WithBom);

                finalContent = $"{existingContent}{newEntries}";
            }
            else
            {
                finalContent = $"{languageKey}:0\n{newEntries}";
            }

            File.WriteAllText(filePath, finalContent, utf8WithBom);
        }
        public void SaveCharacterPortraits()
        {
            var bigIcon = CurrentConfig.Gfx.Content;
            var smallIcon = CurrentConfig.Gfx.Content;
            try
            {
                if (bigIcon == null && smallIcon == null)
                {
                    MessageBox.Show("Необходимо добавить хотя бы 1 изображение!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string leadersDir = Path.Combine(ModManagerSettings.Instance.ModDirectory, "gfx", "leaders", CurrentConfig.Tag);
                string advisorsDir = Path.Combine(ModManagerSettings.Instance.ModDirectory, "gfx", "advisors", CurrentConfig.Tag);
                Directory.CreateDirectory(leadersDir);
                Directory.CreateDirectory(advisorsDir);

                bigIcon?.SaveAsDDS(leadersDir, $"{CurrentConfig.Id}_big", 156, 210);
                smallIcon?.SaveAsDDS(advisorsDir, $"{CurrentConfig.Id}_small", 65, 67);

                MessageBox.Show("Портреты сохранены успешно!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void CreateCharacterFile()
        {
            try
            {
                string charactersDir = Path.Combine(ModManagerSettings.Instance.ModDirectory, "common", "characters");
                Directory.CreateDirectory(charactersDir);

                string filePath = Path.Combine(charactersDir, $"{CurrentConfig.Tag}.txt");
                StringBuilder content = new StringBuilder();

                content.AppendLine($"\t{CurrentConfig.Id} = {{");
                content.AppendLine($"\t\tname = {CurrentConfig.Id}");

                content.AppendLine("\t\tportraits = {");
                content.AppendLine("\t\t\tcivilian = {");
                content.AppendLine($"\t\t\t\tlarge = GFX_portrait_{CurrentConfig.Id}_large");
                content.AppendLine($"\t\t\t\tsmall = GFX_portrait_{CurrentConfig.Id}_small");
                content.AppendLine("\t\t\t}");
                content.AppendLine("\t\t\tarmy = {");
                content.AppendLine($"\t\t\t\tlarge = GFX_portrait_{CurrentConfig.Id}_large");
                content.AppendLine($"\t\t\t\tsmall = GFX_portrait_{CurrentConfig.Id}_small");
                content.AppendLine("\t\t\t}");
                content.AppendLine("\t\t}");

                if (CurrentConfig.Types.Contains("advisor"))
                {
                    string slot = !string.IsNullOrEmpty(CurrentConfig.AdvisorSlot)
                        ? CurrentConfig.AdvisorSlot
                        : "high_command";

                    content.AppendLine("\t\tadvisor = {");
                    content.AppendLine($"\t\t\tslot = {slot}");
                    content.AppendLine($"\t\t\tidea_token = {CurrentConfig.Id}");
                    content.AppendLine("\t\t\tledger = army");
                    content.AppendLine("\t\t\tallowed = {");
                    content.AppendLine($"\t\t\t\toriginal_tag = {CurrentConfig.Tag}");
                    content.AppendLine("\t\t\t}");
                    content.AppendLine("\t\t\ttraits = {");
                    foreach (var trait in CurrentConfig.Traits)
                    {
                        content.AppendLine($"\t\t\t\t{trait}");
                    }
                    content.AppendLine("\t\t\t}");
                    content.AppendLine($"\t\t\tcost = {CurrentConfig.AdvisorCost}");
                    content.AppendLine("\t\t\tai_will_do = {");
                    content.AppendLine($"\t\t\t\tfactor = {CurrentConfig.AiWillDo}");
                    content.AppendLine("\t\t\t}");
                    content.AppendLine("\t\t}");
                }

                string[] militaryTypes = { "navy_leader", "field_marshal", "corps_commander" };
                var militaryType = CurrentConfig.Types.FirstOrDefault(t => militaryTypes.Contains(t));

                if (militaryType != null)
                {
                    content.AppendLine($"\t\t{militaryType} = {{");
                    content.AppendLine("\t\t\ttraits = {");
                    foreach (var trait in CurrentConfig.Traits)
                    {
                        content.AppendLine($"\t\t\t\t{trait}");
                    }
                    content.AppendLine("\t\t\t}");
                    content.AppendLine($"\t\t\tskill = {CurrentConfig.Skill}");
                    content.AppendLine($"\t\t\tattack_skill = {CurrentConfig.Attack}");
                    content.AppendLine($"\t\t\tdefense_skill = {CurrentConfig.Defense}");

                    if (militaryType == "navy_leader")
                    {
                        content.AppendLine($"\t\t\tmaneuvering_skill = {CurrentConfig.Speed}");
                        content.AppendLine($"\t\t\tcoordination_skill = {CurrentConfig.Supply}");
                    }
                    else
                    {
                        content.AppendLine($"\t\t\tplanning_skill = {CurrentConfig.Speed}");
                        content.AppendLine($"\t\t\tlogistics_skill = {CurrentConfig.Supply}");
                    }

                    content.AppendLine($"\t\t\tlegacy_id = {CurrentConfig.Id}");
                    content.AppendLine("\t\t}");
                }
                if (CurrentConfig.Types.Contains("country_leader"))
                {
                    content.AppendLine("\t\tcountry_leader = {");
                    if (!string.IsNullOrEmpty(CurrentConfig.Expire))
                    {
                        content.AppendLine($"\t\t\texpire = \"{CurrentConfig.Expire}\"");
                    }
                    content.AppendLine($"\t\t\tideology = \"{CurrentConfig.Ideology}\"");
                    content.AppendLine("\t\t\ttraits = {");
                    foreach (var trait in CurrentConfig.Traits)
                    {
                        content.AppendLine($"\t\t\t\t{trait}");
                    }
                    content.AppendLine("\t\t\t}");
                    content.AppendLine($"\t\t\tdesc = {CurrentConfig.Id}_desc");
                    content.AppendLine("\t\t}");
                }

                content.AppendLine("\t}");

                // Запись в файл
                string finalContent;
                if (File.Exists(filePath))
                {
                    string existingContent = File.ReadAllText(filePath);
                    if (existingContent.TrimEnd().EndsWith("}"))
                    {
                        existingContent = existingContent.TrimEnd()[..^1];
                    }
                    finalContent = $"{existingContent}\n{content}\n}}";
                }
                else
                {
                    finalContent = $"characters = {{\n{content}\n}}";
                }

                File.WriteAllText(filePath, finalContent, new UTF8Encoding(false));
                MessageBox.Show("Файл персонажа успешно создан/обновлен!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла персонажа: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
