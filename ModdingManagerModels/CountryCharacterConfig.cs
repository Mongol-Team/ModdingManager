using ModdingManagerData;
using ModdingManagerModels.GfxTypes;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.SubModels;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels
{
    public class CountryCharacterConfig : IConfig
    {
        public IGfx Gfx { get; set; } = new SpriteType(DataDefaultValues.NullImageSource, DataDefaultValues.Null);
        public IGfx SmallGfx { get; set; } = new SpriteType(DataDefaultValues.NullImageSource, DataDefaultValues.Null);
        public Identifier Id { get; set; } = new(DataDefaultValues.Null);
        public ConfigLocalisation Localisation { get; set; } = new();
        public List<ICharacterType> Types { get; set; } = [];
        public string Tag { get; set; } = DataDefaultValues.Null;
        public string AiWillDo { get; set; } = DataDefaultValues.Null;
        
    }
}
