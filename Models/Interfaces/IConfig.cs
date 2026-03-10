using Models.Types.LocalizationData;
using Models.Types.Utils;

namespace Models.Interfaces
{
    public interface IConfig
    {
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
    }
}
