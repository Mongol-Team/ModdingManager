using System.Drawing;
using System.Text.Json.Serialization;

namespace ModdingManagerModels
{
    public class RegimentConfig : IModel
    {
        public RegimentConfig() { }
        public string Name { get; set; }

        [JsonIgnore]
        public Image Icon { get; set; }

        public List<string> Categories { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
