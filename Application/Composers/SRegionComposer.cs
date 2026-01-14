using Application.utils.Pathes;
using Models.Configs;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;

namespace Application.Composers
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

                foreach (string FileFullPath in files)
                {
                    var configs = ParseConfig(FileFullPath, strategicMap);
                    foreach (var config in configs)
                    {
                        config.FileFullPath = FileFullPath;
                        strategicMap[config.Id.ToInt()] = config;
                    }
                }
            }

            res.AddRange(strategicMap.Values);
            return res;
        }

        private static List<StrategicRegionConfig> ParseConfig(string FileFullPath, Dictionary<int, StrategicRegionConfig> existingMap)
        {
            var result = new List<StrategicRegionConfig>();
            if (new TxtParser(new TxtPattern()).Parse(FileFullPath) is not HoiFuncFile file) return result;

            foreach (var regionBracket in file.Brackets)
            {
                var idVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "id");
                if (idVar == null || !int.TryParse(idVar.Value.ToString(), out int id) || existingMap.ContainsKey(id))
                    continue;

                var keyVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "name");
                var provincesBracket = regionBracket.Arrays.FirstOrDefault(b => b.Name == "provinces");
                if (provincesBracket == null) continue;

                var matchedProvinces = ModDataStorage.Mod.Map.Provinces
                    .Where(p => provincesBracket.Values.Contains(p.Id))
                    .ToList();

                result.Add(new StrategicRegionConfig
                {
                    Id = new Identifier(id),
                    Provinces = matchedProvinces,
                    FileFullPath = file.FileFullPath,
                    Color = System.Drawing.Color.FromArgb((byte)((id * 53) % 255), (byte)((id * 97) % 255), (byte)((id * 151) % 255)),
                    LocKey = keyVar?.Name ?? string.Empty,
                });
            }

            return result;
        }
    }
}