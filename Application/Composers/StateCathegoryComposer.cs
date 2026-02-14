using Application.Debugging;
using Application.Extensions;
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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class StateCathegoryComposer 
    {
        /// <summary>
        /// Парсит все файлы категорий штатов и возвращает список файлов (ConfigFile<StateCathegoryConfig>)
        /// </summary>
        public static List<ConfigFile<StateCathegoryConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var categoryFiles = new List<ConfigFile<StateCathegoryConfig>>();

            string[] possiblePaths =
            {
                ModPathes.StateCathegoryPath,
                GamePathes.StateCathegoryPath
            };

            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path))
                {
                    Logger.AddLog($"Директория категорий штатов не найдена: {path}", LogLevel.Info);
                    continue;
                }

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.StateCathegoryPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            categoryFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл категорий штатов: {configFile.FileName} → {configFile.Entities.Count} категорий");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[StateCathegoryComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг категорий штатов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {categoryFiles.Count}, категорий всего: {categoryFiles.Sum(f => f.Entities.Count)}");

            return categoryFiles;
        }

        private static ConfigFile<StateCathegoryConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<StateCathegoryConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            var categoriesBracket = hoiFuncFile.Brackets.FirstOrDefault(b => b.Name == "state_categories");
            if (categoriesBracket == null)
            {
                Logger.AddDbgLog($"Блок 'state_categories' не найден в файле: {fileFullPath}");
                return configFile;
            }

            foreach (Bracket catBr in categoriesBracket.SubBrackets)
            {
                var cfg = ParseCategory(catBr, fileFullPath, isOverride);
                if (cfg != null)
                {
                    configFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлена категория штата: {cfg.Id}");
                }
            }

            return configFile;
        }

        private static StateCathegoryConfig ParseCategory(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var cfg = new StateCathegoryConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>()
            };

            // Цвет категории
            var colorArray = bracket.Arrays.FirstOrDefault(a => a.Name == "color");
            if (colorArray != null)
            {
                cfg.Color = colorArray.AsColor();
            }

            // Модификаторы — ищем через новый экстеншен SearchConfigInFile
            foreach (Var modVar in bracket.SubVars)
            {
                var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(modVar.Name);

                if (modDef != null)
                {
                    cfg.Modifiers.Add(modDef, modVar.Value);
                }
                else
                {
                    Logger.AddDbgLog($"Модификатор {modVar.Name} не найден для категории {bracket.Name} (файл: {fileFullPath})");
                }
            }

            return cfg;
        }
    }
}