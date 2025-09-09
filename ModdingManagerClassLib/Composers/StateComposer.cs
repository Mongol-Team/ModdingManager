using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types;
using ModdingManagerModels.Types.ObjectCacheData;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ModdingManagerClassLib.Composers
{
    public class StateComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            var watch = Stopwatch.StartNew();

            var result = new ConcurrentBag<IConfig>();

            string[] priorityFolders = {
        ModPathes.StatesPath,
        GamePathes.StatesPath,
    };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                    continue;

                var files = Directory.GetFiles(folder);

                //Parallel.ForEach(files, file =>
                //{
                //    var stateConfig = ParseStateConfig(file);
                //    result.Add(stateConfig);
                //});
                foreach (var file in files)
                {
                    var stateConfig = ParseStateConfig(file);
                    result.Add(stateConfig);
                }

                if (!result.IsEmpty)
                    break;
                else
                {
                    Logger.AddLog($"[⚠️] No state files found in mod folder: {folder}");
                }
            }

            watch.Stop();
            return result.ToList();
        }
        public static StateConfig ParseStateConfig(string filePath)
        {

            if (!File.Exists(filePath))
            {
                Logger.AddLog($"Файл не найден: {filePath}");
                return null;
            }

            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;

            if (file == null || file.Brackets.Count == 0)
            {
                Logger.AddLog($"Не удалось распарсить файл: {filePath}");
                return null;
            }

            var stateBracket = file.Brackets.First();

            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            var fim = idVar.Value.ToString();
            if (idVar == null || !int.TryParse(idVar.Value.ToString(), out int id))
            {
                Logger.AddLog($"Не удалось извлечь ID из файла: {filePath}");
                return null;
            }

            var provincesArray = stateBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            var provinceIds = provincesArray?.Values
                .OfType<object>()
                .Select(v => v is HoiReference hr ? hr.Value : v)
                .OfType<int>()
                .ToList() ?? new List<int>();
            var matchedProvinces = ModManager.CurrentConfig.Map.Provinces
                .Where(p => provinceIds.Contains(p.Id))
                .ToList();

            return new StateConfig
            {
                Id = id,
                Provinces = matchedProvinces,
                FilePath = file.FilePath,
                LocalizationKey = stateBracket.SubVars.FirstOrDefault(v => v.Name == "name")?.Value as string ?? $"state_{id}",
                Color = ModManager.GenerateColorFromId(id),
                Manpower = stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower")?.Value as int?,
                LocalSupply = stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supply")?.Value as double? ?? 0.0,
                //Cathegory = ModManager.CurrentConfig.StateCathegories.Where(s => s.Id == stateBracket.SubVars.FirstOrDefault(v => v.Name == "category")?.Value as string).First(),
            };
        }
    }
}
