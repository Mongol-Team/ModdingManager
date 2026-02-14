using Application.Debugging;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles;
using Models.Types.LocalizationData;
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
    public class SRegionComposer
    {
        /// <summary>
        /// Парсит все файлы стратегических регионов и возвращает список файлов (ConfigFile<StrategicRegionConfig>)
        /// </summary>
        public static List<ConfigFile<StrategicRegionConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var regionFiles = new List<ConfigFile<StrategicRegionConfig>>();

            string[] priorityFolders =
            {
                ModPathes.StrategicRegionPath,
                GamePathes.StrategicRegionPath
            };

            foreach (string folder in priorityFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Logger.AddLog($"Директория стратегических регионов не найдена: {folder}", LogLevel.Info);
                    continue;
                }

                string[] files = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = folder.StartsWith(ModPathes.StrategicRegionPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            regionFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл стратегических регионов: {configFile.FileName} → {configFile.Entities.Count} регионов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[SRegionComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг стратегических регионов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {regionFiles.Count}, регионов всего: {regionFiles.Sum(f => f.Entities.Count)}");

            return regionFiles;
        }

        private static ConfigFile<StrategicRegionConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<StrategicRegionConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket regionBracket in hoiFuncFile.Brackets)
            {
                var region = ParseStrategicRegion(regionBracket, fileFullPath, isOverride);
                if (region != null)
                {
                    configFile.Entities.Add(region);
                    Logger.AddDbgLog($"  → добавлен стратегический регион: {region.Id}");
                }
            }

            return configFile;
        }

        private static StrategicRegionConfig ParseStrategicRegion(Bracket regionBracket, string fileFullPath, bool isOverride)
        {
            var idVar = regionBracket.SubVars.FirstOrDefault(v => v.Name == "id");
            if (idVar == null || !int.TryParse(idVar.Value?.ToString(), out int id))
            {
                Logger.AddDbgLog($"Регион без валидного id в файле {fileFullPath}");
                return null;
            }

            // Проверяем, не существует ли уже такой регион (по id)
            var existing = ModDataStorage.Mod.Map.StrategicRegions.SearchConfigInFile(id.ToString());
            if (existing != null)
            {
                Logger.AddDbgLog($"Стратегический регион {id} уже существует, пропускаем дубликат из {fileFullPath}");
                return null;
            }

            var region = new StrategicRegionConfig
            {
                Id = new Identifier(id),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                Provinces = new List<ProvinceConfig>(),
                Color = Color.FromArgb(
                    (byte)((id * 53) % 255),
                    (byte)((id * 97) % 255),
                    (byte)((id * 151) % 255)
                ),
                LocKey = regionBracket.SubVars.FirstOrDefault(v => v.Name == "name")?.Value?.ToString() ?? string.Empty
            };

            // Провинции региона
            var provincesArray = regionBracket.Arrays.FirstOrDefault(a => a.Name == "provinces");
            if (provincesArray != null && provincesArray.Values != null)
            {
                foreach (var provIdObj in provincesArray.Values)
                {
                    if (provIdObj == null) continue;

                    string provIdStr = provIdObj.ToString();
                    if (!int.TryParse(provIdStr, out int provId)) continue;

                    var province = ModDataStorage.Mod.Map.Provinces.SearchConfigInFile(provId.ToString());
                    if (province != null)
                    {
                        region.Provinces.Add(province);
                    }
                    else
                    {
                        Logger.AddDbgLog($"Провинция {provId} не найдена для региона {id} (файл: {fileFullPath})");
                    }
                }
            }

            return region;
        }
    }
}