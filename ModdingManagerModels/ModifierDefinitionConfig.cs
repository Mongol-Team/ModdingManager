using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class ModifierDefinitionConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public ModifierDefenitionValueType ValueType { get; set; } = ModifierDefenitionValueType.Number;
        public int Precision { get; set; } = 1;
        public ModifierDefinitionCathegoryType Cathegory { get; set; } = ModifierDefinitionCathegoryType.Country;
        public ModifierDefenitionColorType ColorType { get; set; } = ModifierDefenitionColorType.Good;
        public bool IsCore { get; set; } = false;
        public ScopeTypes ScopeType { get; set; }
        public string FilePath { get; set; }
    }
}
