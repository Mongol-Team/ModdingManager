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
    public class EquipmentConfig : IConfig
    {
        public Identifier Id { get; set ; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public int Year { get; set; }
        public string CanBeProduced { get; set; } //todo: raw trigger data
        public bool IsArchetype { get; set; }
        public bool IsBuidable { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; }
        public string GroupBy { get; set; }
        public EquipmentInterfaceCategory InterfaceType { get; set; }
        public Dictionary<ResourceConfig, int> Cost { get; set; }
        public Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
