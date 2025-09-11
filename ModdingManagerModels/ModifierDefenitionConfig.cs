using ModdingManagerModels.Enums;
using ModdingManagerModels.Interfaces;

namespace ModdingManagerModels
{
    public class ModifierDefenitionConfig : IConfig, IFimoz
    {
        public string Name { get; set; }
        public ModifierDefenitionValueType ValueType { get; set; } = ModifierDefenitionValueType.Number;
        public int Precision { get; set; } = 1;
        public ModifierDefinitionCathegoryType Cathegory { get; set; } = ModifierDefinitionCathegoryType.Country;
        public ModifierDefenitionColorType ColorType { get; set; } = ModifierDefenitionColorType.Good;
        public ScopeTypes ScopeType { get; set; }

    }
}
