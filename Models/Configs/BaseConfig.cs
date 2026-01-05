using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Configs
{
    public class BaseConfig : IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
    }
}
