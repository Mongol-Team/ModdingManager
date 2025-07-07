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
using System.Runtime.CompilerServices;
using ModdingManager.classes.handlers;
using ModdingManager.classes.utils;

namespace ModdingManager
{
    public partial class CharacterCreator : Form
    {
        public CountryCharacterConfig CurrentConfig = new();
        private CharacterHandler HandlerIncetance;
        public CharacterCreator()
        {
            InitializeComponent();
        }
        #region Helper Methods
        private void UpdateCurrentConfig()
        {
            CurrentConfig.Tag = TagBox.Text;
            CurrentConfig.Id = IdBox.Text;
            CurrentConfig.Name = NameBox.Text;
            CurrentConfig.Expire = ExpireBox.Text;

            CurrentConfig.Attack = TryParseOrDefault(AtkBox.Text, 0);
            CurrentConfig.Defense = TryParseOrDefault(DefBox.Text, 0);
            CurrentConfig.Supply = TryParseOrDefault(SupplyBox.Text, 0);
            CurrentConfig.Speed = TryParseOrDefault(SpdBox.Text, 0);
            CurrentConfig.Skill = TryParseOrDefault(SkillBox.Text, 0);
            CurrentConfig.AdvisorCost = TryParseOrDefault(AdvisorCost.Text, 0);

            CurrentConfig.Types = CharTypesBox.GetLines();
            CurrentConfig.Traits = PercBox.GetLines();
            CurrentConfig.AiWillDo = AiDoBox.Text;
            CurrentConfig.Ideology = IdeologyBox.Text;
            CurrentConfig.BigImage = BigIconPanel.BackgroundImage;
            CurrentConfig.SmallImage = SmalIconPanel.BackgroundImage;
            HandlerIncetance = new CharacterHandler() { CurrentConfig = CurrentConfig };
        }

        private int TryParseOrDefault(string input, int defaultValue)
        {
            return int.TryParse(input, out int result) ? result : defaultValue;
        }


        #endregion
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            UpdateCurrentConfig();
            HandlerIncetance.CreateCharacterFile();
            HandlerIncetance.SaveCharacterPortraits();
            HandlerIncetance.CreateCharacterLocalizationFiles();
            HandlerIncetance.HandleGFXFile();
            HandlerIncetance.HandleRecruting();
        }
        #region Events
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

        private void CharacterCreator_Load(object sender, EventArgs e)
        {
            CharacterCreator form = sender as CharacterCreator;
            ComboBox cmb = (ComboBox)form.Controls["IdeologyBox"];
            List<string> list = new();
            foreach (IdeologyConfig ideo in Registry.Instance.Ideologies)
            {
                foreach (IdeologyType type in ideo.SubTypes)
                {
                    list.Add(type.Name);
                }
            }
            list.Sort();
            cmb.Items.AddRange(list.ToArray());
        }
        #endregion

    }
}
