using Application.utils.Pathes;
using Models.Configs;
using System.Drawing;

namespace Application.Composers
{
    public class MapComposer
    {
        public static MapConfig Parse()
        {
            MapConfig mapConfig = new MapConfig();
            string bmPath = "";
            if (File.Exists(ModPathes.ProvinceImagePath))
            {
                bmPath = ModPathes.ProvinceImagePath;
                mapConfig.IsOverride = true;
            }
            else if (File.Exists(GamePathes.ProvinceImagePath))
            {
                bmPath = GamePathes.ProvinceImagePath;
            }
            else
            {
                return mapConfig;
            }
            
            mapConfig.MapImage = new Bitmap(bmPath);
            return mapConfig;
        }
    }
}
