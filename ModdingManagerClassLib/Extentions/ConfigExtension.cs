using ModdingManager.classes.utils;
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
        public static StateConfig GetState(this ModConfig mod, int id)
        {
            return mod.Map.States.FirstOrDefault(s => s.Id.AsInt() == id);
        }
        public static IdeologyConfig GetIdeology(this ModConfig mod, string id)
        {
            return mod.Ideologies.FirstOrDefault();
        }
        
    }
}
