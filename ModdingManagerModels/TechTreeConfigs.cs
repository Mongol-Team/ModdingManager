namespace ModdingManagerModels
{
    public class TechTreeConfig : IModel
    {
        public string Name { get; set; }
        public string Orientation { get; set; }
        public List<TechTreeItemConfig> Items { get; set; } = new List<TechTreeItemConfig>();
        public List<List<string>> ChildOf { get; set; } = new List<List<string>>();
        public List<List<string>> Mutal { get; set; } = new List<List<string>>();
        public string Ledger { get; set; }

        //OTM
        //public void SaveAllTechIconsAsDDS()
        //{

        //    string techIconDir = Path.Combine(ModManager.ModDirectory, "gfx", "interface", "technologies");
        //    Directory.CreateDirectory(techIconDir);

        //    foreach (var item in this.Items)
        //    {
        //        if (item.Image == null || string.IsNullOrWhiteSpace(item.Id))
        //            continue;

        //        try
        //        {
        //            using (var bmp = item.Image.ToBitmap())
        //            {
        //                bmp.SaveAsDDS(techIconDir, item.Id, 64, 64);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Windows.MessageBox.Show($"Ошибка при сохранении иконки технологии {item.Id}: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        //        }
        //    }

        //    System.Windows.MessageBox.Show("Сохранение иконок технологий завершено!", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        //}
    }
}
