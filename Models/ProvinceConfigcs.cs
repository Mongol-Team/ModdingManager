using Models.Args;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Models
{
    public class ProvinceConfig : IConfig
    {
        [JsonIgnore]
        public Identifier Id { get; set; }
        public IGfx Gfx { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsCoastal { get; set; }
        public int ContinentId { get; set; }
        public ProvinceType Type { get; set; }
        public int VictoryPoints { get; set; }
        public string Terrain { get; set; }
        public ProvinceShapeArg Shape { get; set; }
    }
}
