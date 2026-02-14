using Application.Debugging;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles;
using Models.Enums;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.TableCacheData;
using Models.Types.Utils;
using RawDataWorker.Healers;
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
    public class ProvinceComposer
    {
        public static CsvHealer OnParsingHealer = new CsvHealer(new CsvDefinitionsPattern());

        /// <summary>
        /// Парсит definition.csv из мода или игры и возвращает список файлов (ConfigFile<ProvinceConfig>)
        /// </summary>
        public static List<ConfigFile<ProvinceConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var provinceFiles = new List<ConfigFile<ProvinceConfig>>();

            string[] possiblePaths =
            {
                ModPathes.DefinitionPath,  // Сначала мод
                GamePathes.DefinitionPath  // Затем игра
            };

            foreach (string path in possiblePaths)
            {
                if (!File.Exists(path))
                {
                    Logger.AddDbgLog($"Файл definition.csv не найден по пути: {path}");
                    continue;
                }

                try
                {
                    string content = File.ReadAllText(path);
                    CsvParser csvParser = new CsvParser(new CsvDefinitionsPattern(), OnParsingHealer);
                    HoiTable defTable = csvParser.Parse(content) as HoiTable;

                    bool isOverride = path.StartsWith(ModPathes.DefinitionPath);

                    var configFile = ParseCsvFile(defTable, path, isOverride);

                    if (configFile.Entities.Any())
                    {
                        provinceFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл провинций: {configFile.FileName} → {configFile.Entities.Count} провинций");
                        break;  // Если нашли в моде - не ищем в игре
                    }
                }
                catch (Exception ex)
                {
                    Logger.AddLog($"[ProvinceComposer] Ошибка парсинга файла {path}: {ex.Message}");
                }
            }

            ModDataStorage.CsvErrors = OnParsingHealer.Errors;

            stopwatch.Stop();
            Logger.AddLog($"Парсинг провинций завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {provinceFiles.Count}, провинций всего: {provinceFiles.Sum(f => f.Entities.Count)}");

            return provinceFiles;
        }

        private static ConfigFile<ProvinceConfig> ParseCsvFile(HoiTable defTable, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<ProvinceConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            List<ProvinceConfig> seaProvinces = new();
            List<ProvinceConfig> otherProvinces = new();

            if (defTable == null || defTable.Values == null)
            {
                Logger.AddDbgLog($"Таблица определений провинций пуста или null в файле: {fileFullPath}");
                return configFile;
            }

            foreach (var line in defTable.Values)
            {
                if (line == null || line.Count < 6) continue;  // Проверка на null и длину

                try
                {
                    if (line[0] == null || !int.TryParse(line[0].ToString(), out int id)) continue;

                    var province = new ProvinceConfig
                    {
                        Id = new Identifier(id),
                        Color = line[1] != null ? (Color)line[1] : Color.Transparent,  // Проверка на null
                        Type = line[2] != null ? (ProvinceType)line[2] : ProvinceType.Land,
                        IsCoastal = line[3] != null ? (bool)line[3] : false,
                        Terrain = line[4]?.ToString() ?? "plains",  // Проверка на null
                        ContinentId = line[5] != null ? (int)line[5] : 0
                    };

                    if (province.Type == ProvinceType.Sea)
                        seaProvinces.Add(province);
                    else
                        otherProvinces.Add(province);
                }
                catch (Exception ex)
                {
                    Logger.AddDbgLog($"Ошибка парсинга строки провинции: {ex.Message}");
                    continue;
                }
            }

            // Объединяем sea + other
            var allProvinces = seaProvinces.Concat(otherProvinces).ToList();

            configFile.Entities.AddRange(allProvinces);

            return configFile;
        }
    }
}