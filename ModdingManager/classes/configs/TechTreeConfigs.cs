using ModdingManager.classes.extentions;
using ModdingManager.managers.@base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
namespace ModdingManager.configs
{
    public class TechTreeConfig
    {
        public string Name { get; set; } 
        public string Orientation { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public List<List<string>> ChildOf { get; set; } = new List<List<string>>(); 
        public List<List<string>> Mutal { get; set; } = new List<List<string>>(); 
        public string Ledger { get; set; }

        public void SaveAllTechIconsAsDDS()
        {

            string techIconDir = Path.Combine(ModManager.ModDirectory, "gfx", "interface", "technologies");
            Directory.CreateDirectory(techIconDir);

            foreach (var item in this.Items)
            {
                if (item.Image == null || string.IsNullOrWhiteSpace(item.Id))
                    continue;

                try
                {
                    using (var bmp = item.Image.ToBitmap())
                    {
                        bmp.SaveAsDDS(techIconDir, item.Id, 64, 64);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при сохранении иконки технологии {item.Id}: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }

            System.Windows.MessageBox.Show("Сохранение иконок технологий завершено!", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }

    public class TechTreeItemConfig
    {
        public string Id { get; set; }
        public string OldId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public bool IsBig { get; set; }
        public int ModifCost { get; set; }
        public string LocName { get; set; }
        public string LocDescription { get; set; }
        public string Categories { get; set; }
        public List<string> Enables { get; set; }
        public int Cost { get; set; }
        public int StartYear { get; set; }
        public List<string> Allowed { get; set; }
        public List<string> Modifiers { get; set; }
        public List<string> Effects { get; set; }
        public string AiWillDo { get; set; }
        public List<string> Dependencies { get; set; }
        [JsonIgnore]
        public ImageSource Image { get; set; }
        public byte[] ImageData { get; set; }
    }
}
