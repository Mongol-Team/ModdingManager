using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models;
using Models.GfxTypes;
using Models.Interfaces;
using Models.Types.ObectCacheData;
using Models.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDF = Data.DataDefaultValues;
using Models.Configs;
using Application.Debugging;
using Models.EntityFiles;
using System.Diagnostics;
using Models.Types.Utils;
using Data;
namespace Application.Composers
{

    public class TechCategoryComposer 
    {
        /// <summary>
        /// Парсит все файлы категорий технологий и возвращает список файлов (ConfigFile<TechCategoryConfig>)
        /// </summary>
        public static List<ConfigFile<TechCategoryConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var categoryFiles = new List<ConfigFile<TechCategoryConfig>>();

            string[] possiblePathes =
            {
            ModPathes.TechDefPath,
            GamePathes.TechDefPath
        };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path))
                {
                    Logger.AddLog($"Директория категорий технологий не найдена: {path}", LogLevel.Info);
                    continue;
                }

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.TechDefPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            categoryFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл категорий технологий: {configFile.FileName} → {configFile.Entities.Count} категорий");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[TechCategoryComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг категорий технологий завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                            $"Файлов: {categoryFiles.Count}, категорий всего: {categoryFiles.Sum(f => f.Entities.Count)}");

            return categoryFiles;
        }

        private static ConfigFile<TechCategoryConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<TechCategoryConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            // В старом коде категории берутся из массивов, обычно это один большой массив "categories" или просто массив значений
            foreach (HoiArray array in hoiFuncFile.Arrays)
            {
                // Предполагаем, что массив содержит просто строки — имена категорий
                foreach (object value in array.Values)
                {
                    string categoryId = value?.ToString();
                    if (string.IsNullOrWhiteSpace(categoryId)) continue;

                    var existing = configFile.Entities.FirstOrDefault(c => c.Id.ToString() == categoryId);
                    if (existing != null) continue; // уже добавлена в этом файле

                    var cfg = new TechCategoryConfig
                    {
                        Id = new Identifier(categoryId),
                        FileFullPath = fileFullPath,
                        IsOverride = isOverride,

                        // Иконка по умолчанию (как было в оригинале)
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null)
                    };

                    configFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлена категория технологии: {cfg.Id}");
                }
            }

            return configFile;
        }
    }
    
}
