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

                foreach (var file in files)
                {
                    var stateConfig = ParseFile(file);
                    stateConfig.FileFullPath = file;
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
        public static StateConfig? ParseFile(string FileFullPath)
        {
            if (string.IsNullOrWhiteSpace(FileFullPath) || !File.Exists(FileFullPath))
            {
                Logger.AddDbgLog($"Файл не найден или путь пуст: {FileFullPath}", "IdeologyComposer");
                return null;
            }

            if (!(new TxtParser(new TxtPattern()).Parse(FileFullPath) is HoiFuncFile file) || file.Brackets.Count == 0)
            {
                Logger.AddDbgLog($"Не удалось распарсить файл: {FileFullPath}", "IdeologyComposer");
                return null;
            }

            var stateBracket = file.Brackets.FirstOrDefault();
            if (stateBracket == null)
            {
                Logger.AddDbgLog($"Нет корневого блока в файле: {FileFullPath}", "IdeologyComposer");
                return null;
            }

            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            if (idVar?.Value == null || !int.TryParse(idVar.Value.ToString(), out int id))
            {
                Logger.AddDbgLog($"Не удалось извлечь ID из файла: {FileFullPath}", "IdeologyComposer");
                return null;
            }

            // Provinces
            var provincesArray = stateBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            var provinceIds = provincesArray?.Values?
                .Select(v => v is HoiReference hr ? hr.Value : v)
                .OfType<int>()
                .ToList() ?? new List<int>();

            var matchedProvinces = ModDataStorage.Mod?.Map?.Provinces?
                .Where(p => provinceIds.Contains(p.Id.ToInt()))
                .ToList() ?? new List<ProvinceConfig>();

            // History
            var historyBra = stateBracket.SubBrackets.FirstOrDefault(v => v.Name == "history");
            string ownerTag = historyBra?.SubVars.FirstOrDefault(v => v.Name == "owner")?.Value as string ?? string.Empty;

            var cores = historyBra?.SubVars
                .Where(a => a.Name == "add_core")
                .Select(a => a.Value?.ToString() ?? string.Empty)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? new List<string>();

            // Victory Points
            var victoryPoints = new Dictionary<int, int>();
            if (historyBra != null)
            {
                foreach (HoiArray hoiArray in historyBra.Arrays.Where(a => a.Name == "victory_points"))
                {
                    if (hoiArray.Values.Count == 2)
                    {
                        int key = hoiArray.Values[0].ToInt();
                        int val = hoiArray.Values[1].ToInt();
                        victoryPoints[key] = val;
                    }
                    else
                    {
                        Logger.AddDbgLog($"Неверный формат victory_points в штате {id} в файле {FileFullPath}", "StateComposer");
                    }
                }
            }

            // Buildings
            var buildings = new Dictionary<BuildingConfig, int>();
            foreach (var buildingBr in stateBracket.SubBrackets.Where(b => b.Name == "buildings"))
            {
                foreach (var buildingVar in buildingBr.SubVars)
                {
                    var buildingConfig = ModDataStorage.Mod?.Buildings?.FirstOrDefault(b => b.Id.ToString() == buildingVar.Name);
                    if (buildingConfig != null)
                    {
                        buildings[buildingConfig] = buildingVar.Value.ToInt();
                    }
                    else
                    {
                        Logger.AddDbgLog($"Неизвестное здание {buildingVar.Name} в штате {id} в файле {FileFullPath}", "StateComposer");
                    }
                }
            }

            // Category
            string categoryStr = stateBracket.SubVars.FirstOrDefault(v => v.Name == "category")?.Value as string ?? string.Empty;
            var category = ModDataStorage.Mod?.StateCathegories?.FirstOrDefault(s => s.Id.ToString() == categoryStr);

            if (category == null)
            {
                Logger.AddDbgLog($"Не найдена категория '{categoryStr}' для штата {id} в файле {FileFullPath}", "StateComposer");
                return null; // или можно вернуть дефолтную категорию
            }

            return new StateConfig
            {
                Id = new Identifier(id),
                Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                Provinces = matchedProvinces,
                OwnerTag = ownerTag,
                CoresTag = cores,
                FileFullPath = file.FileFullPath,
                VictoryPoints = victoryPoints,
                Buildings = buildings,
                Color = System.Drawing.Color.FromArgb((byte)((id * 53) % 255), (byte)((id * 97) % 255), (byte)((id * 151) % 255)),
                Manpower = stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower")?.Value?.ToInt() ?? DataDefaultValues.NullInt,
                LocalSupply = stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supply")?.Value?.ToDouble() ?? DataDefaultValues.NullInt,
                Cathegory = category,
            };
        }

    }
}
