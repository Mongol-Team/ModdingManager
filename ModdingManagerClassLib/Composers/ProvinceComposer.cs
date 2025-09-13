using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types.TableCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class ProvinceComposer : IComposer
    {
        public ProvinceComposer() { }
        public static List<IConfig> Parse()
        {
            List<IConfig> res = new List<IConfig>();
            List<IConfig> seaProvinces = new List<IConfig>();
            List<IConfig> otherProvinces = new List<IConfig>();
            CsvParser csvParser = new CsvParser(new CsvDefinitionsPattern());
            var defFile = csvParser.Parse(ModPathes.DefinitionPath) as HoiTable;
            foreach (var line in defFile.Values)
            {
               
                try
                {
                    var province = new ProvinceConfig
                    {
                        Id = new Identifier((int)line[0]),
                        Color = (Color)line[1],
                        Type = (ProvinceType)line[2],
                        IsCoastal = (bool)line[3],
                        Terrain = (string)line[4],
                        ContinentId = (int)line[5],
                    };

                    if (province.Type == ProvinceType.sea)
                        seaProvinces.Add(province);
                    else
                        otherProvinces.Add(province);
                }
                catch
                {
                    continue;
                }
            }

            res = seaProvinces.Concat(otherProvinces).ToList();
            return res;
        }
    }
}
