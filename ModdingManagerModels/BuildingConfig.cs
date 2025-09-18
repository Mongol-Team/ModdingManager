using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels
{
    public class BuildingConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public int BaseCost { get; set; }
        public int PerLevelCost { get; set; }
        public int PerControlledBuildingExtraCost { get; set; }
        public int MaxStateLevel { get; set; }
        public int MaxProvinceLevel { get; set; }   
        public string Group { get; set; }
        public BuildingConfig ExcludeWith { get; set; }
        public int Helth { get; set; }
        public double DamageFactor { get; set; }
        public bool AlliedBuild { get; set; }
        public bool OnlyCoastal { get; set; }
        public bool DisabledInDmZones { get; set; }
        public bool NeedsSupply { get; set; }
        public bool HideIfMissingTech { get; set; }
        public List<BuildingConfig> RaidGroupBuildings { get; set; }
        public bool IsBuildable { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> ProvineDamageModifiers { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> StateDamageModifiers { get; set; }
        public int ShowOnMap { get; set; }
        public int ShowOnMapMeshes { get; set; }
        public bool AlwaysShown { get; set; }
        public bool HasDestroyedMesh { get; set; }
        public bool Centered { get; set; }

    }
}
