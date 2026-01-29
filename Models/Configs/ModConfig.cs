using Models.Interfaces;
using Models.Types.ObjectCacheData;
using System.Collections.ObjectModel;

namespace Models.Configs
{
    public class ModConfig
    {
        public ObservableCollection<RuleConfig> Rules { get; set; } = new();
        public ObservableCollection<StateCathegoryConfig> StateCathegories { get; set; } = new();
        public ObservableCollection<SubUnitConfig> SubUnits { get; set; } = new();
        public ObservableCollection<CountryConfig> Countries { get; set; } = new();
        public ObservableCollection<IdeaConfig> Ideas { get; set; } = new();
        public ObservableCollection<TriggerDefenitionConfig> TriggerDefenitions { get; set; } = new();
        public ObservableCollection<IdeaTagConfig> IdeaTags { get; set; } = new();
        public ObservableCollection<StaticModifierConfig> StaticModifiers { get; set; } = new();
        public ObservableCollection<OpinionModifierConfig> OpinionModifiers { get; set; } = new();
        public ObservableCollection<DynamicModifierConfig> DynamicModifiers { get; set; } = new();
        public ObservableCollection<ModifierDefinitionConfig> ModifierDefinitions { get; set; } = new();
        public ObservableCollection<IdeaGroupConfig> IdeaSlots { get; set; } = new();
        public ObservableCollection<BuildingConfig> Buildings { get; set; } = new();
        public ObservableCollection<IGfx> Gfxes { get; set; } = new();
        public ObservableCollection<TechCategoryConfig> TechCategories { get; set; } = new();
        public ObservableCollection<EquipmentConfig> Equipments { get; set; } = new();
        public MapConfig Map { get; set; } = new();
        public ObservableCollection<TechTreeConfig> TechTreeLedgers { get; set; } = new();
        public ObservableCollection<TechTreeItemConfig> TechTreeItems { get; set; } = new();
        public ObservableCollection<CountryCharacterConfig> Characters { get; set; } = new();
        public ObservableCollection<IdeologyConfig> Ideologies { get; set; } = new();
        public ObservableCollection<CharacterTraitConfig> CharacterTraits { get; set; } = new();
        public ObservableCollection<SubUnitGroupConfig> SubUnitGroups { get; set; } = new();
        public ObservableCollection<SubUnitCategoryConfig> SubUnitChategories { get; set; } = new();
        
    }
}
