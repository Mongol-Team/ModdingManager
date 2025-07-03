using ModdingManager.classes.gfx;
using ModdingManager.configs;
using ModdingManager.managers.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class TemplateCreator : Form
    {
        public static List<RegimentConfig> AvaibleRegiments = new List<RegimentConfig>();
        public static List<int> AvaibleTypes = new List<int>([0, 1]);
        public TemplateConfig CurrentConfig = new TemplateConfig();
        public TemplateCreator()
        {
            InitializeComponent();
            AvaibleRegiments = LoadAvailableRegiments();
            //debug
            var def = CollectUnitDefinitions();
            var gavno = 1;
        }


        public static List<(string Name, List<string> Categories)> CollectUnitDefinitions()
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var rootPath in new[] { ModManager.GameDirectory, ModManager.Directory })
            {
                if (!Directory.Exists(rootPath)) continue;

                var unitDirs = Directory.GetDirectories(rootPath, "units", SearchOption.AllDirectories)
                                      .Where(path => Path.GetFullPath(path)
                                          .Contains(Path.Combine("common", "units")));

                foreach (var unitDir in unitDirs)
                {
                    foreach (var file in Directory.GetFiles(unitDir, "*.txt", SearchOption.TopDirectoryOnly))
                    {
                        var fileContent = File.ReadAllText(file);
                        var fileSearcher = new BracketSearcher
                        {
                            OpenBracketChar = '{',
                            CloseBracketChar = '}',
                            CurrentString = fileContent.ToCharArray()
                        };

                        var subUnitsContents = fileSearcher.GetBracketContentByHeaderName("sub_units".ToCharArray());

                        foreach (var subUnitsContent in subUnitsContents)
                        {
                            var subUnitsSearcher = new BracketSearcher
                            {
                                OpenBracketChar = '{',
                                CloseBracketChar = '}',
                                CurrentString = subUnitsContent.ToCharArray()
                            };

                            var unitDefinitions = subUnitsSearcher.GetAllBracketSubbracketsNames(1);

                            foreach (var unitDef in unitDefinitions)
                            {
                                var unitContents = subUnitsSearcher.GetBracketContentByHeaderName(unitDef.ToCharArray());

                                foreach (var unitContent in unitContents)
                                {
                                    var unitSearcher = new BracketSearcher
                                    {
                                        OpenBracketChar = '{',
                                        CloseBracketChar = '}',
                                        CurrentString = unitContent.ToCharArray()
                                    };
                                    var categories = unitSearcher.GetBracketContentByHeaderName("categories".ToCharArray());

                                    if (categories.Count > 0)
                                    {
                                        var cleanCategories = categories.SelectMany(c => c.Split('\n'))
                                            .Select(line => line.Trim())
                                            .Where(line => !string.IsNullOrEmpty(line) &&
                                                          !line.Contains("{") &&
                                                          !line.Contains("}"))
                                            .ToList();

                                        if (cleanCategories.Count > 0)
                                        {
                                            var unitName = unitDef.Trim();
                                            if (!result.ContainsKey(unitName))
                                            {
                                                result[unitName] = cleanCategories;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        public static List<RegimentConfig> LoadAvailableRegiments()
        {
            var result = new List<RegimentConfig>();
            var unitDefs = CollectUnitDefinitions();

            foreach (var (name, categories) in unitDefs)
            {
                var icon = ImageManager.FindUnitIcon(name);
                result.Add(new RegimentConfig
                {
                    Name = name,
                    Categories = categories,
                    Icon = icon
                });
            }

            return result;
        }

        private static Bitmap SearchIconWithParts(FileSearcher searcher, string[] nameParts)
        {
            searcher.PatternsList = new List<string> { string.Join("_", nameParts) };
            var fullMatch = searcher.SearchFile();
            if (fullMatch != null)
            {
                try
                {
                    return ImageManager.LoadAndCropRightSideOfIcon(fullMatch.Name);
                }
                finally
                {
                    fullMatch.Close();
                }
            }

            for (int partsToTake = nameParts.Length - 1; partsToTake > 0; partsToTake--)
            {
                for (int startIdx = 0; startIdx <= nameParts.Length - partsToTake; startIdx++)
                {
                    var partialName = string.Join("_", nameParts.Skip(startIdx).Take(partsToTake));
                    searcher.PatternsList = new List<string> { partialName };
                    var partialMatch = searcher.SearchFile();

                    if (partialMatch != null)
                    {
                        try
                        {
                            return ImageManager.LoadAndCropRightSideOfIcon(partialMatch.Name);
                        }
                        finally
                        {
                            partialMatch.Close();
                        }
                    }
                }
            }

            return null;
        }


        private void AddElementEvent(object sender, MouseEventArgs e)
        {
            Panel panel = sender as Panel;
            if (e.Button == MouseButtons.Right && panel != null)
            {
                string panelName = panel.Name;
                string type = new string(panelName.TakeWhile(char.IsLetter).ToArray());
                string coords = new string(panelName.SkipWhile(char.IsLetter).ToArray());

                if (coords.Length != 2 || !int.TryParse(coords[0].ToString(), out int x) || !int.TryParse(coords[1].ToString(), out int y))
                {
                    MessageBox.Show($"Invalid panel name format: {panelName}. Expected format like 'Support01'.");
                    return;
                }

                string requiredCategory = type.Equals("Support", StringComparison.OrdinalIgnoreCase)
                    ? "category_support_battalions"
                    : type.Equals("Brigade", StringComparison.OrdinalIgnoreCase)
                        ? "category_army"
                        : null;

                if (requiredCategory == null)
                {
                    MessageBox.Show($"Unknown panel type: {type} in panel name {panelName}");
                    return;
                }

                var matchingRegiments = AvaibleRegiments
                    .Where(r => r.Categories != null &&
                        r.Categories.Contains(requiredCategory, StringComparer.OrdinalIgnoreCase) &&
                        (!type.Equals("Brigade", StringComparison.OrdinalIgnoreCase) ||
                         !r.Categories.Contains("category_support_battalions", StringComparer.OrdinalIgnoreCase)))
                    .OrderBy(r => r.Name);


                ContextMenuStrip menu = new ContextMenuStrip();

                foreach (var regiment in matchingRegiments)
                {
                    var item = new ToolStripMenuItem(regiment.Name)
                    {
                        Tag = regiment,
                        Image = regiment.Icon
                    };

                    item.Click += (s, args) =>
                    {
                        var selected = (RegimentConfig)((ToolStripMenuItem)s).Tag;

                        // Установка иконки
                        if (panel.BackgroundImage != null && panel.BackgroundImage != selected.Icon)
                        {
                            panel.BackgroundImage.Dispose();
                        }
                        panel.BackgroundImage = new Bitmap(selected.Icon);

                        // Создаем RegimentItem с координатами
                        var regimentItem = new RegimentConfig
                        {
                            Name = selected.Name,
                            Icon = selected.Icon,
                            Categories = selected.Categories,
                            X = x,
                            Y = y
                        };

                        // Добавляем в нужный список
                        if (type.Equals("Support", StringComparison.OrdinalIgnoreCase))
                        {
                            CurrentConfig.SupportItems.RemoveAll(r => r.X == x && r.Y == y); // Удаляем предыдущие на этих координатах
                            CurrentConfig.SupportItems.Add(regimentItem);
                        }
                        else if (type.Equals("Brigade", StringComparison.OrdinalIgnoreCase))
                        {
                            CurrentConfig.BrigadeItems.RemoveAll(r => r.X == x && r.Y == y);
                            CurrentConfig.BrigadeItems.Add(regimentItem);
                        }
                    };

                    menu.Items.Add(item);
                }

                if (menu.Items.Count == 0)
                {
                    menu.Items.Add("No matching regiments available");
                }

                menu.Show(panel, e.Location);
            }
        }

        private void UpdtadeConfig()
        {
            CurrentConfig.Name = this.NameBox.Text;
            CurrentConfig.IsLocked = this.IsLocked.Checked;
            CurrentConfig.Namespace = this.GroupNameBox.Text;
            CurrentConfig.AllowTraining = this.CanRecrutingLocked.Checked;
            CurrentConfig.ModelName = this.ModelIDBox.Text;
            CurrentConfig.DivisionCap = int.TryParse(this.DivisionCapBox.Text, out var cap) ? cap : 0;
            CurrentConfig.Priority = int.TryParse(this.PriorityBox.Text, out var priority) ? priority : 0;
            CurrentConfig.CustomIconId = int.TryParse(this.CustomIconBox.Text, out var iconId) ? iconId : 0;
            CurrentConfig.OOBFileName = this.OOBTagBox.Text;
            CurrentConfig.OOBFileYear = int.TryParse(this.OOBYearBox.Text, out var year) ? year : 0;
        }
        private void ConfigLoadButton_Click(object sender, EventArgs e)
        {
            WinFormConfigManager.LoadConfigWrapper(this);
            UpdtadeConfig();
        }

        private void SaveConfigButton_Click(object sender, EventArgs e)
        {
            UpdtadeConfig();
            WinFormConfigManager.SaveConfigWrapper(this);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            UpdtadeConfig();
            var config = CurrentConfig;

            if (string.IsNullOrWhiteSpace(config.Name))
            {
                MessageBox.Show("Название шаблона обязательно");
                return;
            }

            if (config.BrigadeItems == null || !config.BrigadeItems.Any())
            {
                MessageBox.Show("Должна быть хотя бы одна полка");
                return;
            }

            var unitsDir = Path.Combine(ModManager.Directory, "history", "units");
            Directory.CreateDirectory(unitsDir);

            var fileName = $"{(config.OOBFileName ?? "division")}_{(config.OOBFileYear)}.txt";
            var filePath = Path.Combine(unitsDir, fileName);

            var content = new StringBuilder();
            content.AppendLine("division_template = {");
            content.AppendLine($"    name = \"{config.Name}\"");

            if (!string.IsNullOrWhiteSpace(config.Namespace))
                content.AppendLine($"    division_names_group = \"{config.Namespace}\"");

            if (config.IsLocked.HasValue && config.IsLocked.Value)
                content.AppendLine("    is_locked = yes");

            if (config.AllowTraining.HasValue && config.AllowTraining.Value)
                content.AppendLine("    force_allow_recruiting = yes");

            if (config.DivisionCap.HasValue && config.DivisionCap != 0)
                content.AppendLine($"    division_cap = {config.DivisionCap}");

            if (config.Priority.HasValue && config.Priority != 0)
                content.AppendLine($"    priority = {config.Priority}");

            if (config.CustomIconId.HasValue && config.CustomIconId != 0)
                content.AppendLine($"    template_counter = {config.CustomIconId}");

            if (!string.IsNullOrWhiteSpace(config.ModelName))
                content.AppendLine($"    override_model = \"{config.ModelName}\"");

            // Полки (обязательные)
            content.AppendLine("    regiments = {");
            foreach (var regiment in config.BrigadeItems)
            {
                content.AppendLine($"        {regiment.Name} = {{ x = {regiment.X} y = {regiment.Y} }}");
            }
            content.AppendLine("    }");

            // Поддержка (только если есть)
            if (config.SupportItems != null && config.SupportItems.Any())
            {
                content.AppendLine("    support = {");
                foreach (var support in config.SupportItems)
                {
                    content.AppendLine($"        {support.Name} = {{ x = {support.X} y = {support.Y} }}");
                }
                content.AppendLine("    }");
            }

            content.AppendLine("}");

            try
            {
                File.WriteAllText(filePath, content.ToString());
                MessageBox.Show($"Файл сохранён: {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
