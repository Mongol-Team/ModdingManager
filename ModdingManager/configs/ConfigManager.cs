using ModdingManager;
using ModdingManager.configs;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace ModdingManager.configs
{
    public static class ConfigManager
    {
        private static readonly string ConfigsPath = Path.Combine("..", "..", "..", "data", "configs");
        private static readonly string CharactersPath = Path.Combine(ConfigsPath, "characters");
        private static readonly string IdeasPath = Path.Combine(ConfigsPath, "ideas");
        private static readonly string CountrysPath = Path.Combine(ConfigsPath, "countrys");
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static async Task LoadCountryConfigAsync(CountryCreator form)
        {
            try
            {
                // Блокируем UI на время загрузки
                form.Invoke((MethodInvoker)(() =>
                {
                    form.Enabled = false;
                    form.Cursor = Cursors.WaitCursor;
                }));

                // Асинхронно получаем список доступных конфигов
                var availableConfigs = await GetAvailableCountryConfigsAsync();

                if (availableConfigs.Count == 0)
                {
                    form.Invoke((MethodInvoker)(() =>
                        MessageBox.Show(form, "Нет сохранённых конфигураций стран", "Информация",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    return;
                }

                // Диалог выбора конфига
                string selectedConfig = await ShowConfigSelectionDialog(form, availableConfigs);

                if (!string.IsNullOrEmpty(selectedConfig))
                {
                    await LoadCountryConfigInternalAsync(form, selectedConfig);
                }
            }
            catch (Exception ex)
            {
                form.Invoke((MethodInvoker)(() =>
                    MessageBox.Show(form, $"Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
            finally
            {
                // Восстанавливаем UI
                form.Invoke((MethodInvoker)(() =>
                {
                    form.Enabled = true;
                    form.Cursor = Cursors.Default;
                }));
            }
        }

        public static async Task<List<string>> GetAvailableCountryConfigsAsync()
        {
            return await Task.Run(() =>
                Directory.GetFiles(CountrysPath, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList());
        }

        private static async Task<string> ShowConfigSelectionDialog(CountryCreator form, List<string> configs)
        {
            string selectedConfig = null;

            await Task.Run(() =>
            {
                form.Invoke((MethodInvoker)(() =>
                {
                    using (var dialog = new Form()
                    {
                        Text = "Выберите конфигурацию страны",
                        Width = 400,
                        Height = 500,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog
                    })
                    {
                        var listBox = new ListBox
                        {
                            Dock = DockStyle.Fill,
                            DataSource = configs,
                            Font = new Font("Arial", 11),
                            SelectionMode = SelectionMode.One
                        };

                        var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 45 };
                        var btnLoad = new Button
                        {
                            Text = "Загрузить",
                            DialogResult = DialogResult.OK,
                            Width = 100,
                            Anchor = AnchorStyles.Right
                        };

                        btnPanel.Controls.Add(btnLoad);
                        dialog.Controls.Add(listBox);
                        dialog.Controls.Add(btnPanel);
                        dialog.AcceptButton = btnLoad;

                        if (dialog.ShowDialog(form) == DialogResult.OK)
                        {
                            selectedConfig = listBox.SelectedItem?.ToString();
                        }
                    }
                }));
            });

            return selectedConfig;
        }

        private static async Task LoadCountryConfigInternalAsync(CountryCreator form, string configName)
        {
            string filePath = Path.Combine(CountrysPath, $"{configName}.json");

            var config = await Task.Run(() =>
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<CountryConfig>(json);
            });

            form.Invoke((MethodInvoker)(() =>
            {
              
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
                form.ColorPickerButton.BackColor = Color.FromArgb(
                        int.Parse(colorParts[0]),
                        int.Parse(colorParts[1]),
                        int.Parse(colorParts[2]));
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
                MessageBox.Show(form, $"Конфигурация '{configName}' успешно загружена",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        private static Dictionary<int, bool> ParseStates(string statesText)
        {
            return statesText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(':'))
                .Where(parts => parts.Length == 2 && int.TryParse(parts[0], out _))
                .ToDictionary(
                    parts => int.Parse(parts[0]),
                    parts => parts[1] == "1"
                );
        }

        public static void SaveCountryConfig(CountryCreator form, string configName)
        {
            try
            {
                Directory.CreateDirectory(CountrysPath);

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

                string filePath = Path.Combine(CountrysPath, $"{configName}.json");
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

        public static bool SaveIdeaConfig(IdeaCreator form, string configName)
        {
            try
            {
                Directory.CreateDirectory(IdeasPath);

                var config = new IdeaConfig
                {
                    Id = form.IdBox.Text.Trim(),
                    Name = form.NameBox.Text.Trim(),
                    Description = form.DescBox.Text.Trim(),
                    Tag = form.TagBox.Text.Trim(),
                    Modifiers = form.ModifBox.Text,
                    RemovalCost = form.RemovalCostBox.Text.Trim(),
                    Available = form.AvaibleBox.Text,
                    AvailableCivilWar = form.AvaibleCivBox.Text,
                    OnAdd = form.OnAddBox.Text
                };

                string filePath = Path.Combine(IdeasPath, $"{configName}.json");
                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(filePath, json);

                // Сохраняем иконку, если она есть
                if (form.ImagePanel.BackgroundImage != null)
                {
                    string iconPath = Path.Combine(IdeasPath, $"{configName}.png");
                    form.ImagePanel.BackgroundImage.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                MessageBox.Show($"Конфигурация '{configName}' успешно сохранена!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения конфигурации: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task LoadIdeaConfigAsync(IdeaCreator form)
        {
            try
            {
                // Блокируем UI на время загрузки
                form.Invoke((MethodInvoker)(() =>
                {
                    form.Enabled = false;
                    form.Cursor = Cursors.WaitCursor;
                }));

                // Асинхронно получаем список доступных конфигов
                var availableConfigs = await GetAvailableIdeaConfigsAsync();

                if (availableConfigs.Count == 0)
                {
                    form.Invoke((MethodInvoker)(() =>
                        MessageBox.Show(form, "Нет сохранённых конфигураций идей", "Информация",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    return;
                }

                // Диалог выбора конфига
                string selectedConfig = await ShowIdeaConfigSelectionDialog(form, availableConfigs);

                if (!string.IsNullOrEmpty(selectedConfig))
                {
                    await LoadIdeaConfigInternalAsync(form, selectedConfig);
                }
            }
            catch (Exception ex)
            {
                form.Invoke((MethodInvoker)(() =>
                    MessageBox.Show(form, $"Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
            finally
            {
                // Восстанавливаем UI
                form.Invoke((MethodInvoker)(() =>
                {
                    form.Enabled = true;
                    form.Cursor = Cursors.Default;
                }));
            }
        }

        public static async Task<List<string>> GetAvailableIdeaConfigsAsync()
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(IdeasPath))
                {
                    Directory.CreateDirectory(IdeasPath);
                    return new List<string>();
                }

                return Directory.GetFiles(IdeasPath, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();
            });
        }

        private static async Task<string> ShowIdeaConfigSelectionDialog(IdeaCreator form, List<string> configs)
        {
            string selectedConfig = null;

            await Task.Run(() =>
            {
                form.Invoke((MethodInvoker)(() =>
                {
                    using (var dialog = new Form()
                    {
                        Text = "Выберите конфигурацию идеи",
                        Width = 400,
                        Height = 500,
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog
                    })
                    {
                        var listBox = new ListBox
                        {
                            Dock = DockStyle.Fill,
                            DataSource = configs,
                            Font = new Font("Arial", 11),
                            SelectionMode = SelectionMode.One
                        };

                        var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 45 };
                        var btnLoad = new Button
                        {
                            Text = "Загрузить",
                            DialogResult = DialogResult.OK,
                            Width = 100,
                            Anchor = AnchorStyles.Right
                        };

                        btnPanel.Controls.Add(btnLoad);
                        dialog.Controls.Add(listBox);
                        dialog.Controls.Add(btnPanel);
                        dialog.AcceptButton = btnLoad;

                        if (dialog.ShowDialog(form) == DialogResult.OK)
                        {
                            selectedConfig = listBox.SelectedItem?.ToString();
                        }
                    }
                }));
            });

            return selectedConfig;
        }

        private static async Task LoadIdeaConfigInternalAsync(IdeaCreator form, string configName)
        {
            var config = await Task.Run(() =>
            {
                string filePath = Path.Combine(IdeasPath, $"{configName}.json");
                return JsonSerializer.Deserialize<IdeaConfig>(File.ReadAllText(filePath));
            });

            form.Invoke((MethodInvoker)(() =>
            {
                form.IdBox.Text = config.Id;
                form.NameBox.Text = config.Name;
                form.DescBox.Text = config.Description;
                form.TagBox.Text = config.Tag;
                form.ModifBox.Text = config.Modifiers;
                form.RemovalCostBox.Text = config.RemovalCost;
                form.AvaibleBox.Text = config.Available;
                form.AvaibleCivBox.Text = config.AvailableCivilWar;
                form.OnAddBox.Text = config.OnAdd;

                // Загружаем иконку, если она есть
                string iconPath = Path.Combine(IdeasPath, $"{configName}.png");
                if (File.Exists(iconPath))
                {
                    form.ImagePanel.BackgroundImage = Image.FromFile(iconPath);
                }

                MessageBox.Show(form, $"Конфигурация '{configName}' успешно загружена",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }
    }
}