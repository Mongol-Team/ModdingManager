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
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class OpinionModifierComposer 
    {
        /// <summary>
        /// Парсит все файлы модификаторов мнения и возвращает список файлов (ConfigFile<OpinionModifierConfig>)
        /// </summary>
        public static List<ConfigFile<OpinionModifierConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var opinionFiles = new List<ConfigFile<OpinionModifierConfig>>();

            string[] possiblePathes =
            {
                ModPathes.OpinionModifiersPath,
                GamePathes.OpinionModifiersPath
            };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.OpinionModifiersPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            opinionFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл модификаторов мнения: {configFile.FileName} → {configFile.Entities.Count} модификаторов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[OpinionModifierComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг модификаторов мнения завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {opinionFiles.Count}, модификаторов всего: {opinionFiles.Sum(f => f.Entities.Count)}");

            return opinionFiles;
        }

        private static ConfigFile<OpinionModifierConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<OpinionModifierConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket topBr in hoiFuncFile.Brackets.Where(b => b.Name == "opinion_modifiers"))
            {
                foreach (Bracket opinBr in topBr.SubBrackets)
                {
                    var config = ParseOpinionModifier(opinBr, fileFullPath, isOverride);
                    if (config != null)
                    {
                        configFile.Entities.Add(config);
                        Logger.AddDbgLog($"  → добавлен модификатор мнения: {config.Id}");
                    }
                }
            }

            return configFile;
        }

        private static OpinionModifierConfig ParseOpinionModifier(Bracket modbr, string fileFullPath, bool isOverride)
        {
            var config = new OpinionModifierConfig
            {
                Id = new Identifier(modbr.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Var item in modbr.SubVars)
            {
                switch (item.Name)
                {
                    case "name":
                        config.Name = item.Value?.ToString();
                        break;

                    case "description":
                        config.Description = item.Value?.ToString();
                        break;

                    case "is_trade":
                        config.IsTrade = item.Value.ToBool();
                        break;

                    case "value":
                        config.Value = item.Value.ToInt();
                        break;

                    case "decay":
                        config.Decay = item.Value.ToInt();
                        break;

                    case "days":
                        config.RemovalTime.SumToKey(TimeUnit.Day, item.Value.ToInt());
                        break;

                    case "months":
                        config.RemovalTime.SumToKey(TimeUnit.Day, item.Value.ToInt() * 30);
                        break;

                    case "years":
                        config.RemovalTime.SumToKey(TimeUnit.Day, item.Value.ToInt() * 365);
                        break;

                    case "min_trust":
                        config.MinTrust = item.Value.ToInt();
                        break;

                    case "max_trust":
                        config.MaxTrust = item.Value.ToInt();
                        break;
                }
            }

            return config;
        }
    }
}