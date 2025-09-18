using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels
{
    public class MapConfig : IConfig
    {
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
