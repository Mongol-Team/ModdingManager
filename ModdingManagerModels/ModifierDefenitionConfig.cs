using ModdingManagerModels.Enums;

using ModdingManagerModels.Types.Utils;

namespace ModdingManagerModels
{
    public class ModifierDefenitionConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ModifierDefenitionValueType ValueType { get; set; } = ModifierDefenitionValueType.Number;
        public int Precision { get; set; } = 1;
        public ModifierDefinitionCathegoryType Cathegory { get; set; } = ModifierDefinitionCathegoryType.Country;
        public ModifierDefenitionColorType ColorType { get; set; } = ModifierDefenitionColorType.Good;
        public bool IsCore { get; set; } = false;
        public ScopeTypes ScopeType { get; set; }

    }
}
