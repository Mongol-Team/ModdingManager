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
using Models.Types.ObjectCacheData;
using Models.Types.TableCacheData;
using Models.Types.Utils;
using Pfim;
using RawDataWorker.Healers;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DDF = Data.DataDefaultValues;

namespace Application.Composers
{
    public class RuleComposer 
    {
        public static CsvHealer OnParsingHealer = new CsvHealer(new RulesDataPattern());

        /// <summary>
        /// Парсит все файлы правил и возвращает список файлов (ConfigFile<RuleConfig>)
        /// </summary>
        public static List<ConfigFile<RuleConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var ruleFiles = new List<ConfigFile<RuleConfig>>();

            // 1. Core-правила из CSV (всегда добавляем как core_objects)
            var coreFile = ParseCoreRules();
            if (coreFile.Entities.Any())
            {
                ruleFiles.Add(coreFile);
                Logger.AddDbgLog($"Добавлен виртуальный core_objects с {coreFile.Entities.Count} core-правилами");
            }

            // 2. Обычные txt-файлы из модов и игры
            string[] possiblePaths =
            {
                ModPathes.RulesPath,
                GamePathes.RulesPath
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

                        bool isOverride = path.StartsWith(ModPathes.RulesPath);

                        var configFile = ParseTxtFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            ruleFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл правил: {configFile.FileName} → {configFile.Entities.Count} правил");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[RuleComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг правил завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {ruleFiles.Count}, правил всего: {ruleFiles.Sum(f => f.Entities.Count)}");

            return ruleFiles;
        }

        /// <summary>
        /// Парсит core-правила из CSV и возвращает виртуальный файл core_objects
        /// </summary>
        private static ConfigFile<RuleConfig> ParseCoreRules()
        {
            var coreFile = new ConfigFile<RuleConfig>
            {
                FileFullPath = "core_objects",
                IsCore = true,
                IsOverride = false,
                Entities = new List<RuleConfig>()
            };

            try
            {
                HoiTable tbl = new CsvParser(new RulesDataPattern(), OnParsingHealer)
                    .Parse(DataLib.RulesCoreDefenitions) as HoiTable;

                foreach (List<object> row in tbl.Values)
                {
                    var cfg = new RuleConfig
                    {
                        Id = new Identifier(row[0].ToString()),
                        IsCore = true,
                        FileFullPath = "core_objects"
                    };

                    coreFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлено core-правило: {cfg.Id}");
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Ошибка парсинга core-правил из CSV: {ex.Message}");
            }

            return coreFile;
        }

        private static ConfigFile<RuleConfig> ParseTxtFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<RuleConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket br in hoiFuncFile.Brackets)
            {
                var cfg = ParseSingleRule(br, fileFullPath, isOverride);
                if (cfg != null)
                {
                    configFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлено правило: {cfg.Id}");
                }
            }

            return configFile;
        }

        private static RuleConfig ParseSingleRule(Bracket ruleBr, string fileFullPath, bool isOverride)
        {
            if (ruleBr == null) return null;

            var res = new RuleConfig
            {
                Id = new Identifier(ruleBr.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,
                IsCore = false,

                GroupId = ruleBr.SubVars.FirstOrDefault(v => v.Name == "group")?.Value?.ToString() ?? DDF.Null,
                RequiredDlc = ruleBr.SubVars.FirstOrDefault(v => v.Name == "required_dlc")?.Value?.ToString() ?? DDF.Null,
                ExcludedDlc = ruleBr.SubVars.FirstOrDefault(v => v.Name == "excluded_dlc")?.Value?.ToString() ?? DDF.Null,

                Options = new List<BaseConfig>(),
                Localisation = new ConfigLocalisation { Language = ModManagerSettings.CurrentLanguage }
            };

            // Иконка правила
            var iconVar = ruleBr.SubVars.FirstOrDefault(v => v.Name == "icon");
            if (iconVar?.Value != null)
            {
                string iconId = iconVar.Value.ToString();
                res.Gfx = ModDataStorage.Mod.Gfxes.SearchConfigInFile(iconId) ?? new SpriteType();
            }
            else
            {
                res.Gfx = new SpriteType();
            }

            // Локализация имени правила
            string locKey = ruleBr.SubVars.FirstOrDefault(v => v.Name == "name")?.Value?.ToString() ?? DDF.Null;
            if (locKey != DDF.Null)
            {
                var loc = ModDataStorage.Localisation.GetLocalisationByKey(locKey);
                res.Localisation.Data.AddPair(loc);
            }

            // Опции и default
            foreach (Bracket subBr in ruleBr.SubBrackets)
            {
                if (subBr.Name == "option")
                {
                    var option = ParseOption(subBr);
                    if (option != null)
                        res.Options.Add(option);
                }
                else if (subBr.Name == "default")
                {
                    var defaultOpt = ParseOption(subBr);
                    if (defaultOpt != null)
                        res.Default = defaultOpt;
                }
            }

            return res;
        }

        private static BaseConfig ParseOption(Bracket br)
        {
            var option = new BaseConfig
            {
                Id = new Identifier(br.SubVars.FirstOrDefault(v => v.Name == "name")?.Value?.ToString() ?? DDF.Null),
                Localisation = new ConfigLocalisation { Language = ModManagerSettings.CurrentLanguage }
            };

            var textKey = br.SubVars.FirstOrDefault(v => v.Name == "text")?.Value?.ToString();
            var descKey = br.SubVars.FirstOrDefault(v => v.Name == "desc")?.Value?.ToString();

            if (textKey != null)
            {
                var nameLoc = ModDataStorage.Localisation.GetLocalisationByKey(textKey);
                option.Localisation.Data.AddPair(nameLoc);
            }

            if (descKey != null)
            {
                var descLoc = ModDataStorage.Localisation.GetLocalisationByKey(descKey);
                option.Localisation.Data.AddPair(descLoc);
            }

            return option;
        }
    }
}