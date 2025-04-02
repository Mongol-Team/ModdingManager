using ModdingManager;
using ModdingManager.configs;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

public static class ConfigManager
{
    private static readonly string ConfigsPath = Path.Combine("..", "..", "..", "data", "configs");
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Сохраняем текущее состояние формы в конфиг
    public static void SaveCurrentConfig(CountryCreator form, string configName)
    {
        try
        {
            Directory.CreateDirectory(ConfigsPath);

            var config = new CountryConfig
            {
                // Основные поля
                Tag = form.TagBox.Text,
                Capital = int.TryParse(form.CapitalBox.Text, out var capital) ? capital : 0,
                GraphicalCulture = form.GrapficalCultureBox.Text,
                Color = $"{form.CountryColorDialog.Color.R} {form.CountryColorDialog.Color.G} {form.CountryColorDialog.Color.B}",

                // Технологии и армия
                Technologies = new List<string>(form.TechBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)),
                Convoys = int.TryParse(form.ConvoyBox.Text, out var convoys) ? convoys : 0,
                OOB = form.StartOOBBox.Text,
                ResearchSlots = int.TryParse(form.ResearchSlotBox.Text, out var slots) ? slots : 0,
                Stab = int.Parse(form.StabBox.Text),
                WarSup = int.Parse(form.WarSupportBox.Text),
                Name = form.CountryNameBox.Text,
                // Политика
                RulingParty = form.RullingPartyBox.SelectedItem?.ToString(),
                LastElection = form.LastElectionBox.Text,
                ElectionFrequency = int.TryParse(form.ElectionFreqBox.Text, out var freq) ? freq : 0,
                ElectionsAllowed = form.IsElectionAllowedBox.Checked,

                // Популярность
                NeutralityPopularity = int.TryParse(form.NeutralPopularBox.Text, out var neutral) ? neutral : 0,
                FascismPopularity = int.TryParse(form.FascismPopularBox.Text, out var fascism) ? fascism : 0,
                CommunismPopularity = int.TryParse(form.CommunismPopularBox.Text, out var communism) ? communism : 0,
                DemocraticPopularity = int.TryParse(form.DemocraticPopularBox.Text, out var democratic) ? democratic : 0,

                // Идеи и персонажи
                Ideas = new List<string>(form.StartIdeasBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)),
                Characters = new List<string>(form.RecruitBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)),

                // Штаты
                States = ParseStates(form.CountryStatesBox.Text)
            };

            string filePath = Path.Combine(ConfigsPath, $"{configName}.json");
            string json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(filePath, json);

            MessageBox.Show($"Конфигурация '{configName}' успешно сохранена!", "Успех",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения конфигурации: {ex.Message}", "Ошибка",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Загружаем конфиг в форму
    public static void LoadConfigToForm(CountryCreator form, string configName)
    {
        try
        {
            string filePath = Path.Combine(ConfigsPath, $"{configName}.json");

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Конфигурация '{configName}' не найдена!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<CountryConfig>(json);

            form.TagBox.Text = config.Tag;
            form.CapitalBox.Text = config.Capital.ToString();
            form.GrapficalCultureBox.Text = config.GraphicalCulture;

            var colorParts = config.Color.Split(' ');
            if (colorParts.Length == 3)
            {
                form.CountryColorDialog.Color = Color.FromArgb(
                    int.Parse(colorParts[0]),
                    int.Parse(colorParts[1]),
                    int.Parse(colorParts[2]));
            }

            form.TechBox.Text = string.Join(Environment.NewLine, config.Technologies);
            form.ConvoyBox.Text = config.Convoys.ToString();
            form.StartOOBBox.Text = config.OOB;
            form.ResearchSlotBox.Text = config.ResearchSlots.ToString();
            form.WarSupportBox.Text = config.WarSup.ToString();
            form.StabBox.Text = config.Stab.ToString();
            form.CountryNameBox.Text = config.Name;
            form.RullingPartyBox.SelectedItem = config.RulingParty;
            form.LastElectionBox.Text = config.LastElection;
            form.ElectionFreqBox.Text = config.ElectionFrequency.ToString();
            form.IsElectionAllowedBox.Checked = config.ElectionsAllowed;

            form.NeutralPopularBox.Text = config.NeutralityPopularity.ToString();
            form.FascismPopularBox.Text = config.FascismPopularity.ToString();
            form.CommunismPopularBox.Text = config.CommunismPopularity.ToString();
            form.DemocraticPopularBox.Text = config.DemocraticPopularity.ToString();

            form.StartIdeasBox.Text = string.Join(Environment.NewLine, config.Ideas);
            form.RecruitBox.Text = string.Join(Environment.NewLine, config.Characters);

            form.CountryStatesBox.Text = string.Join(Environment.NewLine,
                config.States.Select(s => $"{s.Key}:{(s.Value ? "1" : "0")}"));

            MessageBox.Show($"Конфигурация '{configName}' успешно загружена!", "Успех",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}", "Ошибка",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Dictionary<int, bool> ParseStates(string statesText)
    {
        var states = new Dictionary<int, bool>();
        var lines = statesText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out var stateId))
            {
                states[stateId] = parts[1] == "1";
            }
        }

        return states;
    }

    private static readonly string CharactersPath = Path.Combine("data", "characters");

    // Сохраняем персонажа из формы
    public static void SaveCharConfigToForm(CharacterCreator form, string configName)
    {
        try
        {
            string dirPath = Path.Combine("data", "characters");
            Directory.CreateDirectory(dirPath);
            Directory.CreateDirectory(CharactersPath);

            string json = JsonSerializer.Serialize(new
            {
                // Основные свойства
                Id = form.IdBox.Text,
                Name = form.NameBox.Text,
                Description = form.DescBox.Text,
                Tag = form.TagBox.Text,

                // Статистика
                Skill = int.Parse(form.SkillBox.Text),
                Attack = int.Parse(form.AtkBox.Text),
                Defense = int.Parse(form.DefBox.Text),
                Supply = int.Parse(form.SupplyBox.Text),
                Speed = int.Parse(form.SpdBox.Text),

                // Советник
                AdvisorSlot = form.AdvisorSlot.Text,
                AdvisorCost = int.Parse(form.AdvisorCost.Text),
                AiWillDo = form.AiDoBox.Text,

                // Дополнительные
                Expire = form.ExpireBox.Text,
                Types = new List<string>(form.CharTypesBox.Text.Split('\n')),
                Traits = new List<string>(form.PercBox.Text.Split('\n')),

                // Иконки
                BigIconPath = form.currentCharacter?.BigIconPath ?? "",
                SmallIconPath = form.currentCharacter?.SmallIconPath ?? ""
            }, JsonOptions);

            string filePath = Path.Combine(CharactersPath, $"{configName}.json");
            File.WriteAllText(Path.Combine(dirPath, configName), json);
            MessageBox.Show("Персонаж сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void LoadCharConfigToForm(CharacterCreator form, string configName)
    {
        try
        {
            string filePath = Path.Combine(CharactersPath, $"{configName}.json");
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Файл не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var character = JsonSerializer.Deserialize<CountryCharacterConfig>(File.ReadAllText(filePath));

            // Заполняем форму
            form.IdBox.Text = character.Id;
            form.NameBox.Text = character.Name;
            form.DescBox.Text = character.Description;
            form.TagBox.Text = character.Tag;
            form.SkillBox.Text = character.Skill.ToString();
            form.AtkBox.Text = character.Attack.ToString();
            form.DefBox.Text = character.Defense.ToString();
            form.SupplyBox.Text = character.Supply.ToString();
            form.SpdBox.Text = character.Speed.ToString();
            form.AdvisorSlot.Text = character.AdvisorSlot;
            form.AdvisorCost.Text = character.AdvisorCost.ToString();
            form.AiDoBox.Text = character.AiWillDo;
            form.ExpireBox.Text = character.Expire;
            form.CharTypesBox.Text = string.Join("\n", character.Types);
            form.PercBox.Text = string.Join("\n", character.Traits);

            MessageBox.Show("Персонаж загружен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}