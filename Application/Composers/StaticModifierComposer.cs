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
    public class StaticModifierComposer
    {
        /// <summary>
        /// Парсит все файлы статических модификаторов и возвращает список файлов (ConfigFile<StaticModifierConfig>)
        /// </summary>
        public static List<ConfigFile<StaticModifierConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var modifierFiles = new List<ConfigFile<StaticModifierConfig>>();

            string[] possiblePaths =
            {
                ModPathes.StaticModifiersPath,
                GamePathes.StaticModifiersPath
            };

            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.StaticModifiersPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            modifierFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл статических модификаторов: {configFile.FileName} → {configFile.Entities.Count} модификаторов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[StaticModifierComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг статических модификаторов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {modifierFiles.Count}, модификаторов всего: {modifierFiles.Sum(f => f.Entities.Count)}");

            return modifierFiles;
        }

        private static ConfigFile<StaticModifierConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<StaticModifierConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket bracket in hoiFuncFile.Brackets)
            {
                var cfg = ParseSingleModifier(bracket, fileFullPath, isOverride);
                if (cfg != null)
                {
                    configFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлен статический модификатор: {cfg.Id}");
                }
            }

            return configFile;
        }

        private static StaticModifierConfig ParseSingleModifier(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var cfg = new StaticModifierConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>()
            };

            foreach (Var v in bracket.SubVars)
            {
                // Используем новый экстеншен SearchConfigInFile для поиска модификатора
                var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);

                if (modDef != null)
                {
                    object value = v.Value switch
                    {
                        int i => i,
                        double d => d,
                        bool b => b,
                        string s => s,
                        _ => v.Value?.ToString() ?? string.Empty
                    };

                    cfg.Modifiers[modDef] = value;
                }
                else
                {
                    Logger.AddDbgLog($"Модификатор {v.Name} не найден для статического модификатора {bracket.Name} (файл: {fileFullPath})");
                }
            }

            return cfg;
        }
    }
}