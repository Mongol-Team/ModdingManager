using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModdingManager.classes.configs
{
    public class StateConfig
    {
        [JsonIgnore]
        public List<ProvinceConfig> Provinces { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int? Id { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        [JsonIgnore]
        public string FilePath { get; set; }
        [JsonIgnore]
        public string LocalizationKey { get; set; } = string.Empty;

    }
}
