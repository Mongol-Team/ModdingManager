using ModdingManager.configs;
using ModdingManager;
using System.Text.Json;
using Microsoft.VisualBasic;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Media.Media3D;
using ModdingManager.classes.managers.gfx;

public static class WPFConfigManager
{
    private static readonly string ConfigsPath = Path.Combine("..", "..", "..", "data", "configs");
    private static readonly string CharactersPath = Path.Combine(ConfigsPath, "characters");
    private static readonly string IdeasPath = Path.Combine(ConfigsPath, "ideas");
    private static readonly string TemplatesPath = Path.Combine(ConfigsPath, "templates");
    private static readonly string CountrysPath = Path.Combine(ConfigsPath, "countrys");
    private static readonly string TechTreesPath = Path.Combine(ConfigsPath, "techtrees");

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task LoadConfigWrapper<TForm>(TForm form) where TForm : Form
    {
        try
        {
            var (configPath, configType) = GetConfigInfo<TForm>();

            if (form is System.Windows.FrameworkElement wpfForm)
            {
                wpfForm.IsEnabled = false;
                wpfForm.Cursor = System.Windows.Input.Cursors.Wait;
            }
            else if (form is Form winFormsForm)
            {
                winFormsForm.Invoke((MethodInvoker)(() =>
                {
                    winFormsForm.Enabled = false;
                    winFormsForm.Cursor = Cursors.WaitCursor;
                }));
            }

            var availableConfigs = await GetAvailableConfigsAsync(configPath);

            if (availableConfigs.Count == 0)
            {
                ShowMessage($"Нет сохранённых конфигураций {configType}", "Информация", MessageBoxImage.Information, form);
                return;
            }

            string selectedConfig = await ShowConfigSelectionDialog(form, availableConfigs, $"Выберите конфигурацию {configType}");

            if (!string.IsNullOrEmpty(selectedConfig))
            {
                if (form is TechTreeCreator techTreeForm)
                {
                    string filePath = Path.Combine(configPath, $"{selectedConfig}.tech");
                    var config = await LoadTechTreeFromArchive(filePath);
                    ApplyTechTreeConfig(techTreeForm, config);
                }
                else
                {
                    await LoadConfigInternalAsync(form, configPath, selectedConfig);
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка: {ex.Message}", "Ошибка", MessageBoxImage.Error, form);
        }
        finally
        {
            if (form is System.Windows.FrameworkElement wpfForm)
            {
                wpfForm.IsEnabled = true;
                wpfForm.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            else if (form is Form winFormsForm)
            {
                winFormsForm.Invoke((MethodInvoker)(() =>
                {
                    winFormsForm.Enabled = true;
                    winFormsForm.Cursor = Cursors.Default;
                }));
            }
        }
    }

    private static void ShowMessage(string message, string title, MessageBoxImage image, object form)
    {
        if (form is System.Windows.FrameworkElement wpfForm)
        {
            wpfForm.Dispatcher.Invoke(() =>
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, image));
        }
        else if (form is Form winFormsForm)
        {
            winFormsForm.Invoke((MethodInvoker)(() =>
                System.Windows.Forms.MessageBox.Show(winFormsForm, message, title, MessageBoxButtons.OK, GetMessageBoxIcon(image))));
        }
    }

    private static MessageBoxIcon GetMessageBoxIcon(MessageBoxImage image)
    {
        return image switch
        {
            MessageBoxImage.Information => MessageBoxIcon.Information,
            MessageBoxImage.Error => MessageBoxIcon.Error,
            MessageBoxImage.Warning => MessageBoxIcon.Warning,
            _ => MessageBoxIcon.None
        };
    }

    private static async Task<string> ShowWpfSelectionDialog(System.Windows.FrameworkElement owner, List<string> configs, string title)
    {
        var tcs = new TaskCompletionSource<string>();

        owner.Dispatcher.Invoke(() =>
        {
            var dialog = new System.Windows.Window
            {
                Title = title,
                Width = 400,
                Height = 500,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            var listBox = new System.Windows.Controls.ListBox
            {
                ItemsSource = configs,
                FontSize = 14,
                SelectionMode = System.Windows.Controls.SelectionMode.Single
            };

            var button = new System.Windows.Controls.Button
            {
                Content = "Загрузить",
                Height = 40,
                IsDefault = true
            };

            button.Click += (s, e) =>
            {
                tcs.SetResult(listBox.SelectedItem?.ToString());
                dialog.Close();
            };

            var stackPanel = new System.Windows.Controls.StackPanel();
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(button);

            dialog.Content = stackPanel;
            dialog.Owner = System.Windows.Window.GetWindow(owner);
            dialog.ShowDialog();
        });

        return await tcs.Task;
    }

    private static async Task<string> ShowWinFormsSelectionDialog(Form owner, List<string> configs, string title)
    {
        string selectedConfig = null;

        await Task.Run(() =>
        {
            owner.Invoke((MethodInvoker)(() =>
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

                    if (dialog.ShowDialog(owner) == DialogResult.OK)
                    {
                        selectedConfig = listBox.SelectedItem?.ToString();
                    }
                }
            }));
        });

        return selectedConfig;
    }
    public static async Task<bool> SaveConfig<TForm>(TForm form, string configName) where TForm : Form
    {
        try
        {
            if (form is TechTreeCreator techTreeForm)
            {
                var config = techTreeForm.CurrentTechTree;
                foreach (var item in config.Items)
                {
                    if (item.Image != null)
                    {
                        item.ImageData = null;
                    }
                }

                string filePath = Path.Combine(TechTreesPath, $"{configName}.tech");
                await SaveTechTreeToArchive(config, filePath);
                return true;
            }
            else
            {
                var (configPath, _) = GetConfigInfo<TForm>();
                Directory.CreateDirectory(configPath);
                object config = form switch
                {
                    CountryCreator countryForm => CreateCountryConfig(countryForm),
                    CharacterCreator characterForm => CreateCharacterConfig(characterForm),
                    IdeaCreator ideaForm => CreateIdeaConfig(ideaForm),
                    TemplateCreator templateForm => CreateTemplateConfig(templateForm),
                    ModifierCreator modifierForm => CreateModifierConfig(modifierForm),
                    _ => throw new NotSupportedException($"Тип формы {typeof(TForm)} не поддерживается")
                };
                string filePath = Path.Combine(configPath, $"{configName}.json");
                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(filePath, json);
                if (form is CharacterCreator charForm && charForm.CurrentConfig != null)
                {
                    //SaveCharacterImages(charForm, configName);
                }
                else if (form is IdeaCreator ideaForm && ideaForm.ImagePanel.BackgroundImage != null)
                {
                    string iconPath = Path.Combine(configPath, $"{configName}.png");
                    ideaForm.ImagePanel.BackgroundImage.Save(iconPath, System.Drawing.Imaging.ImageFormat.Png);
                }
                System.Windows.Forms.MessageBox.Show($"Конфигурация '{configName}' успешно сохранена!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
        }
        catch (Exception ex)
        {
            if (form is System.Windows.Threading.DispatcherObject wpfForm)
            {
                wpfForm.Dispatcher.Invoke(() =>
                    System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка"));
            }
            else if (form is Form winFormsForm)
            {
                winFormsForm.Invoke((MethodInvoker)(() =>
                    System.Windows.Forms.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка")));
            }

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
            nameof(TechTreeCreator) => (TechTreesPath, "дерева технологий"),
            nameof(ModifierCreator) => (Path.Combine(ConfigsPath, "modifiers"), "модификатора"),
            nameof(TemplateCreator) => (TemplatesPath, "шаблона"),
            _ => throw new NotSupportedException($"Тип формы {typeof(TForm)} не поддерживается")
        };
    }
    #region TechTree Config Methods
    private static TechTreeConfig CreateTechTreeConfig(TechTreeCreator form)
    {
        return form.CurrentTechTree;
    }

    private static void ApplyTechTreeConfig(TechTreeCreator form, TechTreeConfig config)
    {
        // Устанавливаем конфиг в WPF свойство
        form.CurrentTechTree = config;

        // Используем Dispatcher для WPF
        form.Dispatcher.Invoke(() =>
        {
            form.RefreshTechTreeView();

            // Для WPF MessageBox используем System.Windows.MessageBox
            System.Windows.MessageBox.Show(
                $"Дерево технологий '{config.Name}' успешно загружено",
                "Успех",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information
            );
        });
    }
    #endregion
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
    #endregion
    #region Template Config Methods
    private static TemplateConfig CreateTemplateConfig(TemplateCreator form)
    {
        return form.CurrentConfig;
    }

    private static void ApplyTemplateConfig(TemplateCreator form, TemplateConfig config)
    {
        form.NameBox.Text = config.Name;
        form.IsLocked.Checked = config.IsLocked ?? false;
        form.GroupNameBox.Text = config.Namespace;
        form.CanRecrutingLocked.Checked = config.AllowTraining ?? false;
        form.ModelIDBox.Text = config.ModelName;
        form.DivisionCapBox.Text = config.DivisionCap.ToString();
        form.PriorityBox.Text = config.Priority.ToString();
        form.CustomIconBox.Text = config.CustomIconId.ToString();
        form.OOBTagBox.Text = config.OOBFileName;
        form.OOBYearBox.Text = config.OOBFileYear.ToString();
        form.CurrentConfig = config;
    
        foreach (var item in config.SupportItems)
        {
            var panel = FindPanel(form, "Support", item.X, item.Y);
            if (panel != null)
            {
                AddImageToPanel(panel, item.Name);
            }
        }

        foreach (var item in config.BrigadeItems)
        {
            var panel = FindPanel(form, "Brigade", item.X, item.Y);
            if (panel != null)
            {
                AddImageToPanel(panel, item.Name);
            }
        }
    }

    

    private static Panel FindPanel(TemplateCreator form, string prefix, int x, int y)
    {
        string panelName = $"{prefix}{x}{y}";
        Panel panel = null;
        if (prefix == "Support")
        {
            panel = form.Controls.OfType<Panel>()
            .FirstOrDefault(p => p.Name == "SupportBG");
        }else if (prefix == "Brigade")
        {
            panel = form.Controls.OfType<Panel>()
            .FirstOrDefault(p => p.Name == "BrigadeBG");
        }
        var result = panel.Controls.OfType<Panel>()
               .FirstOrDefault(p => p.Name == panelName);
        return result;
    }

    private static void AddImageToPanel(Panel panel, string unitName)
    {
        var image = ImageManager.FindUnitIcon(unitName);
        if (image != null)
        {
            panel.BackgroundImage = image;
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

            return Directory.EnumerateFiles(configPath)
             .Where(f => f.EndsWith(".json") || f.EndsWith(".tech"))
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
            if (form is TechTreeCreator techTreeForm)
            {
                var config = JsonSerializer.Deserialize<TechTreeConfig>(json);
                ApplyTechTreeConfig(techTreeForm, config);
            }
            else
            {
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
                        case TemplateCreator templateForm:
                            var templateConfig = JsonSerializer.Deserialize<TemplateConfig>(json);
                            ApplyTemplateConfig(templateForm, templateConfig);
                            break;

                    }

                    System.Windows.Forms.MessageBox.Show(form, $"Конфигурация '{configName}' успешно загружена",
                                  "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
        });
    }
    #region Country Config Methods
    private static CountryConfig CreateCountryConfig(CountryCreator form)
    {
        var conf = new CountryConfig
        {
            Tag = form.TagBox.Text,
            Capital = int.TryParse(form.CapitalBox.Text, out var capital) ? capital : 0,
            GraphicalCulture = form.GrapficalCultureBox.Text,
            //Color = $"{form.CountryColorDialog.Color.R} {form.CountryColorDialog.Color.G} {form.CountryColorDialog.Color.B}",
            //Technologies = form.TechBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Convoys = int.TryParse(form.ConvoyBox.Text, out var convoys) ? convoys : 0,
            OOB = form.StartOOBBox.Text,
            ResearchSlots = int.TryParse(form.ResearchSlotBox.Text, out var slots) ? slots : 0,
            Stab = int.Parse(form.StabBox.Text),
            WarSup = int.Parse(form.WarSupportBox.Text),
            Name = form.CountryNameBox.Text,
            RulingParty = form.RullingPartyBox.SelectedItem?.ToString(),
            //LastElection = form.LastElectionBox.Text,
            ElectionFrequency = int.TryParse(form.ElectionFreqBox.Text, out var freq) ? freq : 0,
            ElectionsAllowed = form.IsElectionAllowedBox.Checked,
            Ideas = form.StartIdeasBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Characters = form.RecruitBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            StateCores = ParseStates(form.CountryStatesBox.Text)
        };
        return conf;
    }
    private static void ApplyCountryConfig(CountryCreator form, CountryConfig config)
    {
        form.TagBox.Text = config.Tag;
        form.CapitalBox.Text = config.Capital.ToString();
        form.GrapficalCultureBox.Text = config.GraphicalCulture;

        //var colorParts = config.Color.Split(' ');
        //if (colorParts.Length == 3)
        //{
        //    form.CountryColorDialog.Color = Color.FromArgb(
        //        int.Parse(colorParts[0]),
        //        int.Parse(colorParts[1]),
        //        int.Parse(colorParts[2]));
        //    form.ColorPickerButton.BackColor = form.CountryColorDialog.Color;
        //}

        //form.TechBox.Text = string.Join(Environment.NewLine, config.Technologies);
        //form.ConvoyBox.Text = config.Convoys.ToString();
        //form.StartOOBBox.Text = config.OOB;
        //form.ResearchSlotBox.Text = config.ResearchSlots.ToString();
        //form.WarSupportBox.Text = config.WarSup.ToString();
        //form.StabBox.Text = config.Stab.ToString();
        //form.CountryNameBox.Text = config.Name;
        //form.RullingPartyBox.SelectedItem = config.RulingParty;
        //form.LastElectionBox.Text = config.LastElection;
        //form.ElectionFreqBox.Text = config.ElectionFrequency.ToString();
        //form.IsElectionAllowedBox.Checked = config.ElectionsAllowed;

        //form.NeutralPopularBox.Text = config.NeutralityPopularity.ToString();
        //form.FascismPopularBox.Text = config.FascismPopularity.ToString();
        //form.CommunismPopularBox.Text = config.CommunismPopularity.ToString();
        //form.DemocraticPopularBox.Text = config.DemocraticPopularity.ToString();

        //form.StartIdeasBox.Text = string.Join(Environment.NewLine, config.Ideas);
        form.RecruitBox.Text = string.Join(Environment.NewLine, config.Characters);
        form.CountryStatesBox.Text = string.Join(Environment.NewLine,
            config.StateCores.Select(s => $"{s.Key}:{(s.Value ? "1" : "0")}"));
    }
    #endregion
    #region Character Config Methods
    private static CountryCharacterConfig CreateCharacterConfig(CharacterCreator form)
    {
        int SafeParseInt(string text)
        {
            return int.TryParse(text, out var result) ? result : 0;
        }

        return new CountryCharacterConfig
        {
            Id = form.IdBox.Text?.Trim() ?? "",
            Name = form.NameBox.Text?.Trim() ?? "",
            Description = form.DescBox.Text?.Trim() ?? "",
            Tag = form.TagBox.Text?.Trim() ?? "",
            Skill = SafeParseInt(form.SkillBox.Text),
            Attack = SafeParseInt(form.AtkBox.Text),
            Defense = SafeParseInt(form.DefBox.Text),
            Supply = SafeParseInt(form.SupplyBox.Text),
            Speed = SafeParseInt(form.SpdBox.Text),
            AdvisorSlot = form.AdvisorSlot.Text?.Trim() ?? "",
            Ideology = form.IdeologyBox.Text?.Trim() ?? "",
            AdvisorCost = SafeParseInt(form.AdvisorCost.Text),
            AiWillDo = form.AiDoBox.Text?.Trim() ?? "",
            Expire = form.ExpireTimePicker.Value.ToString("yyyy.MM.dd"),
            Types = form.CharTypesBox.Text?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            Traits = form.PercBox.Text?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
       
        };
    }

    private static void ApplyCharacterConfig(CharacterCreator form, CountryCharacterConfig config)
    {
        form.IdBox.Text = config.Id;
        form.NameBox.Text = config.Name;
        form.DescBox.Text = config.Description;
        form.AdvisorCost.Text = config.AdvisorCost.ToString();
        form.AiDoBox.Text = config.AiWillDo;
        form.ExpireTimePicker.Text = config.Expire;
        form.SpdBox.Text = config.Speed.ToString();
        form.SupplyBox.Text = config.Supply.ToString();
        form.DefBox.Text = config.Defense.ToString();
        form.AtkBox.Text = config.Attack.ToString();
        form.SkillBox.Text = config.Skill.ToString();
        form.AdvisorSlot.Text = config.AdvisorSlot;
        form.TagBox.Text = config.Tag;
        form.PercBox.Lines = config.Traits.ToArray();
        form.CharTypesBox.Lines = config.Types.ToArray();
        form.IdeologyBox.SelectedItem = config.Ideology;
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

            System.Windows.Forms.MessageBox.Show("Сохранение отменено!");
            return;
        }
        SaveConfig(form, fileName);
    }
   


    private static async Task<bool> SaveTechTreeToArchive(TechTreeConfig config, string filePath)
    {
        using (var memoryStream = new MemoryStream())
        {
            string json = JsonSerializer.Serialize(config, JsonOptions);

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var jsonEntry = archive.CreateEntry("config.json");
                using (var writer = new StreamWriter(jsonEntry.Open()))
                {
                    await writer.WriteAsync(json);
                }

                foreach (var item in config.Items.Where(i => i.Image != null))
                {
                    var imageEntry = archive.CreateEntry($"images/{item.Id}.png");
                    using (var stream = imageEntry.Open())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)item.Image));
                        encoder.Save(stream);
                    }
                }
            }

            await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
            return true;
        }
    }

    private static async Task<TechTreeConfig> LoadTechTreeFromArchive(string filePath)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Open))
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
        {
            var jsonEntry = archive.GetEntry("config.json");
            using (var reader = new StreamReader(jsonEntry.Open()))
            {
                string json = await reader.ReadToEndAsync();
                var config = JsonSerializer.Deserialize<TechTreeConfig>(json);

                foreach (var item in config.Items)
                {
                    var imageEntry = archive.GetEntry($"images/{item.Id}.png");
                    if (imageEntry != null)
                    {
                        using (var stream = imageEntry.Open())
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            item.Image = bitmap;
                        }
                    }
                }

                return config;
            }
        }
    }
}