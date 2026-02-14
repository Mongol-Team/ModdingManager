using Application.Debugging;
using Application.Extensions;
using Application.extentions;
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
    public class IdeaTagComposer 
    {
        /// <summary>
        /// Парсит все файлы категорий и тегов идей и возвращает список файлов (ConfigFile<IdeaTagConfig>)
        /// </summary>
        public static List<ConfigFile<IdeaTagConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var tagFiles = new List<ConfigFile<IdeaTagConfig>>();

            string[] pathes =
            {
                ModPathes.IdeaTagsPath,
                GamePathes.IdeaTagsPath
            };

            foreach (string path in pathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.IdeaTagsPath);

                        var configFile = ParseFile(hoiFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            tagFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл тегов идей: {configFile.FileName} → {configFile.Entities.Count} тегов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[IdeaTagComposer] Ошибка при парсинге файла {file}: {ex.Message}");
                        Logger.AddDbgLog($"Стек: {ex.StackTrace}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг тегов и категорий идей завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {tagFiles.Count}, тегов всего: {tagFiles.Sum(f => f.Entities.Count)}");

            return tagFiles;
        }

        private static ConfigFile<IdeaTagConfig> ParseFile(HoiFuncFile file, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<IdeaTagConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket topBr in file.Brackets.Where(b => b.Name == "idea_categories"))
            {
                foreach (Bracket tagBr in topBr.SubBrackets)
                {
                    var cfg = ParseIdeaTag(tagBr, fileFullPath, isOverride);
                    if (cfg != null)
                    {
                        configFile.Entities.Add(cfg);
                        Logger.AddDbgLog($"  → добавлен тег идеи: {cfg.Id}");
                    }
                }
            }

            return configFile;
        }

        private static IdeaTagConfig ParseIdeaTag(Bracket br, string fileFullPath, bool isOverride)
        {
            var cfg = new IdeaTagConfig
            {
                Id = new Identifier(br.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                Slots = new List<IdeaGroupConfig>(),
                CharacterSlots = new List<string>()
            };

            foreach (Var v in br.SubVars)
            {
                switch (v.Name)
                {
                    case "type":
                        string typeStr = v.Value?.ToString()?.ToLower() ?? string.Empty;
                        cfg.Type = typeStr switch
                        {
                            "army_spirit" => IdeaType.ArmySpirit,
                            "navy_spirit" => IdeaType.NavySpirit,
                            "air_spirit" => IdeaType.AirSpirit,
                            "national_spirit" => IdeaType.NationalSpirit,
                            _ => IdeaType.NationalSpirit
                        };
                        break;

                    case "slot":
                        var slot = ModDataStorage.Mod.IdeaSlots.SearchConfigInFile(v.Value?.ToString());
                        if (slot != null)
                            cfg.Slots.Add(slot);
                        else
                            Logger.AddDbgLog($"Слот {v.Value} не найден для тега {br.Name} (файл: {fileFullPath})");
                        break;

                    case "character_slot":
                        if (!string.IsNullOrEmpty(v.Value?.ToString()))
                            cfg.CharacterSlots.Add(v.Value.ToString());
                        break;

                    case "cost":
                        cfg.Cost = v.Value.ToInt();
                        break;

                    case "removal_cost":
                        cfg.RemovalCost = v.Value.ToInt();
                        break;

                    case "ledger":
                        string ledgerStr = v.Value?.ToString()?.ToLower() ?? string.Empty;
                        cfg.Ledger = ledgerStr switch
                        {
                            "civilian" => IdeaLedgerType.Civilian,
                            "army" => IdeaLedgerType.Army,
                            "air" => IdeaLedgerType.Air,
                            "navy" => IdeaLedgerType.Navy,
                            "military" => IdeaLedgerType.Military,
                            "all" => IdeaLedgerType.All,
                            "hidden" => IdeaLedgerType.Hidden,
                            _ => IdeaLedgerType.Invalid
                        };
                        break;

                    case "hidden":
                        cfg.Hidden = v.Value.ToBool();
                        break;

                    case "politics_tab":
                        cfg.PoliticsTab = v.Value.ToBool();
                        break;
                }
            }

            return cfg;
        }

    }
}