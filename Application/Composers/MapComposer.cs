using Application.utils.Pathes;
using Models.Configs;
using Models.EntityFiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Composers
{
    public class MapComposer
    {
        public static MapConfig Parse()
        {

            List<ConfigFile<ProvinceConfig>> provinceConfigs = ProvinceComposer.Parse();
            List<ConfigFile<StateConfig>> stateConfigs = StateComposer.Parse();
            List<ConfigFile<StrategicRegionConfig>> strategicRegionConfigs = SRegionComposer.Parse();
            List<ConfigFile<CountryConfig>> countryConfigs = CountryComposer.Parse();
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
                Basic = provinceConfigs.SelectMany(p => p.Entities),
                StrategicRegions = strategicRegionConfigs,
                Countries = countryConfigs,
                MapImage = new Bitmap(bmPath)
            };
            return mapConfig;
        }
    }
}
