using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class RegimentConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public RegimentConfig() { }
        public Identifier Id { get; set; }

        [JsonIgnore]
        public ConfigLocalisation Localisation { get; set; }
        public List<string> Categories { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
