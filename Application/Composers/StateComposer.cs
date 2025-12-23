using ModdingManager.managers.@base;
using Application.Debugging;
using Application.Settings;
using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models;
using Models.Types;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using System.Collections.Concurrent;
using System.Diagnostics;
using Application.Extentions;
using System.Linq;
using Models.Types.ObectCacheData;
using Data;
using Models.GfxTypes;

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

            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;

            if (file == null || file.Brackets.Count == 0)
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

            Bracket historyBra = stateBracket.SubBrackets.FirstOrDefault(v => v.Name == "history");
            string ownerTag = historyBra.SubVars.FirstOrDefault(v => v.Name == "owner")?.Value as string;
            List<string> cores = historyBra.SubVars
                .Where(a => a.Name == "add_core")
                .Select(a => a.Value)
                .ToList().ToListString();
            Dictionary<int, int> victoryPoints = new Dictionary<int, int>();
            foreach (HoiArray hoiArray in historyBra.Arrays.Where(a => a.Name == "victory_points"))
            {
                if (hoiArray.Values.Count == 2)
                {
                    victoryPoints[hoiArray.Values[0].ToInt()] = hoiArray.Values[1].ToInt();
                }
                else
                {
                    Logger.AddDbgLog($"Неверный формат victory_points в штате {id} в файле {filePath}", "StateComposer");
                    //todo: handle healing
                }
            }
            Dictionary<BuildingConfig, int> buildings = new Dictionary<BuildingConfig, int>();
            foreach (Bracket buildingBr in stateBracket.SubBrackets.Where(b => b.Name == "buildings"))
            {
                foreach (Var buildingVar in buildingBr.SubVars)
                {
                    var buildingConfig = ModDataStorage.Mod.Buildings.FirstOrDefault(b => b.Id.ToString() == buildingVar.Name);
                    if (buildingConfig != null)
                    {
                        buildings[buildingConfig] = buildingVar.Value.ToInt();
                    }
                    else
                    {
                        Logger.AddDbgLog($"Неизвестное здание {buildingVar.Name} в штате {id} в файле {filePath}", "StateComposer");
                        //todo: handle healing
                    }
                }
            }
            return new StateConfig
            {
                Id = new Identifier(id),
                Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                Provinces = matchedProvinces,
                OwnerTag = ownerTag,
                CoresTag = cores,
                FilePath = file.FilePath,
                VictoryPoints = victoryPoints,
                Buildings = buildings,
                Color = ModManager.GenerateColorFromId(id),
                Manpower = stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower")?.Value as int? ?? 0,
                LocalSupply = stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supply")?.Value as double? ?? 0.0,
                Cathegory = ModDataStorage.Mod.StateCathegories.Where(s => s.Id.ToString() == stateBracket.SubVars.FirstOrDefault(v => v.Name == "category")?.Value as string).First(),
            };
        }
    }
}
