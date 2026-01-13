using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Configs
{
    public class ModifierDefinitionConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public ModifierDefenitionValueType ValueType { get; set; } = ModifierDefenitionValueType.Number;
        public int Precision { get; set; } = 1;
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public ModifierDefinitionCathegoryType Cathegory { get; set; } = ModifierDefinitionCathegoryType.Country;
        public ModifierDefenitionColorType ColorType { get; set; } = ModifierDefenitionColorType.Good;
        public ScopeTypes ScopeType { get; set; }
    }
}
