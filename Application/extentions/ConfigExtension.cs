using Application.Extensions;
using Models.Configs;
using Models.Interfaces;
using System.Collections.ObjectModel;

namespace Application.Extentions
{
    public static class ConfigExtension
    {
        #region TechTreeMethods
        public static TechTreeItemConfig GetTreeItem(this List<TechTreeConfig> config, string id)
        {
            foreach (var cfg in config)
            {
                var item = cfg.Items.FirstOrDefault(i => i.Id.ToString() == id);
                if (item != null)
                    return item;
            }
            return null;
        }
        public static TechTreeItemConfig GetTreeItem(this ObservableCollection<TechTreeConfig> config, string id)
        {
            foreach (var cfg in config)
            {
                var item = cfg.Items.FirstOrDefault(i => i.Id.ToString() == id);
                if (item != null)
                    return item;
            }
            return null;
        }
        public static void RemoveItem(this List<TechTreeConfig> config, string id)
        {
            foreach (var cfg in config)
            {
                var item = cfg.Items.FirstOrDefault(i => i.Id.ToString() == id);
                if (item != null)
                {
                    cfg.Items.Remove(item);
                    foreach (var it in cfg.Items)
                    {
                        it.ChildOf = null;
                        it.Mutal.RemoveById(id);
                    }
                    return;
                }
            }
        }
        public static TechTreeConfig ReplaceItem(this List<TechTreeConfig> config, TechTreeItemConfig item)
        {
            foreach (var cfg in config)
            {
                var existingItem = cfg.Items.FirstOrDefault(i => i.Id.ToString() == item.Id.ToString());
                if (existingItem != null)
                {
                    int index = cfg.Items.IndexOf(existingItem);
                    cfg.Items[index] = item;
                    return cfg;
                }
            }
            return null;
        }

        public static TechTreeConfig GetTechTree(this ModConfig mod, string id)
        {
            return mod.TechTreeLedgers.FileEntitiesToList()?.FirstOrDefault(t => t.Id.ToString() == id);
        }
        public static TechTreeItemConfig GetTreeItem(this TechTreeConfig config, string id)
        {
            if (config == null || string.IsNullOrEmpty(id))
                return null;

            return config.Items.FirstOrDefault(i => i.Id.ToString() == id);
        }
        public static bool ReplaceItem(this TechTreeConfig config, TechTreeItemConfig item)
        {
            if (config == null || item == null)
                return false;

            var existingItem = config.Items.FirstOrDefault(i => i.Id.ToString() == item.Id.ToString());
            if (existingItem != null)
            {
                int index = config.Items.IndexOf(existingItem);
                config.Items[index] = item;
                return true;
            }

            return false;
        }
        public static void RemoveItem(this TechTreeConfig config, string id)
        {
            if (config == null || string.IsNullOrEmpty(id))
                return;

            var item = config.Items.FirstOrDefault(i => i.Id.ToString() == id);
            if (item != null)
            {
                config.Items.Remove(item);
                foreach (var it in config.Items)
                {
                    it.ChildOf = null;
                    it.Mutal.RemoveById(id); // предполагается, что Mutal — это список
                }
            }
        }

        #endregion
        #region ModCfgMethods
        public static CountryConfig GetCountry(this ModConfig mod, string id)
        {
            return mod.Countries.FileEntitiesToList()?.FirstOrDefault(c => c.Id.ToString() == id);
        }
        public static IdeaConfig GetIdea(this ModConfig mod, string id)
        {
            return mod.Ideas.FileEntitiesToList()?.FirstOrDefault(i => i.Id.ToString() == id);
        }
        public static IModifier GetModifier(this ModConfig mod, string id)
        {
            StaticModifierConfig st = mod.StaticModifiers.FileEntitiesToList()?.FirstOrDefault(s => s.Id.ToString() == id);
            if (st != null) return st;
            OpinionModifierConfig op = mod.OpinionModifiers.FileEntitiesToList()?.FirstOrDefault(o => o.Id.ToString() == id);
            if (op != null) return op;
            DynamicModifierConfig dm = mod.DynamicModifiers.FileEntitiesToList()?.FirstOrDefault(d => d.Id.ToString() == id);
            if (dm != null) return dm;
            return null;
        }
        public static StateConfig GetState(this ModConfig mod, int id)
        {
            return mod.Map.States.FileEntitiesToList()?.FirstOrDefault(s => s.Id.ToInt() == id);
        }
        public static IdeologyConfig GetIdeology(this ModConfig mod, string id)
        {
            return mod.Ideologies.FileEntitiesToList().FirstOrDefault();
        }
        #endregion
    }
}
