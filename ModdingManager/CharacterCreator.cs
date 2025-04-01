using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModdingManager
{
    public partial class CharacterCreator : Form
    {
        public CharacterCreator()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

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
                string charTypes = ((RichTextBox)form.Controls["CharTypes"]).Text;
                string traits = ((RichTextBox)form.Controls["PercBox"]).Text;
                string filePath = Path.Combine(charactersDir, $"{tag}.txt");

                StringBuilder content = new StringBuilder();
                content.AppendLine("characters = {");
                string existingContent = File.Exists(filePath) ?
                    File.ReadAllText(filePath).Replace("}", "").Trim() : "";

                if (!string.IsNullOrEmpty(existingContent))
                {
                    content.AppendLine(existingContent);
                }

                content.AppendLine($"\t{charId} = {{");
                content.AppendLine($"\t\tname = {charId}");

                // Портреты
                content.AppendLine("\t\tportraits = {");
                content.AppendLine("\t\t\tcivilian = {");
                content.AppendLine($"\t\t\t\tlarge = \"gfx/leaders/{tag}/{charId}_civilian.png\"");
                content.AppendLine($"\t\t\t\tsmall = \"gfx/advisors/{tag}/{charId}_civilian.png\"");
                content.AppendLine("\t\t\t}");
                content.AppendLine("\t\t\tarmy = {");
                content.AppendLine($"\t\t\t\tlarge = \"gfx/leaders/{tag}/{charId}_army.png\"");
                content.AppendLine($"\t\t\t\tsmall = \"gfx/advisors/{tag}/{charId}_army.png\"");
                content.AppendLine("\t\t\t}");
                content.AppendLine("\t\t}");

                // Обрабатываем роли персонажа
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
                    content.AppendLine("\t\t\tideology = neutrality");
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
                content.AppendLine("}");

                // Сохраняем файл в UTF-8 без BOM
                File.WriteAllText(filePath, content.ToString(), new UTF8Encoding(false));

                MessageBox.Show("Файл персонажа успешно создан!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла персонажа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            CreateCharacterFile(this, ModManager.directory);
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

                        SmallIconPanel.BackgroundImage = image;

                        SmallIconPanel.BackgroundImageLayout = ImageLayout.Stretch;
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
    }
}
