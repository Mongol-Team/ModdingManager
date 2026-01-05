using Application.Debugging;
using Application.utils.Pathes;
using Models.Configs;
using Models.Types;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Application.Composers
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
                    var stateConfig = ParseFile(file);
                    result.Add(stateConfig);
                }

                if (!result.IsEmpty)
                    break;
                else
                {
                    Logger.AddDbgLog($"[⚠️] No state files found in mod folder: {folder}", "IdeologyComposer");
                }
            }

            watch.Stop();
            return result.ToList();
        }
        public static StateConfig ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.AddDbgLog($"Файл не найден: {filePath}", "IdeologyComposer");
                return null;
            }


            if (new TxtParser(new TxtPattern()).Parse(filePath) is not HoiFuncFile file || file.Brackets.Count == 0)
            {
                Logger.AddDbgLog($"Не удалось распарсить файл: {filePath}", "IdeologyComposer");
                return null;
            }

            var stateBracket = file.Brackets.First();

            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            if (idVar == null || !int.TryParse(idVar.Value.ToString(), out int id))
            {
                Logger.AddDbgLog($"Не удалось извлечь ID из файла: {filePath}", "IdeologyComposer");
                return null;
            }

            var provincesArray = stateBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            var provinceIds = provincesArray?.Values
                .OfType<object>()
                .Select(v => v is HoiReference hr ? hr.Value : v)
                .OfType<int>()
                .ToList() ?? new List<int>();
            var matchedProvinces = ModDataStorage.Mod.Map.Provinces
                .Where(p => provinceIds.Contains(p.Id.ToInt()))
                .ToList();

            return new StateConfig
            {
                Id = new Identifier(id),
                Provinces = matchedProvinces,
                FilePath = file.FilePath,
                Color = System.Drawing.Color.FromArgb((byte)((id * 53) % 255), (byte)((id * 97) % 255), (byte)((id * 151) % 255)),
                Manpower = stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower")?.Value as int? ?? 0,
                LocalSupply = stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supply")?.Value as double? ?? 0.0,
                Cathegory = ModDataStorage.Mod.StateCathegories.Where(s => s.Id.ToString() == stateBracket.SubVars.FirstOrDefault(v => v.Name == "category")?.Value as string).First(),
            };
        }
    }
}
