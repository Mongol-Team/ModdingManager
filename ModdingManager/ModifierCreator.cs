using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using TeximpNet.Compression;
using TeximpNet.DDS;
using TeximpNet;
using ModdingManager.managers.utils;

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
                                SaveModifierIconWithTexImpNet();
                                UpdateInterfaceFiles();
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
            //UpdateLocalizationFiles();
            //UpdateInterfaceFiles();
            SaveModifierIconWithTexImpNet();
        }

        private void ConfigLoadButton_Click(object sender, EventArgs e)
        {
            WinFormConfigManager.LoadConfigWrapper(this);
        }

        private void SaveConfigButton_Click(object sender, EventArgs e)
        {
            WinFormConfigManager.SaveConfigWrapper(this);
        }

        public void UpdateInterfaceFiles()
        {
            string tag = TagBox.Text;
            string id = IdBox.Text;

            if (string.IsNullOrEmpty(tag)) tag = "custom";
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("ID cannot be empty for interface files!");
                return;
            }

            try
            {
                // 1. Обновляем файл иконок (*_ideas.gfx)
                UpdateIdeasGfxFile(tag, id);

                // 2. Обновляем countrystateview.gui
                UpdateCountryStateViewGui(tag, id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating interface files: {ex.Message}");
            }
        }

        private void UpdateIdeasGfxFile(string tag, string id)
        {
            string filePath = Path.Combine(ModManager.Directory, "interface", $"{tag}_ideas.gfx");
            string spriteEntry = $"\tspriteType = {{\n\t\tname = \"GFX_modifiers_{id}_icon\"\n\t\ttextureFile = \"gfx/interface/modifiers_{id}_icon.dds\"\n\t}}";

            // Создаем директорию, если ее нет
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);

                if (content.Contains("spriteTypes = {"))
                {
                    if (content.Contains($"name = \"GFX_modifiers_{id}_icon\""))
                    {
                        // Обновляем существующую запись (если нужно)
                        string pattern = $@"spriteType\s*=\s*{{\s*name\s*=\s*""GFX_modifiers_{id}_icon""[^}}*}}";
                        content = Regex.Replace(content, pattern, spriteEntry);
                    }
                    else
                    {
                        // Добавляем новую запись перед закрывающей скобкой
                        int insertPos = content.LastIndexOf('}');
                        content = content.Substring(0, insertPos) + spriteEntry + "\n}";
                    }
                }
                else
                {
                    // Создаем новый файл с базовой структурой
                    content = $"spriteTypes = {{\n{spriteEntry}\n}}";
                }

                File.WriteAllText(filePath, content);
            }
            else
            {
                // Создаем новый файл
                string content = $"spriteTypes = {{\n{spriteEntry}\n}}";
                File.WriteAllText(filePath, content);
            }
        }

        private void UpdateCountryStateViewGui(string tag, string id)
        {
            string filePath = Path.Combine(ModManager.Directory, "interface", "countrystateview.gui");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("countrystateview.gui not found! Skipping icon registration.");
                return;
            }

            string content = File.ReadAllText(filePath);
            string iconEntry = $"\t\ticonType = {{\n\t\t\tname = \"{id}_icon\"\n\t\t\tspriteType = \"GFX_modifiers_{id}_icon\"\n\t\t\tposition = {{ x = 0 y = 0 }}\n\t\t\tOrientation = \"UPPER_LEFT\"\n\t\t}}";

            if (content.Contains("name = \"custom_icon_container\""))
            {
                if (content.Contains($"name = \"{id}_icon\""))
                {
                    // Обновляем существующую запись (если нужно)
                    string pattern = $@"iconType\s*=\s*{{\s*name\s*=\s*""{id}_icon""[^}}*}}";
                    content = Regex.Replace(content, pattern, iconEntry);
                }
                else
                {
                    // Добавляем новую запись перед закрывающим containerWindowType
                    int containerPos = content.IndexOf("name = \"custom_icon_container\"");
                    int insertPos = content.IndexOf("}", containerPos);

                    // Ищем последнюю закрывающую скобку перед containerWindowType
                    while (insertPos > 0 && content.Substring(insertPos).Contains("containerWindowType"))
                    {
                        insertPos = content.LastIndexOf("}", insertPos - 1);
                    }

                    content = content.Insert(insertPos, iconEntry + "\n\t\t");
                }

                File.WriteAllText(filePath, content);
            }
            else
            {
                MessageBox.Show("custom_icon_container not found in countrystateview.gui! Skipping icon registration.");
            }
        }

        private void ImagePanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                string filePath = files[0];

                if (Path.GetExtension(filePath).ToLower() == ".jpg" || Path.GetExtension(filePath).ToLower() == ".png")
                {
                    try
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromFile(filePath);

                        ImagePanel.BackgroundImage = image;

                        ImagePanel.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, перетащите изображение в формате JPG или PNG.");
                }
            }
        }

        private void ImagePanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0 && (Path.GetExtension(files[0]).ToLower() == ".jpg" || Path.GetExtension(files[0]).ToLower() == ".png"))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public void SaveModifierIconWithTexImpNet()
        {
            string id = IdBox.Text;
            string tag = TagBox.Text;

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("ID cannot be empty for icon saving!");
                return;
            }

            if (ImagePanel.BackgroundImage == null)
            {
                MessageBox.Show("No image selected in ImagePanel!");
                return;
            }

            try
            {
                string directory = Path.Combine(ModManager.Directory, "gfx", "interface", "modifiers");
                Directory.CreateDirectory(directory);

                string outputPath = Path.Combine(directory, $"modifiers_{id}_icon.dds");

                // Получаем изображение и ресайзим до 30x30
                using (var resized = new Bitmap(ImagePanel.BackgroundImage, 30, 30))
                {
                    // Конвертируем в формат 32bpp ARGB
                    using (var bmp = new Bitmap(resized.Width, resized.Height, PixelFormat.Format32bppArgb))
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(resized, 0, 0, resized.Width, resized.Height);

                        // Получаем raw-данные изображения
                        BitmapData bmpData = bmp.LockBits(
                            new Rectangle(0, 0, bmp.Width, bmp.Height),
                            ImageLockMode.ReadOnly,
                            PixelFormat.Format32bppArgb);

                        try
                        {
                            byte[] pixelData = new byte[bmpData.Stride * bmp.Height];
                            Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

                            // Конвертируем BGRA → RGBA (если нужно)
                            for (int i = 0; i < pixelData.Length; i += 4)
                            {
                                byte b = pixelData[i];
                                byte r = pixelData[i + 2];
                                pixelData[i] = r;
                                pixelData[i + 2] = b;
                            }

                            // Создаем Surface из данных
                            using (var surface = new Surface(bmp.Width, bmp.Height))
                            {
                                Marshal.Copy(pixelData, 0, surface.DataPtr, pixelData.Length);

                                // Сохраняем в DDS
                                DDSFile.Write(outputPath, surface, TextureDimension.Three, DDSFlags.None);
                            }
                        }
                        finally
                        {
                            bmp.UnlockBits(bmpData);
                        }
                    }
                }

                MessageBox.Show($"Icon saved successfully to:\n{outputPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving icon: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}
