using ModdingManager.classes.utils;
using ModdingManager.managers.@base;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;

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
                GamePathes.StrategicRegionPath,
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    var configs = ParseConfig(filePath, strategicMap);
                    foreach (var config in configs)
                    {
                        strategicMap[config.Id.AsInt()] = config;
                    }
                }
            }

            res.AddRange(strategicMap.Values);
            return res;
        }

        private static List<StrategicRegionConfig> ParseConfig(string filePath, Dictionary<int, StrategicRegionConfig> existingMap)
        {
            var result = new List<StrategicRegionConfig>();
            var file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;
            if (file == null) return result;

            foreach (var regionBracket in file.Brackets)
            {
                var idVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                if (idVar == null || !int.TryParse(idVar.Value.ToString(), out int id) || existingMap.ContainsKey(id))
                    continue;

                var keyVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "name");
                var provincesBracket = regionBracket.Arrays.FirstOrDefault(b => b.Name == "provinces");
                if (provincesBracket == null) continue;

                var matchedProvinces = ModConfig.Instance.Map.Provinces
                    .Where(p => provincesBracket.Values.Contains(p.Id))
                    .ToList();

                result.Add(new StrategicRegionConfig
                {
                    Id = new Identifier(id),
                    Provinces = matchedProvinces,
                    FilePath = file.FilePath,
                    Color = ModManager.GenerateColorFromId(id),
                    LocKey = keyVar?.Name ?? string.Empty,
                });
            }

            return result;
        }
    }
}