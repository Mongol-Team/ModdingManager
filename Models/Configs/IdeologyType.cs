using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;

namespace Models.Configs
{
    public class IdeologyType : IConfig
    {
        public string Parrent { get; set; }
        public bool CanBeRandomlySelected { get; set; }
        public Color Color { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public IGfx Gfx { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
