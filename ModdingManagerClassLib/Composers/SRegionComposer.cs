using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class SRegionComposer : IComposer
    {
        public SRegionComposer() { }
        public static List<IConfig> Parse()
        {
            var strategicMap = new Dictionary<int, StrategicRegionConfig>();
            var res = new List<IConfig>();
            string[] priorityFolders = {
                ModPathes.StrategicRegionPath,
                GamePathes.StrategicRegionsPath,
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;

                    foreach (var regionBracket in file.Brackets)
                    {
                        var idVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                        if (idVar == null || !int.TryParse(idVar.Value as string, out int id) || strategicMap.ContainsKey(id))
                            continue;

                        HoiArray provincesBracket = regionBracket.Arrays.FirstOrDefault(b => b.Name == "provinces");
                        if (provincesBracket == null) continue;

                        var matchedProvinces = ModConfig.Instance.Map.Provinces
                            .Where(p => provincesBracket.Values.Contains(p.Id))
                            .ToList();

                        strategicMap[id] = new StrategicRegionConfig
                        {
                            Id = id,
                            Provinces = matchedProvinces,
                            FilePath = file.FilePath,
                            Color = ModManager.GenerateColorFromId(id)
                        };
                    }
                }
            }
            foreach (var region in strategicMap.Values)
            {
                res.Add(region);
            }
            return res;
        }
    }
}
