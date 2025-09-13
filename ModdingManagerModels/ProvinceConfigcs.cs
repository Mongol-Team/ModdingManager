using ModdingManagerModels.Args;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class ProvinceConfig : IConfig
    {
        [JsonIgnore]
        public Identifier Id { get; set; }
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
