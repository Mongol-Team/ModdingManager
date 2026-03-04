using Models.Attributes;
using Models.EntityFiles;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System.Drawing;

namespace Models.Configs
{
    [ConfigCreator(ConfigCreatorType.MapCreator)]
    public class MapConfig : IConfig, IPoliticalMap
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; } = new ConfigLocalisation();
        public bool IsCore { get; set; }
        public bool IsOverride { get; set; }
        public string FileFullPath { get; set; }
        public IEnumerable<IBasicMapEntity> Basic { get; set; } = new List<IBasicMapEntity>();
        public List<ConfigFile<StateConfig>> States { get; set; } = new List<ConfigFile<StateConfig>>();
        public List<ConfigFile<StrategicRegionConfig>> StrategicRegions { get; set; } = new List<ConfigFile<StrategicRegionConfig>>();
        public List<ConfigFile<CountryConfig>> Countries { get; set; } = new List<ConfigFile<CountryConfig>>();
        public Bitmap MapImage { get; set; }

        public IEnumerable<(string LayerName, List<IMapEntity> Entities)> GetLayers()
        {
            var stateEntities = States.SelectMany(f => f.Entities).Cast<IMapEntity>().ToList();
            var regionEntities = StrategicRegions.SelectMany(f => f.Entities).Cast<IMapEntity>().ToList();
            var countryEntities = Countries.SelectMany(f => f.Entities).Cast<IMapEntity>().ToList();

            yield return ("States", stateEntities);
            yield return ("StrategicRegions", regionEntities);
            yield return ("Countries", countryEntities);
        }

        public Point? GetProvincePos(int provinceId)
        {
            var province = Basic
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
