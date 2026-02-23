using Models.Attributes;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.MapCreator)]
    public class StrategicRegionConfig : IConfig, IMapEntity
    {
        public IGfx Gfx { get; set; }
        public List<ProvinceConfig> Provinces { get; set; }
        public string LocKey { get; set; }
        [JsonIgnore]
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public Color? Color { get; set; }
        public string FileFullPath { get; set; }
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }

        public void AddChild(object child)
        {
            Provinces.Add((ProvinceConfig)child);
        }

        public IEnumerable<IBasicMapEntity> GetAllBasicEntities()
        {
            return Provinces;
        }

        public IEnumerable<object> GetChildren()
        {
            return Provinces;
        }

        public void RemoveChild(object child)
        {
            Provinces.Remove((ProvinceConfig)child);
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
