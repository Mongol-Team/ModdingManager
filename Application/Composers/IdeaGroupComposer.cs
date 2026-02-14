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
using Models.GfxTypes;
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
    public class IdeaGroupComposer
    {
        /// <summary>
        /// Парсит все файлы групп и слотов идей и возвращает список файлов (ConfigFile<IdeaGroupConfig>)
        /// </summary>
        public static List<ConfigFile<IdeaGroupConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var groupFiles = new List<ConfigFile<IdeaGroupConfig>>();

            string[] pathes =
            {
                GamePathes.IdeasPath,
                ModPathes.IdeasPath
            };

            foreach (string path in pathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    HoiFuncFile hoiFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    bool isOverride = path.StartsWith(ModPathes.IdeasPath);

                    var configFile = ParseSingleFile(hoiFile, file, isOverride, groupFiles);

                    if (configFile.Entities.Any())
                    {
                        groupFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл групп идей: {configFile.FileName} → {configFile.Entities.Count} слотов");
                    }
                }
            }

            PaseDynamicModifierDefenitions(groupFiles);

            stopwatch.Stop();
            Logger.AddLog($"Парсинг групп и слотов идей завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {groupFiles.Count}, слотов всего: {groupFiles.Sum(f => f.Entities.Count)}");

            return groupFiles;
        }

        private static ConfigFile<IdeaGroupConfig> ParseSingleFile(HoiFuncFile file, string fileFullPath, bool isOverride,
            List<ConfigFile<IdeaGroupConfig>> allGroupFiles)
        {
            var configFile = new ConfigFile<IdeaGroupConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket topBr in file.Brackets.Where(b => b.Name == "ideas"))
            {
                foreach (Bracket slotBr in topBr.SubBrackets)
                {
                    var slotConfig = ParseIdeaGroup(slotBr, fileFullPath, isOverride, allGroupFiles);

                    if (slotConfig != null)
                    {
                        configFile.Entities.Add(slotConfig);
                    }
                }
            }

            return configFile;
        }

        private static IdeaGroupConfig ParseIdeaGroup(Bracket slotBr, string fileFullPath, bool isOverride,
            List<ConfigFile<IdeaGroupConfig>> allGroupFiles)
        {
            string slotName = slotBr.Name;
            if (string.IsNullOrEmpty(slotName))
                return null;

            // Ищем слот с таким именем в уже собранных файлах
            var existing = allGroupFiles
                .SelectMany(f => f.Entities)
                .FirstOrDefault(s => s.Id.ToString() == slotName);

            if (existing != null)
            {
                Logger.AddDbgLog($"Слот {slotName} уже существует, дополняем его данными из {fileFullPath}");

                foreach (Bracket ideaBr in slotBr.SubBrackets)
                {
                    string ideaId = ideaBr.Name;
                    var idea = ModDataStorage.Mod.Ideas.SearchConfigInFile(ideaId);

                    if (idea != null && !existing.Ideas.Any(i => i.Id == idea.Id))
                    {
                        existing.Ideas.Add(idea);
                        Logger.AddDbgLog($"  → добавлена идея {ideaId} в существующий слот {slotName}");
                    }
                }

                return existing;
            }

            // Новый слот
            var slotConfig = new IdeaGroupConfig
            {
                Id = new Identifier(slotName),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                IsLaw = slotBr.SubVars.FirstOrDefault(v => v.Name == "law")?.Value.ToBool() ?? false,
                UseListView = slotBr.SubVars.FirstOrDefault(v => v.Name == "use_list_view")?.Value.ToBool() ?? false,
                IsDesigner = slotBr.SubVars.FirstOrDefault(v => v.Name == "designer")?.Value.ToBool() ?? false,

                Ideas = new List<IdeaConfig>(),
                Localisation = new ConfigLocalisation { Language = ModManagerSettings.CurrentLanguage }
            };

            foreach (Bracket ideaBr in slotBr.SubBrackets)
            {
                string ideaId = ideaBr.Name;
                var idea = ModDataStorage.Mod.Ideas.SearchConfigInFile(ideaId);

                if (idea != null)
                {
                    slotConfig.Ideas.Add(idea);
                }
                else
                {
                    Logger.AddDbgLog($"Идея {ideaId} не найдена при парсинге слота {slotName} (файл: {fileFullPath})");
                }
            }

            return slotConfig;
        }


        public static void PaseDynamicModifierDefenitions(List<ConfigFile<IdeaGroupConfig>> groupFiles)
        {
            // Ищем или создаём виртуальный файл core_objects для динамических модификаторов
            var coreFile = ModDataStorage.Mod.ModifierDefinitions?
                .FirstOrDefault(f => f.FileName == "core_objects");

            if (coreFile == null)
            {
                coreFile = new ConfigFile<ModifierDefinitionConfig>
                {
                    FileFullPath = "core_objects",
                    IsCore = true,
                    IsOverride = false,
                };
                ModDataStorage.Mod.ModifierDefinitions.Add(coreFile);
                Logger.AddDbgLog("Создан виртуальный core_objects для динамических модификаторов идей");
            }

            int createdCount = 0;

            foreach (var groupFile in groupFiles)
            {
                foreach (IdeaGroupConfig ig in groupFile.Entities)
                {
                    if (ig.Id == null) continue;

                    var def = new ModifierDefinitionConfig
                    {
                        Id = new Identifier($"{ig.Id}_cost_factor"),
                        ScopeType = ScopeTypes.Country,
                        ValueType = ModifierDefenitionValueType.Percent,
                        IsCore = true,
                        ColorType = ModifierDefenitionColorType.Bad,
                        Precision = 2,
                        FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                        Cathegory = ModifierDefinitionCathegoryType.Country,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                        Localisation = new ConfigLocalisation
                        {
                            Language = ModManagerSettings.CurrentLanguage
                        }
                    };

                    def.Localisation.Data.AddPair(
                        ModDataStorage.Localisation.GetLocalisationByKey(def.Id.ToString())
                    );

                    coreFile.Entities.Add(def);
                    createdCount++;

                    Logger.AddDbgLog($"Создан динамический модификатор: {def.Id}");
                }
            }

            Logger.AddLog($"Создано {createdCount} динамических модификаторов для групп идей");
        }

       
    }
}