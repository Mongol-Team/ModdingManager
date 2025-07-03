using Microsoft.VisualBasic;
using ModdingManager.configs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModdingManager.classes.gfx;
using ModdingManager.classes.extentions;
using ModdingManager.managers.utils;

namespace ModdingManager
{
    public partial class CharacterCreator : Form
    {
        public CountryCharacterConfig currentCharacter = new();
        public CharacterCreator()
        {
            InitializeComponent();
        }
        public static void CreateCharacterLocalizationFiles(Form form, string modPath)
        {
            try
            {
                string tag = ((TextBox)form.Controls["TagBox"]).Text;
                string charId = ((TextBox)form.Controls["IdBox"]).Text;
                string charName = ((TextBox)form.Controls["NameBox"]).Text;
                string charDesc = ((TextBox)form.Controls["DescBox"]).Text;
                string charTypes = ((RichTextBox)form.Controls["CharTypesBox"]).Text;

                string ruLocPath = Path.Combine(modPath, "localisation", "russian");
                string enLocPath = Path.Combine(modPath, "localisation", "english");
                Directory.CreateDirectory(ruLocPath);
                Directory.CreateDirectory(enLocPath);

                string ruFilePath = Path.Combine(ruLocPath, $"{tag}_characters_l_russian.yml");
                string enFilePath = Path.Combine(enLocPath, $"{tag}_characters_l_english.yml");

                string ruEntries = GenerateLocalizationEntries(charId, charName, charDesc, charTypes.Contains("country_leader"), false);
                string enEntries = GenerateLocalizationEntries(charId, "", "", charTypes.Contains("country_leader"), true);

                ProcessLocalizationFile(ruFilePath, "l_russian", ruEntries);
                ProcessLocalizationFile(enFilePath, "l_english", enEntries);

                MessageBox.Show("Файлы локализации созданы успешно!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания локализации: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public static void SaveCharacterPortraits(Form form, string modPath)
        {
            try
            {
                string tag = ((TextBox)form.Controls["TagBox"]).Text;
                string charId = ((TextBox)form.Controls["IdBox"]).Text;

                Panel bigIconPanel = (Panel)form.Controls["BigIconPanel"];
                Panel smallIconPanel = (Panel)form.Controls["SmalIconPanel"];

                if (bigIconPanel.BackgroundImage == null && smallIconPanel.BackgroundImage == null)
                {
                    MessageBox.Show("Необходимо добавить хотябы 1 изображение!",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string leadersDir = Path.Combine(modPath, "gfx", "leaders", tag);
                string advisorsDir = Path.Combine(modPath, "gfx", "advisors", tag);
                Directory.CreateDirectory(leadersDir);
                Directory.CreateDirectory(advisorsDir);


                bigIconPanel.BackgroundImage.SaveAsDDS(leadersDir, $"{charId}_army.dds", 156, 210);
                bigIconPanel.BackgroundImage.SaveAsDDS(leadersDir, $"{charId}_civilian.dds", 156, 210);

                smallIconPanel.BackgroundImage.SaveAsDDS(advisorsDir, $"{charId}_army.dds",65, 67);
                smallIconPanel.BackgroundImage.SaveAsDDS(advisorsDir, $"{charId}_civilian.dds", 65, 67);

                MessageBox.Show("Портреты сохранены успешно!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void CreateCharacterFile(Form form, string modPath)
        {
            try
            {
                string charactersDir = Path.Combine(modPath, "common", "characters");
                Directory.CreateDirectory(charactersDir);

                string tag = ((TextBox)form.Controls["TagBox"]).Text;
                string charId = ((TextBox)form.Controls["IdBox"]).Text;
                string charName = ((TextBox)form.Controls["NameBox"]).Text;
                string charDesc = ((TextBox)form.Controls["DescBox"]).Text;
                string charTypes = ((RichTextBox)form.Controls["CharTypesBox"]).Text;
                string traits = ((RichTextBox)form.Controls["PercBox"]).Text;
                string filePath = Path.Combine(charactersDir, $"{tag}.txt");

                // Формируем содержимое нового персонажа
                StringBuilder content = new StringBuilder();

                // Начинаем описание персонажа
                content.AppendLine($"\t{charId} = {{");
                content.AppendLine($"\t\tname = {charId}");

                // Портреты (исправлены закрывающие скобки)
                content.AppendLine("\t\tportraits = {");
                content.AppendLine("\t\t\tcivilian = {");
                content.AppendLine($"\t\t\t\tlarge = \"gfx/leaders/{tag}/{charId}_civilian.dds\"");
                content.AppendLine($"\t\t\t\tsmall = \"gfx/advisors/{tag}/{charId}_civilian.dds\"");
                content.AppendLine("\t\t\t}");
                content.AppendLine("\t\t\tarmy = {");
                content.AppendLine($"\t\t\t\tlarge = \"gfx/leaders/{tag}/{charId}_army.dds\"");
                content.AppendLine($"\t\t\t\tsmall = \"gfx/advisors/{tag}/{charId}_army.dds\"");
                content.AppendLine("\t\t\t}");
                content.AppendLine("\t\t}");
                if (charTypes.Contains("advisor"))
                {
                    string slot = ((ComboBox)form.Controls["AdvisorSlot"]).SelectedItem?.ToString() ?? "high_command";
                    string cost = ((TextBox)form.Controls["AdvisorCost"]).Text;
                    string aiDo = ((TextBox)form.Controls["AiDoBox"]).Text;

                    content.AppendLine("\t\tadvisor = {");
                    content.AppendLine($"\t\t\tslot = {slot}");
                    content.AppendLine($"\t\t\tidea_token = {charId}");
                    content.AppendLine("\t\t\tledger = army");
                    content.AppendLine("\t\t\tallowed = {");
                    content.AppendLine($"\t\t\t\toriginal_tag = {tag}");
                    content.AppendLine("\t\t\t}");
                    content.AppendLine("\t\t\ttraits = {");
                    foreach (var trait in traits.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        content.AppendLine($"\t\t\t\t{trait}");
                    }
                    content.AppendLine("\t\t\t}");
                    content.AppendLine($"\t\t\tcost = {cost}");
                    content.AppendLine("\t\t\tai_will_do = {");
                    content.AppendLine($"\t\t\t\tfactor = {aiDo}");
                    content.AppendLine("\t\t\t}");
                    content.AppendLine("\t\t}");
                }

                if (charTypes.Contains("navy_leader") || charTypes.Contains("field_marshal") || charTypes.Contains("corps_commander"))
                {
                    string type = charTypes.Contains("navy_leader") ? "navy_leader" :
                                 charTypes.Contains("field_marshal") ? "field_marshal" : "corps_commander";

                    content.AppendLine($"\t\t{type} = {{");
                    content.AppendLine("\t\t\ttraits = {");
                    foreach (var trait in traits.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        content.AppendLine($"\t\t\t\t{trait}");
                    }
                    content.AppendLine("\t\t\t}");
                    content.AppendLine($"\t\t\tskill = {((TextBox)form.Controls["SkillBox"]).Text}");
                    content.AppendLine($"\t\t\tattack_skill = {((TextBox)form.Controls["AtkBox"]).Text}");
                    content.AppendLine($"\t\t\tdefense_skill = {((TextBox)form.Controls["DefBox"]).Text}");

                    if (type == "navy_leader")
                    {
                        content.AppendLine($"\t\t\tmaneuvering_skill = {((TextBox)form.Controls["SpdBox"]).Text}");
                        content.AppendLine($"\t\t\tcoordination_skill = {((TextBox)form.Controls["SupplyBox"]).Text}");
                    }
                    else
                    {
                        content.AppendLine($"\t\t\tplanning_skill = {((TextBox)form.Controls["SpdBox"]).Text}");
                        content.AppendLine($"\t\t\tlogistics_skill = {((TextBox)form.Controls["SupplyBox"]).Text}");
                    }

                    content.AppendLine($"\t\t\tlegacy_id = {charId}");
                    content.AppendLine("\t\t}");
                }

                if (charTypes.Contains("country_leader"))
                {
                    content.AppendLine("\t\tcountry_leader = {");
                    content.AppendLine($"\t\t\texpire = \"{((TextBox)form.Controls["ExpireBox"]).Text}\"");
                    content.AppendLine($"\t\t\tideology = \"{((TextBox)form.Controls["IdeologyBox"]).Text}\"");
                    content.AppendLine("\t\t\ttraits = {");
                    foreach (var trait in traits.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        content.AppendLine($"\t\t\t\t{trait}");
                    }
                    content.AppendLine("\t\t\t}");
                    content.AppendLine($"\t\t\tdesc = {charId}_desc");
                    content.AppendLine("\t\t}");
                }

                content.AppendLine("\t}");

                // Обрабатываем существующий файл
                string finalContent;
                if (File.Exists(filePath))
                {
                    string existingContent = File.ReadAllText(filePath);

                    // Удаляем последнюю закрывающую скобку если есть
                    if (existingContent.TrimEnd().EndsWith("}"))
                    {
                        existingContent = existingContent.TrimEnd().Substring(0, existingContent.TrimEnd().Length - 1);
                    }

                    // Добавляем запятую после последнего персонажа если нужно


                    // Собираем финальный контент
                    finalContent = $"{existingContent}\n{content}\n}}";
                }
                else
                {
                    finalContent = $"characters = {{\n{content}\n}}";
                }

                // Сохраняем файл в UTF-8 без BOM
                File.WriteAllText(filePath, finalContent, new UTF8Encoding(false));

                MessageBox.Show("Файл персонажа успешно создан/обновлен!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла персонажа: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            CreateCharacterFile(this, ModManager.Directory);
            SaveCharacterPortraits(this, ModManager.Directory);
            CreateCharacterLocalizationFiles(this, ModManager.Directory);
            
        }

        private void BigIconBox_DragEnter(object sender, DragEventArgs e)
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

        private void BigIconBox_DragDrop(object sender, DragEventArgs e)
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

                        BigIconPanel.BackgroundImage = image;

                        BigIconPanel.BackgroundImageLayout = ImageLayout.Stretch;
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

        private void SmallIconPanel_DragDrop(object sender, DragEventArgs e)
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

                        SmalIconPanel.BackgroundImage = image;

                        SmalIconPanel.BackgroundImageLayout = ImageLayout.Stretch;
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

        private void SmallIconPanel_DragEnter(object sender, DragEventArgs e)
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

        private void LoadButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                WinFormConfigManager.LoadConfigWrapper(this);
            });
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {

            WinFormConfigManager.SaveConfigWrapper(this);
        }

        private void UpdateCharacterFromForm()
        {
            // Основные свойства
            currentCharacter.Id = IdBox.Text;
            currentCharacter.Name = NameBox.Text;
            currentCharacter.Description = DescBox.Text;
            currentCharacter.Traits = new List<string>(PercBox.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
            currentCharacter.Tag = TagBox.Text;

            // Типы персонажа
            currentCharacter.Types = new List<string>(CharTypesBox.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

            // Статистика
            if (int.TryParse(SkillBox.Text, out int skill)) currentCharacter.Skill = skill;
            if (int.TryParse(AtkBox.Text, out int attack)) currentCharacter.Attack = attack;
            if (int.TryParse(DefBox.Text, out int defense)) currentCharacter.Defense = defense;
            if (int.TryParse(SupplyBox.Text, out int supply)) currentCharacter.Supply = supply;
            if (int.TryParse(SpdBox.Text, out int speed)) currentCharacter.Speed = speed;

            // Свойства советника
            currentCharacter.AdvisorSlot = AdvisorSlot.Text;
            if (int.TryParse(AdvisorCost.Text, out int cost)) currentCharacter.AdvisorCost = cost;
            currentCharacter.AiWillDo = AiDoBox.Text;

            // Дополнительные свойства
            currentCharacter.Expire = ExpireBox.Text;
        }

        private void UpdateFormFromCharacter()
        {
            // Основные свойства
            IdBox.Text = currentCharacter.Id;
            NameBox.Text = currentCharacter.Name;
            DescBox.Text = currentCharacter.Description;
            PercBox.Text = string.Join("\n", currentCharacter.Traits);
            TagBox.Text = currentCharacter.Tag;

            // Типы персонажа
            CharTypesBox.Text = string.Join("\n", currentCharacter.Types);

            // Статистика
            SkillBox.Text = currentCharacter.Skill.ToString();
            AtkBox.Text = currentCharacter.Attack.ToString();
            DefBox.Text = currentCharacter.Defense.ToString();
            SupplyBox.Text = currentCharacter.Supply.ToString();
            SpdBox.Text = currentCharacter.Speed.ToString();

            // Свойства советника
            AdvisorSlot.Text = currentCharacter.AdvisorSlot;
            AdvisorCost.Text = currentCharacter.AdvisorCost.ToString();
            AiDoBox.Text = currentCharacter.AiWillDo;

            // Дополнительные свойства
            ExpireBox.Text = currentCharacter.Expire;
        }

        
    }
}
