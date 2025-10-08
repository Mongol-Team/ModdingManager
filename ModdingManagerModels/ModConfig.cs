using ModdingManagerModels;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObjectCacheData;

namespace ModdingManager.classes.utils
{
    public class ModConfig
    {
        public ModConfig() { }
        private static ModConfig _instance = new();
        public static ModConfig Instance => _instance ??= new ModConfig();
        public List<RuleConfig> Rules { get; set; }
        public List<StateCathegoryConfig> StateCathegories { get; set; }
        public List<RegimentConfig> Regiments { get; set; }
        public List<CountryConfig> Countries { get; set; }
        public List<IdeaConfig> Ideas { get; set; }
        public List<TriggerDefenitionConfig> TriggerDefenitions { get; set; }
        public List<IdeaTagConfig> IdeaTags { get; set; }
        public List<Var> Vars { get; set; }
        public List<StaticModifierConfig> StaticModifiers { get; set; }
        public List<OpinionModifierConfig> OpinionModifiers { get; set; }
        public List<DynamicModifierConfig> DynamicModifiers { get; set; }
        public List<ModifierDefinitionConfig> ModifierDefinitions { get; set; }
        public List<IdeaSlotConfig> IdeaSlots { get; set; }
        public List<IGfx> Gfxes { get; set; }
        public MapConfig Map { get; set; } = new MapConfig();
        public List<TechTreeConfig> TechTreeLedgers { get; set; } = new List<TechTreeConfig>();
        public List<CountryCharacterConfig> Characters { get; set; }
        public List<IdeologyConfig> Ideologies { get; set; }

    }
}
