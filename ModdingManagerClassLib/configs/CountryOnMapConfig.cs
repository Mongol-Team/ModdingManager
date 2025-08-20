using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModdingManager.classes.configs
{
    public class CountryOnMapConfig
    {
        public CountryOnMapConfig() { }
        public List<StateConfig> States { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public string Tag { get; set; }
        public System.Windows.Media.Color Color { get; set; }
    }
}
