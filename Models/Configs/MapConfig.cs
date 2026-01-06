using Models.Attributes;
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
        public ConfigLocalisation Localisation { get; set; }
        public List<StateConfig> States { get; set; }
        public List<ProvinceConfig> Provinces { get; set; }
        public List<StrategicRegionConfig> StrategicRegions { get; set; }
        public List<CountryConfig> Countries { get; set; }
        public Bitmap Bitmap { get; set; }
        public Point? GetProvincePos(int provinceId)
        {
            var province = Provinces?.FirstOrDefault(p => p.Id.ToInt() == provinceId);
            if (province?.Shape == null)
                return null;

            return province.Shape.Pos;
        }

    }
}
