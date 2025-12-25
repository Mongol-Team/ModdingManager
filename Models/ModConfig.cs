using Models;
using Models.Interfaces;
using Models.Types.ObjectCacheData;

namespace ModdingManager.classes.utils
{
    public class ModConfig
    {
        public List<RuleConfig> Rules { get; set; } = new();
        public List<StateCathegoryConfig> StateCathegories { get; set; } = new();
        public List<SubUnitConfig> SubUnits { get; set; } = new();
        public List<CountryConfig> Countries { get; set; } = new();
        public List<IdeaConfig> Ideas { get; set; } = new();
        public List<TriggerDefenitionConfig> TriggerDefenitions { get; set; } = new();
        public List<IdeaTagConfig> IdeaTags { get; set; } = new();
        public List<Var> Vars { get; set; } = new();
        public List<StaticModifierConfig> StaticModifiers { get; set; } = new();
        public List<OpinionModifierConfig> OpinionModifiers { get; set; } = new();
        public List<DynamicModifierConfig> DynamicModifiers { get; set; } = new();
        public List<ModifierDefinitionConfig> ModifierDefinitions { get; set; } = new();
        public List<IdeaGroupConfig> IdeaSlots { get; set; } = new();
        public List<BuildingConfig> Buildings { get; set; } = new();
        public List<IGfx> Gfxes { get; set; } = new();
        public List<TechCategoryConfig> TechCategories { get; set; } = new();
        public List<EquipmentConfig> Equipments { get; set; } = new();
        public MapConfig Map { get; set; } = new();
        public List<TechTreeConfig> TechTreeLedgers { get; set; } = new();
        public List<TechTreeItemConfig> TechTreeItems { get; set; } = new();
        public List<CountryCharacterConfig> Characters { get; set; } = new();
        public List<IdeologyConfig> Ideologies { get; set; } = new();
        public List<CharacterTraitConfig> CharacterTraits { get; set; } = new();
        public List<SubUnitGroupConfig> SubUnitGroups { get; set; } = new();
        public List<SubUnitCategoryConfig> SubUnitChategories { get; set; } = new();
    }
}
