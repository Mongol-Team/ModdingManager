using System.Drawing;

namespace ModdingManagerModels
{
    public class MapConfig : IModel
    {
        public List<StateConfig> States { get; set; }
        public List<ProvinceConfig> Provinces { get; set; }
        public List<StrategicRegionConfig> StrategicRegions { get; set; }
        public List<CountryOnMapConfig> Countries { get; set; }
        public Bitmap Bitmap { get; set; }
        public Point? GetProvincePos(int provinceId)
        {
            var province = Provinces?.FirstOrDefault(p => p.Id == provinceId);
            if (province?.Shape == null)
                return null;

            return province.Shape.Pos;
        }

    }
}
