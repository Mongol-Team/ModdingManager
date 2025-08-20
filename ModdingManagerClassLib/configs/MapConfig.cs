using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.configs
{
    public class MapConfig
    {
        public MapConfig() { }
        public List<StateConfig> States { get; set; }
        public List<ProvinceConfig> Provinces { get; set; }
        public List<StrategicRegionConfig> StrategicRegions { get; set; }
        public List<CountryOnMapConfig> Countries { get; set; }
        public Bitmap Bitmap { get; set; }
        public System.Windows.Point? GetProvincePos(int provinceId)
        {
            var province = Provinces?.FirstOrDefault(p => p.Id == provinceId);
            if (province?.Shape == null)
                return null;

            return province.Shape.Pos;
        }

    }
}
