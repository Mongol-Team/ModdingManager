using Data;
using Models.Attributes;
using Models.GfxTypes;
using Models.Interfaces;
using Models.SubModels;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class CountryCharacterConfig : IConfig
    {
        public IGfx Gfx { get; set; } = new SpriteType(DataDefaultValues.NullImageSource, DataDefaultValues.Null);
        public IGfx SmallGfx { get; set; } = new SpriteType(DataDefaultValues.NullImageSource, DataDefaultValues.Null);
        public Identifier Id { get; set; } = new(DataDefaultValues.Null);
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public ConfigLocalisation Localisation { get; set; } = new();
        public List<ICharacterType> Types { get; set; } = [];
        public string Tag { get; set; } = DataDefaultValues.Null;
        public string AiWillDo { get; set; } = DataDefaultValues.Null;
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
