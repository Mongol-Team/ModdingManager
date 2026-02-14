using Application.Debugging;
using Application.Extensions;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObectCacheData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class StateComposer
    {
        /// <summary>
        /// Парсит все файлы штатов и возвращает список файлов (ConfigFile<StateConfig>)
        /// </summary>
        public static List<ConfigFile<StateConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var stateFiles = new List<ConfigFile<StateConfig>>();

            string[] priorityFolders =
            {
                ModPathes.StatesPath,
                GamePathes.StatesPath
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Logger.AddLog($"Директория штатов не найдена: {folder}", LogLevel.Info);
                    continue;
                }

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = folder.StartsWith(ModPathes.StatesPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            stateFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл штатов: {configFile.FileName} → {configFile.Entities.Count} штатов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[StateComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }

                // Если в моде что-то нашли — не ищем в игре
                if (stateFiles.Any()) break;
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг штатов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {stateFiles.Count}, штатов всего: {stateFiles.Sum(f => f.Entities.Count)}");

            return stateFiles;
        }

        /// <summary>
        /// Парсит один файл штатов (принимает HoiFuncFile)
        /// </summary>
        public static ConfigFile<StateConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<StateConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            if (hoiFuncFile == null || hoiFuncFile.Brackets.Count == 0)
            {
                Logger.AddDbgLog($"Файл пуст или не распарсился: {fileFullPath}");
                return configFile;
            }

            foreach (Bracket stateBracket in hoiFuncFile.Brackets)
            {
                var state = ParseState(stateBracket, fileFullPath, isOverride);
                if (state != null)
                {
                    configFile.Entities.Add(state);
                    Logger.AddDbgLog($"  → добавлен штат: {state.Id}");
                }
            }

            return configFile;
        }

        private static StateConfig? ParseState(Bracket stateBracket, string fileFullPath, bool isOverride)
        {
            var idVar = stateBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            if (idVar?.Value == null || !int.TryParse(idVar.Value.ToString(), out int id))
            {
                Logger.AddDbgLog($"Штат без валидного id в файле: {fileFullPath}");
                return null;
            }

            var state = new StateConfig
            {
                Id = new Identifier(id),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                Provinces = new List<ProvinceConfig>(),
                VictoryPoints = new Dictionary<int, int>(),
                Buildings = new Dictionary<BuildingConfig, int>(),
                CoresTag = new List<string>(),
                Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                Manpower = DataDefaultValues.NullInt,
                LocalSupply = DataDefaultValues.NullInt
            };

            // Провинции
            var provincesArray = stateBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            if (provincesArray?.Values != null)
            {
                foreach (var provIdObj in provincesArray.Values)
                {
                    if (provIdObj == null) continue;

                    string provIdStr = provIdObj.ToString();
                    if (!int.TryParse(provIdStr, out int provId)) continue;

                    var province = ModDataStorage.Mod.Map.Provinces.SearchConfigInFile(provId.ToString());
                    if (province != null)
                        state.Provinces.Add(province);
                    else
                        Logger.AddDbgLog($"Провинция {provId} не найдена для штата {id} (файл: {fileFullPath})");
                }
            }

            // История (owner, cores, victory_points, buildings)
            var historyBr = stateBracket.SubBrackets.FirstOrDefault(b => b.Name == "history");
            if (historyBr != null)
            {
                // Owner
                state.OwnerTag = historyBr.SubVars.FirstOrDefault(v => v.Name == "owner")?.Value?.ToString() ?? string.Empty;

                // Cores
                var coreVars = historyBr.SubVars.Where(v => v.Name == "add_core");
                state.CoresTag = coreVars.Select(v => v.Value?.ToString() ?? string.Empty)
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .ToList();

                // Victory Points
                foreach (HoiArray vpArray in historyBr.Arrays.Where(a => a.Name == "victory_points"))
                {
                    if (vpArray.Values.Count >= 2)
                    {
                        int provId = vpArray.Values[0].ToInt();
                        int points = vpArray.Values[1].ToInt();
                        state.VictoryPoints[provId] = points;
                    }
                    else
                    {
                        Logger.AddDbgLog($"Неверный формат victory_points в штате {id} (файл: {fileFullPath})");
                    }
                }

                // Buildings
                foreach (Bracket buildingBr in historyBr.SubBrackets.Where(b => b.Name == "buildings"))
                {
                    foreach (Var buildingVar in buildingBr.SubVars)
                    {
                        var building = ModDataStorage.Mod.Buildings.SearchConfigInFile(buildingVar.Name);
                        if (building != null)
                            state.Buildings.Add(building, buildingVar.Value.ToInt());
                        else
                            Logger.AddDbgLog($"Неизвестное здание {buildingVar.Name} в штате {id} (файл: {fileFullPath})");
                    }
                }
            }

            // Категория штата
            string categoryStr = stateBracket.SubVars.FirstOrDefault(v => v.Name == "state_category")?.Value?.ToString() ?? string.Empty;
            var category = ModDataStorage.Mod.StateCathegories.SearchConfigInFile(categoryStr);
            if (category != null)
                state.Cathegory = category;
            else
                Logger.AddDbgLog($"Категория '{categoryStr}' не найдена для штата {id} (файл: {fileFullPath})");

            // Manpower и Local Supply
            state.Manpower = stateBracket.SubVars.FirstOrDefault(v => v.Name == "manpower")?.Value.ToInt() ?? DataDefaultValues.NullInt;
            state.LocalSupply = stateBracket.SubVars.FirstOrDefault(v => v.Name == "local_supply")?.Value.ToDouble() ?? DataDefaultValues.NullInt;

            return state;
        }
    }
}