using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class MapComposer : IComposer
    {
        public MapComposer() { }
        public static List<IConfig> Parse()
        {
            List<StateConfig> stateConfigs = StateComposer.Parse().Cast<StateConfig>().ToList();
            List<ProvinceConfig> provinceConfigs = ProvinceComposer.Parse().Cast<ProvinceConfig>().ToList();
            List<StrategicRegionConfig> strategicRegionConfigs = SRegionComposer.Parse().Cast<StrategicRegionConfig>().ToList();
            List<CountryConfig> countryConfigs = CountryComposer.Parse().Cast<CountryConfig>().ToList();
            string bmPath = "";
            if (File.Exists(ModPathes.ProvinceImagePath))
            {
                bmPath = ModPathes.ProvinceImagePath;
            }
            else if (File.Exists(GamePathes.ProvinceImagePath))
            {
                bmPath = GamePathes.ProvinceImagePath;
            }
            else
            {
                throw new FileNotFoundException("Bitmap file not found in mod or game directories.");
            }

            MapConfig mapConfig = new MapConfig
            {
                States = stateConfigs,
                Provinces = provinceConfigs,
                StrategicRegions = strategicRegionConfigs,
                Countries = countryConfigs,
                Bitmap = new Bitmap(bmPath)
            };
            return new List<IConfig> { mapConfig };
        }
    }
}
