using Application.Debugging;
using Application.Extensions;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles;
using Models.Enums;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using Pfim;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DDF = Data.DataDefaultValues;
using System.Linq;

namespace Application.Composers
{
    public class TechnologyComposer 
    {
        /// <summary>
        /// Парсит все файлы папок технологий (technology folders) и возвращает список файлов (ConfigFile<TechTreeConfig>)
        /// </summary>
        public static List<ConfigFile<TechTreeConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var techTreeFiles = new List<ConfigFile<TechTreeConfig>>();

            string[] possiblePathes =
            {
                ModPathes.TechTreePath,
                GamePathes.TechTreePath
            };

            string[] possibleDefPathes =
            {
                ModPathes.TechDefPath,
                GamePathes.TechDefPath
            };

            // 1. Парсим определения папок технологий (обычно из common/technology_folders/*.txt)
            foreach (string defPath in possibleDefPathes)
            {
                if (!Directory.Exists(defPath)) continue;

                string[] files = Directory.GetFiles(defPath, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = defPath.StartsWith(ModPathes.TechDefPath);

                        var configFile = ParseDefFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            techTreeFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл определений папок технологий: {configFile.FileName} → {configFile.Entities.Count} папок");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[TechnologyComposer] Ошибка парсинга файла определений {file}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг папок технологий завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {techTreeFiles.Count}, папок всего: {techTreeFiles.Sum(f => f.Entities.Count)}");

            return techTreeFiles;
        }

        private static ConfigFile<TechTreeConfig> ParseDefFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<TechTreeConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            // Обычно структура: technology_folders → folder_name → свойства
            foreach (Bracket topBr in hoiFuncFile.Brackets.Where(b => b.Name == "technology_folders"))
            {
                foreach (Bracket folderBr in topBr.SubBrackets)
                {
                    var config = ParseTechnologyFolder(folderBr, fileFullPath, isOverride);
                    if (config != null)
                    {
                        configFile.Entities.Add(config);
                        Logger.AddDbgLog($"  → добавлена папка технологий: {config.Id}");
                    }
                }
            }

            return configFile;
        }

        private static TechTreeConfig ParseTechnologyFolder(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var config = new TechTreeConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                Localisation = new ConfigLocalisation { Language = ModManagerSettings.CurrentLanguage }
            };

            // ledger
            var ledgerVar = bracket.SubVars.FirstOrDefault(v => v.Name == "ledger");
            config.Ledger = ledgerVar != null
                ? ledgerVar.Value.ToString().SnakeToPascal().ToEnum<TechTreeLedgerType>(TechTreeLedgerType.Null)
                : TechTreeLedgerType.Null;

            // available (триггер)
            var availableBr = bracket.SubBrackets.FirstOrDefault(b => b.Name == "available");
            config.Available = availableBr?.ToString() ?? DDF.Null;

            // Иконка папки (обычно GFX_{id}_tab)
            string gfxKey = $"GFX_{config.Id}_tab";
            config.Gfx = ModDataStorage.Mod.Gfxes.SearchConfigInFile(gfxKey)
                         ?? new SpriteType(DDF.NullImageSource, DDF.Null);

            // Локализация
            var nameLoc = ModDataStorage.Localisation.GetLocalisationByKey(config.Id.ToString());
            config.Localisation.Data.AddPair(nameLoc);

            var descLoc = ModDataStorage.Localisation.GetLocalisationByKey(config.Id.ToString() + "_desc");
            config.Localisation.Data.AddPair(descLoc);

            // Если локализация пустая — помечаем
            if (!config.Localisation.Data.Any())
            {
                config.Localisation.IsConfigLocNull = true;
            }

            return config;
        }
    }
}