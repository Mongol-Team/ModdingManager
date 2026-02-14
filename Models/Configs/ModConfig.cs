using Models.EntityFiles;
using Models.Interfaces;
using Models.Types.ObjectCacheData;
using System.Collections.ObjectModel;

namespace Models.Configs
{
    public class ModConfig
    {
        public ObservableCollection<ConfigFile<RuleConfig>> Rules { get; set; } = new();
        public ObservableCollection<ConfigFile<StateCathegoryConfig>> StateCathegories { get; set; } = new();
        public ObservableCollection<ConfigFile<SubUnitConfig>> SubUnits { get; set; } = new();
        public ObservableCollection<ConfigFile<CountryConfig>> Countries { get; set; } = new();
        public ObservableCollection<ConfigFile<IdeaConfig>> Ideas { get; set; } = new();
        public ObservableCollection<ConfigFile<TriggerDefenitionConfig>> TriggerDefenitions { get; set; } = new();
        public ObservableCollection<ConfigFile<IdeaTagConfig>> IdeaTags { get; set; } = new();
        public ObservableCollection<ConfigFile<StaticModifierConfig>> StaticModifiers { get; set; } = new();
        public ObservableCollection<ConfigFile<OpinionModifierConfig>> OpinionModifiers { get; set; } = new();
        public ObservableCollection<ConfigFile<DynamicModifierConfig>> DynamicModifiers { get; set; } = new();
        public ObservableCollection<ConfigFile<ModifierDefinitionConfig>> ModifierDefinitions { get; set; } = new();
        public ObservableCollection<ConfigFile<IdeaGroupConfig>> IdeaSlots { get; set; } = new();
        public ObservableCollection<ConfigFile<BuildingConfig>> Buildings { get; set; } = new();
        public ObservableCollection<GfxFile<IGfx>> Gfxes { get; set; } = new();
        public ObservableCollection<ConfigFile<TechCategoryConfig>> TechCategories { get; set; } = new();
        public ObservableCollection<ConfigFile<EquipmentConfig>> Equipments { get; set; } = new();
        public MapConfig Map { get; set; } = new();
        public ObservableCollection<ConfigFile<TechTreeConfig>> TechTreeLedgers { get; set; } = new();
        public ObservableCollection<ConfigFile<TechTreeItemConfig>> TechTreeItems { get; set; } = new();
        public ObservableCollection<ConfigFile<CountryCharacterConfig>> Characters { get; set; } = new();
        public ObservableCollection<ConfigFile<IdeologyConfig>> Ideologies { get; set; } = new();
        public ObservableCollection<ConfigFile<CharacterTraitConfig>> CharacterTraits { get; set; } = new();
        public ObservableCollection<ConfigFile<SubUnitGroupConfig>> SubUnitGroups { get; set; } = new();
        public ObservableCollection<ConfigFile<SubUnitCategoryConfig>> SubUnitChategories { get; set; } = new();
        public ObservableCollection<ConfigFile<ResourceConfig>> Resources { get; set; } = new();
    }
}
