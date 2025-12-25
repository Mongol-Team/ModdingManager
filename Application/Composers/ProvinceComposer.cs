using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models;
using Models.Enums;
using Models.Types.TableCacheData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Composers
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
            HoiTable defFile = csvParser.Parse(ModPathes.DefinitionPath) as HoiTable;
            foreach (var line in defFile.Values)
            {

                try
                {
                    if (line.Count < 6)
                    {
                        continue;
                    }
                        
                    if ((int)line[0] == 0)
                    {
                        
                    }
                    var province = new ProvinceConfig
                    {
                        Id = new Identifier((int)line[0]),
                        Color = (Color)line[1],
                        Type = (ProvinceType)line[2],
                        IsCoastal = (bool)line[3],
                        Terrain = (string)line[4],
                        ContinentId = (int)line[5],
                    };

                    if (province.Type == ProvinceType.Sea)
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
