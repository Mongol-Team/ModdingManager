using ModdingManager.classes.args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ModdingManager.classes.configs
{
    public class ProvinceConfig
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public bool IsCoastal { get; set; }
        public int ContinentId { get; set; }
        public string Type { get; set; }
        public int VictoryPoints { get; set; }
        public string Terrain { get; set; }
        public ProvinceShapeArg Shape { get; set; }
    }
}
