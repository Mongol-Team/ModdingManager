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

    private static readonly string CharactersPath = Path.Combine(ConfigsPath, "characters");

    // Сохраняем персонажа из формы
    public static bool SaveCharacterConfig(CharacterCreator form, string configName)
    {
        try
        {
            var character = new CountryCharacterConfig
            {
                // Основные свойства
                Id = form.IdBox.Text.Trim(),
                Name = form.NameBox.Text.Trim(),
                Description = form.DescBox.Text.Trim(),
                Tag = form.TagBox.Text.Trim(),

                // Статистика
                Skill = int.Parse(form.SkillBox.Text),
                Attack = int.Parse(form.AtkBox.Text),
                Defense = int.Parse(form.DefBox.Text),
                Supply = int.Parse(form.SupplyBox.Text),
                Speed = int.Parse(form.SpdBox.Text),

                // Советник
                AdvisorSlot = form.AdvisorSlot.Text,
                AdvisorCost = int.Parse(form.AdvisorCost.Text),
                AiWillDo = form.AiDoBox.Text.Trim(),

                // Дополнительные
                Expire = form.ExpireBox.Text.Trim(),
                Types = form.CharTypesBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
                Traits = form.PercBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList(),

                // Иконки
                BigIconPath = form.currentCharacter?.BigIconPath ?? "",
                SmallIconPath = form.currentCharacter?.SmallIconPath ?? ""
            };

            // Сохраняем JSON
            string filePath = Path.Combine(CharactersPath, $"{configName}.json");
            File.WriteAllText(filePath, JsonSerializer.Serialize(character, JsonOptions));

           

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
    public static List<string> GetAvailableCharacterConfigs()
    {
        try
        {
            if (!Directory.Exists(CharactersPath))
            {
                Directory.CreateDirectory(CharactersPath);
                return new List<string>();
            }

            return Directory.GetFiles(CharactersPath, "*.json")
                          .Select(Path.GetFileNameWithoutExtension)
                          .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static async Task LoadCharacterConfigAsync(CharacterCreator form)
    {
        try
        {
            // Получаем конфиги в фоновом потоке
            var configs = await Task.Run(() => GetAvailableCharacterConfigs());

            if (configs.Count == 0)
            {
                form.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(form, "Нет сохранённых персонажей", "Информация",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
                return;
            }

            // Создаем диалог в UI-потоке
            string selectedConfig = null;
            await Task.Run(() =>
            {
                form.Invoke((MethodInvoker)delegate
                {
                    using (var dialog = new Form()
                    {
                        Text = "Выберите персонажа",
                        Width = 350,
                        Height = 450,
                        StartPosition = FormStartPosition.CenterParent
                    })
                    {
                        var listBox = new ListBox
                        {
                            Dock = DockStyle.Fill,
                            DataSource = configs,
                            Font = new Font("Arial", 12)
                        };

                        var btnLoad = new Button
                        {
                            Text = "Загрузить",
                            Dock = DockStyle.Bottom,
                            Height = 40,
                            DialogResult = DialogResult.OK
                        };

                        dialog.Controls.Add(listBox);
                        dialog.Controls.Add(btnLoad);
                        dialog.AcceptButton = btnLoad;

                        if (dialog.ShowDialog(form) == DialogResult.OK)
                        {
                            selectedConfig = listBox.SelectedItem?.ToString();
                        }
                    }
                });
            });

            if (!string.IsNullOrEmpty(selectedConfig))
            {
                await LoadCharacterConfigInternalAsync(form, selectedConfig);
            }
        }
        catch (Exception ex)
        {
            form.Invoke((MethodInvoker)delegate
            {
                MessageBox.Show(form, $"Ошибка загрузки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }
    }

    private static async Task LoadCharacterConfigInternalAsync(CharacterCreator form, string configName)
    {
        var character = await Task.Run(() =>
        {
            string filePath = Path.Combine(CharactersPath, $"{configName}.json");
            return JsonSerializer.Deserialize<CountryCharacterConfig>(File.ReadAllText(filePath));
        });

        form.Invoke((MethodInvoker)delegate
        {
            form.IdBox.Text = character.Id;
            form.NameBox.Text = character.Name;
            form.DescBox.Text = character.Description;
            form.AdvisorCost.Text = character.AdvisorCost.ToString();
            form.AiDoBox.Text = character.AiWillDo;
            form.ExpireBox.Text = character.Expire;
            form.SpdBox.Text = character.Speed.ToString();
            form.SupplyBox.Text = character.Supply.ToString();
            form.DefBox.Text = character.Defense.ToString();
            form.AtkBox.Text = character.Attack.ToString();
            form.SkillBox.Text = character.Skill.ToString();
            form.ExpireBox.Text = character.Expire;
            form.AdvisorSlot.Text = character.AdvisorSlot.ToString();
            form.TagBox.Text = character.Tag.ToString();
            form.PercBox.Lines = character.Traits.ToArray(); 

            string bigIconPath = Path.Combine(CharactersPath, $"{character.Id}_big.png");
            string smallIconPath = Path.Combine(CharactersPath, $"{character.Id}_small.png");

            if (File.Exists(bigIconPath))
            {
                form.BigIconPanel.BackgroundImage = Image.FromFile(bigIconPath);
            }
            if (File.Exists(smallIconPath))
            {
                form.SmalIconPanel.BackgroundImage = Image.FromFile(smallIconPath);
            }

            MessageBox.Show(form, $"Персонаж {character.Name} загружен!", "Успех",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }
}