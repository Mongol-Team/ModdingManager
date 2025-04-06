using ModdingManager.configs;
using ModdingManager;
using System.Text.Json;
using Microsoft.VisualBasic;

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

    public static async Task LoadConfigAsync<TForm>(TForm form) where TForm : Form
    {
        try
        {
            // Блокируем UI на время загрузки
            form.Invoke((MethodInvoker)(() =>
            {
                form.Enabled = false;
                form.Cursor = Cursors.WaitCursor;
            }));

            // Определяем путь и тип конфигурации на основе типа формы
            var (configPath, configType) = GetConfigInfo<TForm>();

            // Асинхронно получаем список доступных конфигов
            var availableConfigs = await GetAvailableConfigsAsync(configPath);

            if (availableConfigs.Count == 0)
            {
                form.Invoke((MethodInvoker)(() =>
                    MessageBox.Show(form, $"Нет сохранённых конфигураций {configType}", "Информация",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information)));
                return;
            }

            // Диалог выбора конфига
            string selectedConfig = await ShowConfigSelectionDialog(form, availableConfigs, $"Выберите конфигурацию {configType}");

            if (!string.IsNullOrEmpty(selectedConfig))
            {
                await LoadConfigInternalAsync(form, configPath, selectedConfig);
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

    public static bool SaveConfig<TForm>(TForm form, string configName) where TForm : Form
    {
        try
        {
            var (configPath, _) = GetConfigInfo<TForm>();
            Directory.CreateDirectory(configPath);

            object config = form switch
            {
                CountryCreator countryForm => CreateCountryConfig(countryForm),
                CharacterCreator characterForm => CreateCharacterConfig(characterForm),
                IdeaCreator ideaForm => CreateIdeaConfig(ideaForm),
                ModifierCreator modifierForm => CreateModifierConfig(modifierForm),
                _ => throw new NotSupportedException($"Тип формы {typeof(TForm)} не поддерживается")
            };

            string filePath = Path.Combine(configPath, $"{configName}.json");
            string json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(filePath, json);

            if (form is CharacterCreator charForm && charForm.currentCharacter != null)
            {
                SaveCharacterImages(charForm, configName);
            }
            else if (form is IdeaCreator ideaForm && ideaForm.ImagePanel.BackgroundImage != null)
            {
                string iconPath = Path.Combine(configPath, $"{configName}.png");
                ideaForm.ImagePanel.BackgroundImage.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            MessageBox.Show($"Конфигурация '{configName}' успешно сохранена!", "Успех",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения конского фига: {ex.Message}", "Ошибка",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private static (string Path, string Type) GetConfigInfo<TForm>() where TForm : Form
    {
        return typeof(TForm).Name switch
        {
            nameof(CountryCreator) => (CountrysPath, "страны"),
            nameof(CharacterCreator) => (CharactersPath, "персонажа"),
            nameof(IdeaCreator) => (IdeasPath, "идеи"),
            nameof(ModifierCreator) => (Path.Combine(ConfigsPath, "modifiers"), "модификатора"),
            _ => throw new NotSupportedException($"Тип формы {typeof(TForm)} не поддерживается")
        };
    }

    #region Modifier Config Methods
    private static ModifierConfig CreateModifierConfig(ModifierCreator form)
    {
        return new ModifierConfig
        {
            Id = form.IdBox.Text.Trim(),
            Name = form.NameBox.Text.Trim(),
            Description = form.DescBox.Text.Trim(),
            Tag = form.TagBox.Text.Trim(),
            Modifiers = form.ModifBox.Text,
            Type = form.TypeBox.SelectedItem?.ToString(),
            Variation = form.VariationBox.SelectedItem?.ToString(),
            EnableTrigger = form.EnableBox.Text,
            RemovalTrigger = form.RemovalTriggerBox.Text,
            Trigger = form.TriggerBox.Text,
            AttackerEffect = form.AttackerEffectBox.Text,
            PowerBalance = form.PowerBalanceBox.Text,
            RelationTrigger = form.RelationTrigerBox.Text,
            IsTrading = form.IsTradeBox.Checked,
            Days = int.TryParse(form.DaysBox.Text, out var days) ? days : 0,
            Decay = int.TryParse(form.DecayBox.Text, out var decay) ? decay : 0,
            MinTrust = int.TryParse(form.MinTrustBox.Text, out var minTrust) ? minTrust : 0,
            MaxTrust = int.TryParse(form.MaxTrustBox.Text, out var maxTrust) ? maxTrust : 0,
            Value = int.TryParse(form.ValueBox.Text, out var value) ? value : 0,
            IconPath = "" // будет заполнено при сохранении изображения
        };
    }

    private static void ApplyModifierConfig(ModifierCreator form, ModifierConfig config)
    {
        form.IdBox.Text = config.Id;
        form.NameBox.Text = config.Name;
        form.DescBox.Text = config.Description;
        form.TagBox.Text = config.Tag;
        form.ModifBox.Text = config.Modifiers;

        form.TypeBox.SelectedItem = config.Type;
        form.VariationBox.SelectedItem = config.Variation;

        form.EnableBox.Text = config.EnableTrigger;
        form.RemovalTriggerBox.Text = config.RemovalTrigger;
        form.TriggerBox.Text = config.Trigger;
        form.AttackerEffectBox.Text = config.AttackerEffect;
        form.PowerBalanceBox.Text = config.PowerBalance;
        form.RelationTrigerBox.Text = config.RelationTrigger;

        form.IsTradeBox.Checked = config.IsTrading;
        form.DaysBox.Text = config.Days.ToString();
        form.DecayBox.Text = config.Decay.ToString();
        form.MinTrustBox.Text = config.MinTrust.ToString();
        form.MaxTrustBox.Text = config.MaxTrust.ToString();
        form.ValueBox.Text = config.Value.ToString();

        if (!string.IsNullOrEmpty(config.IconPath) && File.Exists(config.IconPath))
        {
            form.ImagePanel.BackgroundImage = Image.FromFile(config.IconPath);
        }
    }

    private static void SaveModifierImage(ModifierCreator form, string configName)
    {
        if (form.ImagePanel.BackgroundImage != null)
        {
            string iconPath = Path.Combine(ConfigsPath, "modifiers", $"{configName}.png");
            form.ImagePanel.BackgroundImage.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
    #endregion

    private static async Task<List<string>> GetAvailableConfigsAsync(string configPath)
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
                return new List<string>();
            }

            return Directory.GetFiles(configPath, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();
        });
    }

    private static async Task<string> ShowConfigSelectionDialog(Form parentForm, List<string> configs, string title)
    {
        string selectedConfig = null;

        await Task.Run(() =>
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                using (var dialog = new Form()
                {
                    Text = title,
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

                    var btnLoad = new Button
                    {
                        Text = "Загрузить",
                        Dock = DockStyle.Bottom,
                        Height = 40,
                        DialogResult = DialogResult.OK
                    };

                    dialog.Controls.Add(btnLoad);
                    dialog.Controls.Add(listBox);
                    dialog.AcceptButton = btnLoad;

                    if (dialog.ShowDialog(parentForm) == DialogResult.OK)
                    {
                        selectedConfig = listBox.SelectedItem?.ToString();
                    }
                }
            }));
        });

        return selectedConfig;
    }

    private static async Task LoadConfigInternalAsync<TForm>(TForm form, string configPath, string configName) where TForm : Form
    {
        string filePath = Path.Combine(configPath, $"{configName}.json");

        await Task.Run(() =>
        {
            string json = File.ReadAllText(filePath);

            form.Invoke((MethodInvoker)(() =>
            {
                switch (form)
                {
                    case CountryCreator countryForm:
                        var countryConfig = JsonSerializer.Deserialize<CountryConfig>(json);
                        ApplyCountryConfig(countryForm, countryConfig);
                        break;

                    case CharacterCreator characterForm:
                        var characterConfig = JsonSerializer.Deserialize<CountryCharacterConfig>(json);
                        ApplyCharacterConfig(characterForm, characterConfig);
                        break;

                    case IdeaCreator ideaForm:
                        var ideaConfig = JsonSerializer.Deserialize<IdeaConfig>(json);
                        ApplyIdeaConfig(ideaForm, ideaConfig);
                        break;
                    case ModifierCreator modifierForm:
                        var modifierConfig = JsonSerializer.Deserialize<ModifierConfig>(json);
                        ApplyModifierConfig(modifierForm, modifierConfig);
                        break;
                }

                MessageBox.Show(form, $"Конфигурация '{configName}' успешно загружена",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        });
    }

    #region Country Config Methods
    private static CountryConfig CreateCountryConfig(CountryCreator form)
    {
        return new CountryConfig
        {
            Tag = form.TagBox.Text,
            Capital = int.TryParse(form.CapitalBox.Text, out var capital) ? capital : 0,
            GraphicalCulture = form.GrapficalCultureBox.Text,
            Color = $"{form.CountryColorDialog.Color.R} {form.CountryColorDialog.Color.G} {form.CountryColorDialog.Color.B}",
            Technologies = form.TechBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Convoys = int.TryParse(form.ConvoyBox.Text, out var convoys) ? convoys : 0,
            OOB = form.StartOOBBox.Text,
            ResearchSlots = int.TryParse(form.ResearchSlotBox.Text, out var slots) ? slots : 0,
            Stab = int.Parse(form.StabBox.Text),
            WarSup = int.Parse(form.WarSupportBox.Text),
            Name = form.CountryNameBox.Text,
            RulingParty = form.RullingPartyBox.SelectedItem?.ToString(),
            LastElection = form.LastElectionBox.Text,
            ElectionFrequency = int.TryParse(form.ElectionFreqBox.Text, out var freq) ? freq : 0,
            ElectionsAllowed = form.IsElectionAllowedBox.Checked,
            NeutralityPopularity = int.TryParse(form.NeutralPopularBox.Text, out var neutral) ? neutral : 0,
            FascismPopularity = int.TryParse(form.FascismPopularBox.Text, out var fascism) ? fascism : 0,
            CommunismPopularity = int.TryParse(form.CommunismPopularBox.Text, out var communism) ? communism : 0,
            DemocraticPopularity = int.TryParse(form.DemocraticPopularBox.Text, out var democratic) ? democratic : 0,
            Ideas = form.StartIdeasBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Characters = form.RecruitBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            States = ParseStates(form.CountryStatesBox.Text)
        };
    }

    private static void ApplyCountryConfig(CountryCreator form, CountryConfig config)
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
            form.ColorPickerButton.BackColor = form.CountryColorDialog.Color;
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
    }
    #endregion

    #region Character Config Methods
    private static CountryCharacterConfig CreateCharacterConfig(CharacterCreator form)
    {
        return new CountryCharacterConfig
        {
            Id = form.IdBox.Text.Trim(),
            Name = form.NameBox.Text.Trim(),
            Description = form.DescBox.Text.Trim(),
            Tag = form.TagBox.Text.Trim(),
            Skill = int.Parse(form.SkillBox.Text),
            Attack = int.Parse(form.AtkBox.Text),
            Defense = int.Parse(form.DefBox.Text),
            Supply = int.Parse(form.SupplyBox.Text),
            Speed = int.Parse(form.SpdBox.Text),
            AdvisorSlot = form.AdvisorSlot.Text,
            AdvisorCost = int.Parse(form.AdvisorCost.Text),
            AiWillDo = form.AiDoBox.Text.Trim(),
            Expire = form.ExpireBox.Text.Trim(),
            Types = form.CharTypesBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Traits = form.PercBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            BigIconPath = form.currentCharacter?.BigIconPath ?? "",
            SmallIconPath = form.currentCharacter?.SmallIconPath ?? ""
        };
    }
    private static void ApplyCharacterConfig(CharacterCreator form, CountryCharacterConfig config)
    {
        form.IdBox.Text = config.Id;
        form.NameBox.Text = config.Name;
        form.DescBox.Text = config.Description;
        form.AdvisorCost.Text = config.AdvisorCost.ToString();
        form.AiDoBox.Text = config.AiWillDo;
        form.ExpireBox.Text = config.Expire;
        form.SpdBox.Text = config.Speed.ToString();
        form.SupplyBox.Text = config.Supply.ToString();
        form.DefBox.Text = config.Defense.ToString();
        form.AtkBox.Text = config.Attack.ToString();
        form.SkillBox.Text = config.Skill.ToString();
        form.AdvisorSlot.Text = config.AdvisorSlot;
        form.TagBox.Text = config.Tag;
        form.PercBox.Lines = config.Traits.ToArray();

        string bigIconPath = Path.Combine(CharactersPath, $"{config.Id}_big.png");
        string smallIconPath = Path.Combine(CharactersPath, $"{config.Id}_small.png");

        if (File.Exists(bigIconPath))
        {
            form.BigIconPanel.BackgroundImage = Image.FromFile(bigIconPath);
        }
        if (File.Exists(smallIconPath))
        {
            form.SmalIconPanel.BackgroundImage = Image.FromFile(smallIconPath);
        }
    }

    private static void SaveCharacterImages(CharacterCreator form, string configName)
    {
        if (form.BigIconPanel.BackgroundImage != null)
        {
            string bigIconPath = Path.Combine(CharactersPath, $"{configName}_big.png");
            form.BigIconPanel.BackgroundImage.Save(bigIconPath, System.Drawing.Imaging.ImageFormat.Png);
        }

        if (form.SmalIconPanel.BackgroundImage != null)
        {
            string smallIconPath = Path.Combine(CharactersPath, $"{configName}_small.png");
            form.SmalIconPanel.BackgroundImage.Save(smallIconPath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
    #endregion

    #region Idea Config Methods
    private static IdeaConfig CreateIdeaConfig(IdeaCreator form)
    {
        return new IdeaConfig
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
    }

    private static void ApplyIdeaConfig(IdeaCreator form, IdeaConfig config)
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

        string iconPath = Path.Combine(IdeasPath, $"{config.Id}.png");
        if (File.Exists(iconPath))
        {
            form.ImagePanel.BackgroundImage = Image.FromFile(iconPath);
        }
    }
    #endregion

    #region Helper Methods
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
    #endregion
    public static void SaveConfigWrapper<TForm>(TForm form) where TForm : Form
    {
        string fileName = Interaction.InputBox(
     "Введите имя файла для сохранения (без .json):",
     "Сохранение конфига");

        if (string.IsNullOrWhiteSpace(fileName))
        {
            MessageBox.Show("Сохранение отменено!");
            return;
        }
        SaveConfig(form, fileName);
    }
}