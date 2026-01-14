using Models.Attributes;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class BuildingConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public bool AffectsEnergy { get; set; }
        public int BaseCost { get; set; }
        public int PerLevelCost { get; set; }
        public int PerControlledBuildingExtraCost { get; set; }
        public int MaxStateLevel { get; set; }
        public int MaxProvinceLevel { get; set; }
        public string Group { get; set; }
        public bool SharesSlots { get; set; }
        public BuildingConfig ExcludeWith { get; set; }
        public int Health { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public double DamageFactor { get; set; }
        public bool AlliedBuild { get; set; }
        public bool OnlyCoastal { get; set; }
        public int SpecialIcon {  get; set; }
        public bool DisabledInDmZones { get; set; }
        public bool NeedsSupply { get; set; }
        public bool NeedsDetection { get; set; }
        public bool HideIfMissingTech { get; set; }
        public bool IsBuildable { get; set; }
        public bool OnlyDisplayIfExists { get; set; }
        public IntelegenceType IntelType { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> StateModifiers { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> CountryModifiers { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> ProvineDamageModifiers { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> StateDamageModifiers { get; set; }
        public int ShowOnMap { get; set; }
        public int ShowOnMapMeshes { get; set; }
        public bool AlwaysShown { get; set; }
        public bool HasDestroyedMesh { get; set; }
        public bool Centered { get; set; }
        public IGfx Gfx { get; set; }

    }
}
