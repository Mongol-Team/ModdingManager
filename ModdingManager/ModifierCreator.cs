using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class ModifierCreator : Form
    {
        public ModifierCreator()
        {
            InitializeComponent();
        }
        public void UpdateLocalizationFiles()
        {
            string id = IdBox.Text;
            string tag = TagBox.Text;
            string name = NameBox.Text;
            string desc = DescBox.Text;
            string type = TypeBox.Text;
            string variation = VariationBox.Text;

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("ID cannot be empty for localization!");
                return;
            }

            try
            {
                // Формируем базовое имя файла в зависимости от типа и вариации
                string fileBaseName = GetLocalizationFileName(tag, type, variation);

                // Русская локализация
                UpdateSingleLocalizationFile(
                    Path.Combine(ModManager.Directory, "localisation", "russian", $"{fileBaseName}_l_russian.yml"),
                    "l_russian:",
                    name,
                    desc);

                // Английская локализация
                UpdateSingleLocalizationFile(
                    Path.Combine(ModManager.Directory, "localisation", "english", $"{fileBaseName}_l_english.yml"),
                    "l_english:",
                    "", // Пустое значение для английской версии
                    "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating localization files: {ex.Message}");
            }
        }

        private string GetLocalizationFileName(string tag, string type, string variation)
        {
            return type switch
            {
                "static" => variation == "default"
                    ? $"{tag}_static"
                    : $"{tag}_{variation}_modifier",
                "dynamic" => $"{tag}_dynamic_modifier",
                "opinion" => $"{tag}_opinion_modifier",
                _ => tag
            };
        }

        private void UpdateSingleLocalizationFile(string filePath, string languageHeader, string nameValue, string descValue)
        {
            string id = IdBox.Text;
            string content = $"{languageHeader}\n {id}:0 \"{nameValue}\"\n {id}_desc:0 \"{descValue}\"\n";

            // Создаем директорию, если ее нет
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // Кодировка UTF-8 с BOM
            Encoding utf8WithBom = new UTF8Encoding(true);

            if (File.Exists(filePath))
            {
                string existingContent = File.ReadAllText(filePath, utf8WithBom);

                if (existingContent.Contains(languageHeader))
                {
                    // Обновляем существующие записи или добавляем новые
                    string pattern = $@"({Regex.Escape(id)}:0\s*"")([^""]*)("")";
                    string replacementName = $"$1{nameValue}$3";
                    string replacementDesc = $@"({Regex.Escape(id)}_desc:0\s*"")([^""]*)("")";
                    replacementDesc = $"$1{descValue}$3";

                    // Заменяем name
                    if (Regex.IsMatch(existingContent, pattern))
                    {
                        existingContent = Regex.Replace(existingContent, pattern, replacementName);
                    }
                    else
                    {
                        existingContent = existingContent.TrimEnd() + $"\n {id}:0 \"{nameValue}\"\n";
                    }

                    // Заменяем desc
                    if (Regex.IsMatch(existingContent, replacementDesc))
                    {
                        existingContent = Regex.Replace(existingContent, replacementDesc, replacementDesc);
                    }
                    else
                    {
                        existingContent = existingContent.TrimEnd() + $"\n {id}_desc:0 \"{descValue}\"\n";
                    }

                    File.WriteAllText(filePath, existingContent, utf8WithBom);
                }
                else
                {
                    // Добавляем новый блок локализации
                    File.AppendAllText(filePath, "\n" + content, utf8WithBom);
                }
            }
            else
            {
                // Создаем новый файл
                File.WriteAllText(filePath, content, utf8WithBom);
            }
        }
        public void CreateModifier()
        {
            string type = TypeBox.Text;
            string variation = VariationBox.Text;
            string tag = TagBox.Text;
            string id = IdBox.Text;

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("ID cannot be empty!");
                return;
            }

            try
            {
                string directory = Path.Combine(ModManager.Directory, "common");
                string filePath = "";
                string content = "";

                switch (type)
                {
                    case "opinion":
                        filePath = Path.Combine(directory, "opinion_modifiers", $"{tag}_option_modifiers.txt");
                        content = CreateOpinionModifier();
                        break;

                    case "dynamic":
                        filePath = Path.Combine(directory, "dynamic_modifiers", $"{tag}_dynamic_modifiers.txt");
                        content = CreateDynamicModifier();
                        break;

                    case "static":
                    default:
                        switch (variation)
                        {
                            case "power_balance":
                                filePath = Path.Combine(directory, "modifiers", $"{tag}_power_balance_modifiers.txt");
                                content = CreatePowerBalanceModifier();
                                break;

                            case "relation":
                                filePath = Path.Combine(directory, "modifiers", $"{tag}_relation_modifiers.txt");
                                content = CreateRelationModifier();
                                break;

                            case "province":
                                filePath = Path.Combine(directory, "modifiers", $"{tag}_province_modifiers.txt");
                                content = CreateStaticModifier();
                                break;

                            case "default":
                            default:
                                filePath = Path.Combine(directory, "modifiers", $"{tag}_static_modifiers.txt");
                                content = CreateStaticModifier();
                                break;
                        }
                        break;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                if (File.Exists(filePath))
                {
                    switch (type)
                    {
                        case "opinion":
                            string existingContent = File.ReadAllText(filePath);
                            if (existingContent.Contains("opinion_modifiers = {"))
                            {
                                // Вставляем перед закрывающей скобкой
                                int insertPos = existingContent.LastIndexOf('}');
                                content = existingContent.Substring(0, insertPos) + content + Environment.NewLine + "}";
                                File.WriteAllText(filePath, content);
                            }
                            else
                            {
                                // Если файл существует, но не содержит opinion_modifiers = {}
                                File.WriteAllText(filePath, $"opinion_modifiers = {{{Environment.NewLine}{content}{Environment.NewLine}}}");
                            }
                            break;

                        case "dynamic":
                            // Для динамических модификаторов просто добавляем в конец файла
                            File.AppendAllText(filePath, content + Environment.NewLine);
                            break;

                        case "static":
                            // Для статических модификаторов проверяем вариации
                            switch (variation)
                            {
                                case "power_balance":
                                case "relation":
                                case "province":
                                case "default":
                                    // Все эти вариации используют простые записи, просто добавляем в конец
                                    File.AppendAllText(filePath, content + Environment.NewLine);
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    // Файл не существует, создаем новый
                    switch (type)
                    {
                        case "opinion":
                            File.WriteAllText(filePath, $"opinion_modifiers = {{{Environment.NewLine}{content}{Environment.NewLine}}}");
                            break;

                        case "dynamic":
                        case "static":
                            File.WriteAllText(filePath, content + Environment.NewLine);
                            break;
                    }
                }

                MessageBox.Show("Modifier created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating modifier: {ex.Message}");
            }
        }

        private string CreateOpinionModifier()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"    {IdBox.Text} = {{");

            if (IsTradeBox.Checked)
                sb.AppendLine("        trade = yes");

            if (!string.IsNullOrEmpty(ValueBox.Text))
                sb.AppendLine($"        value = {ValueBox.Text}");

            if (!string.IsNullOrEmpty(DecayBox.Text))
                sb.AppendLine($"        decay = {DecayBox.Text}");

            if (!string.IsNullOrEmpty(DaysBox.Text))
                sb.AppendLine($"        days = {DaysBox.Text}");

            if (!string.IsNullOrEmpty(MinTrustBox.Text))
                sb.AppendLine($"        min_trust = {MinTrustBox.Text}");

            if (!string.IsNullOrEmpty(MaxTrustBox.Text))
                sb.AppendLine($"        max_trust = {MaxTrustBox.Text}");

            sb.AppendLine("    }");

            return sb.ToString();
        }

        private string CreateDynamicModifier()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{IdBox.Text} = {{");
            sb.AppendLine($"    icon = GFX_idea_{IdBox.Text}");

            if (EnableBox.Lines.Length > 0)
            {
                sb.AppendLine("    enable = {");
                foreach (string line in EnableBox.Lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        sb.AppendLine($"        {line}");
                }
                sb.AppendLine("    }");
            }

            if (RemovalTriggerBox.Lines.Length > 0)
            {
                sb.AppendLine("    remove_trigger = {");
                foreach (string line in RemovalTriggerBox.Lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        sb.AppendLine($"        {line}");
                }
                sb.AppendLine("    }");
            }

            foreach (string line in ModifBox.Lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    if (parts.Length == 3 && parts[2] == "1")
                    {
                        sb.AppendLine($"    hidden_modifier = {{ {parts[0]} = {parts[1]} }}");
                    }
                    else
                    {
                        sb.AppendLine($"    {parts[0]} = {parts[1]}");
                    }
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string CreateStaticModifier()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{IdBox.Text} = {{");

            foreach (string line in ModifBox.Lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    if (parts.Length == 3 && parts[2] == "1")
                    {
                        sb.AppendLine($"    hidden_modifier = {{ {parts[0]} = {parts[1]} }}");
                    }
                    else
                    {
                        sb.AppendLine($"    {parts[0]} = {parts[1]}");
                    }
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string CreatePowerBalanceModifier()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{IdBox.Text} = {{");

            foreach (string line in PowerBalanceBox.Lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    if (parts.Length == 3 && parts[2] == "1")
                    {
                        sb.AppendLine($"    hidden_modifier = {{ {parts[0]} = {parts[1]} }}");
                    }
                    else
                    {
                        sb.AppendLine($"    {parts[0]} = {parts[1]}");
                    }
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string CreateRelationModifier()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{IdBox.Text} = {{");

            // Relation triggers
            if (RelationTrigerBox.Lines.Length > 0)
            {
                sb.AppendLine("    valid_relation_trigger = {");
                foreach (string line in RelationTrigerBox.Lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        sb.AppendLine($"        {line}");
                }
                sb.AppendLine("    }");
            }

            // Modifiers
            foreach (string line in ModifBox.Lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(':');
                if (parts.Length >= 2)
                {
                    if (parts.Length == 3 && parts[2] == "1")
                    {
                        sb.AppendLine($"    hidden_modifier = {{ {parts[0]} = {parts[1]} }}");
                    }
                    else
                    {
                        sb.AppendLine($"    {parts[0]} = {parts[1]}");
                    }
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            //CreateModifier();
            UpdateLocalizationFiles();
        }

        private void ConfigLoadButton_Click(object sender, EventArgs e)
        {
            ConfigManager.LoadConfigAsync(this);
        }

        private void SaveConfigButton_Click(object sender, EventArgs e)
        {
            ConfigManager.SaveConfigWrapper(this);
        }
    }
}
