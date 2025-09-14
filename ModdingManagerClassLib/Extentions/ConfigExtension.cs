using ModdingManagerModels;

namespace ModdingManagerClassLib.Extentions
{
    public static class ConfigExtension
    {
        public static TechTreeItemConfig GetTreeItem(this List<TechTreeConfig> config, string id)
        {
            foreach (var cfg in config)
            {
                var item = cfg.Items.FirstOrDefault(i => i.Id.AsString() == id);
                if (item != null)
                    return item;
            }
            return null;
        }
        
    }
}
