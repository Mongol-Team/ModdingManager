using Models.Attributes;
using Models.EntityFiles;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.MapCreator)]
    public class MapConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; } = new ConfigLocalisation();
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public List<ConfigFile<StateConfig>> States { get; set; } = new List<ConfigFile<StateConfig>>();
        public List<ConfigFile<ProvinceConfig>> Provinces { get; set; } = new List<ConfigFile<ProvinceConfig>>();
        public List<ConfigFile<StrategicRegionConfig>> StrategicRegions { get; set; } = new List<ConfigFile<StrategicRegionConfig>>();
        public List<ConfigFile<CountryConfig>> Countries { get; set; } = new List<ConfigFile<CountryConfig>>();
        public Bitmap Bitmap { get; set; }
        public Point? GetProvincePos(int provinceId)
        {
            var province = Provinces.SelectMany(cf => cf.Entities)
                .FirstOrDefault(e => e.Id.ToString() == provinceId.ToString());
            if (province?.Shape == null)
                return null;

            return province.Shape.Pos;
        }
        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
