using ModdingManager.classes.utils;
using ModdingManagerModels;
using ModdingManagerModels.Interfaces;

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
        public static CountryConfig GetCountry(this ModConfig mod, string id)
        {
            return mod.Countries.FirstOrDefault(c => c.Id.AsString() == id);
        }
        public static IdeaConfig GetIdea(this ModConfig mod, string id)
        {
            return mod.Ideas.FirstOrDefault(i => i.Id.AsString() == id);
        }
        public static IModifier GetModifier(this ModConfig mod, string id)
        {
            StaticModifierConfig st = mod.StaticModifiers.FirstOrDefault(s => s.Id.AsString() == id);
            if (st != null) return st;
            OpinionModifierConfig op = mod.OpinionModifiers.FirstOrDefault(o => o.Id.AsString() == id);
            if (op != null) return op;
            DynamicModifierConfig dm = mod.DynamicModifiers.FirstOrDefault(d => d.Id.AsString() == id);
            if (dm != null) return dm;
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
