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
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class ResourceComposer
    {
        /// <summary>
        /// Парсит все файлы ресурсов и возвращает список файлов (ConfigFile<ResourceConfig>)
        /// </summary>
        public static List<ConfigFile<ResourceConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var resourceFiles = new List<ConfigFile<ResourceConfig>>();

            string[] possiblePathes =
            {
                ModPathes.ResourcesPath,
                GamePathes.ResourcesPath
            };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path))
                {
                    Logger.AddLog($"Директория ресурсов не найдена: {path}", LogLevel.Info);
                    continue;
                }

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.ResourcesPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            resourceFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл ресурсов: {configFile.FileName} → {configFile.Entities.Count} ресурсов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[ResourceComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }
            PaseDynamicModifierDefenitions(resourceFiles);
            stopwatch.Stop();
            Logger.AddLog($"Парсинг ресурсов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {resourceFiles.Count}, ресурсов всего: {resourceFiles.Sum(f => f.Entities.Count)}");

            return resourceFiles;
        }

        private static ConfigFile<ResourceConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<ResourceConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket topBr in hoiFuncFile.Brackets.Where(b => b.Name == "resources"))
            {
                foreach (Bracket resBr in topBr.SubBrackets)
                {
                    var config = ParseResource(resBr, fileFullPath, isOverride);
                    if (config != null)
                    {
                        configFile.Entities.Add(config);
                        Logger.AddDbgLog($"  → добавлен ресурс: {config.Id}");
                    }
                }
            }

            return configFile;
        }

        private static ResourceConfig ParseResource(Bracket cfgBr, string fileFullPath, bool isOverride)
        {
            var config = new ResourceConfig
            {
                Id = new Identifier(cfgBr.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            // Иконка ресурсов — общая для всех (GFX_resources_strip)
            // Используем новый экстеншен SearchConfigInFile
            config.Gfx = ModDataStorage.Mod.Gfxes.SearchConfigInFile("GFX_resources_strip");

            if (config.Gfx == null)
            {
                Logger.AddDbgLog($"Иконка GFX_resources_strip не найдена для ресурса {config.Id}");
            }

            foreach (Var v in cfgBr.SubVars)
            {
                switch (v.Name)
                {
                    case "cic":
                        config.Cost = v.Value.ToDouble();
                        break;

                    case "convoys":
                        config.Convoys = v.Value.ToDouble();
                        break;

                    case "icon_frame":
                        config.IconIndex = v.Value.ToInt();
                        break;
                }
            }

            return config;
        }

        public static void PaseDynamicModifierDefenitions(List<ConfigFile<ResourceConfig>> buildingFiles)
        {
            var coreFile = ModDataStorage.Mod.ModifierDefinitions?.FirstOrDefault(f => f.FileName == "core_objects");

            if (coreFile == null)
            {
                coreFile = new ConfigFile<ModifierDefinitionConfig>
                {
                    FileFullPath = "core_objects",
                    IsCore = true
                };
                ModDataStorage.Mod.ModifierDefinitions.Add(coreFile); // Если коллекции нет - невозможно, добавить по рекомендации
                Logger.AddDbgLog("Создан core_objects файл для динамических модификаторов.");
            }

            foreach (ResourceConfig bc in buildingFiles.SelectMany(f => f.Entities))
            {
                
                ModifierDefinitionConfig mdresstate = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"state_resource_{bc.Id}"),
                    FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                    IsCore = true,
                    Cathegory = ModifierDefinitionCathegoryType.Country,
                    Precision = 2,
                    ScopeType = ScopeTypes.Country,
                    ColorType = ModifierDefenitionColorType.Good,
                    Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                    Localisation = new ConfigLocalisation
                    {
                        Language = ModManagerSettings.CurrentLanguage,
                    }
                };
                mdresstate.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdresstate.Id.ToString()));
                ModifierDefinitionConfig mdresstatef = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"state_resource_{bc.Id}_factor"),
                    FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                    IsCore = true,
                    Cathegory = ModifierDefinitionCathegoryType.Country,
                    Precision = 2,
                    ScopeType = ScopeTypes.Country,
                    ColorType = ModifierDefenitionColorType.Good,
                    Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                    Localisation = new ConfigLocalisation
                    {
                        Language = ModManagerSettings.CurrentLanguage,
                    }
                };
                mdresstatef.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdresstatef.Id.ToString()));
                ModifierDefinitionConfig mdtempresstate = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"temporary_state_resource_{bc.Id}"),
                    FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                    IsCore = true,
                    Cathegory = ModifierDefinitionCathegoryType.Country,
                    Precision = 2,
                    ScopeType = ScopeTypes.Country,
                    ColorType = ModifierDefenitionColorType.Good,
                    Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                    Localisation = new ConfigLocalisation
                    {
                        Language = ModManagerSettings.CurrentLanguage,
                    }
                };
                mdtempresstate.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdtempresstate.Id.ToString()));
                ModifierDefinitionConfig mdrescountry = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"country_resource_{bc.Id}"),
                    FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                    IsCore = true,
                    Cathegory = ModifierDefinitionCathegoryType.Country,
                    Precision = 2,
                    ScopeType = ScopeTypes.Country,
                    ColorType = ModifierDefenitionColorType.Good,
                    Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                    Localisation = new ConfigLocalisation
                    {
                        Language = ModManagerSettings.CurrentLanguage,
                    }
                };
                mdrescountry.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdrescountry.Id.ToString()));
                if (!coreFile.Entities.Any(m => m.Id == mdresstate.Id || m.Id == mdresstatef.Id || m.Id == mdtempresstate.Id || m.Id == mdrescountry.Id))
                {
                    coreFile.Entities.Add(mdresstate);
                    coreFile.Entities.Add(mdresstatef);
                    coreFile.Entities.Add(mdtempresstate);
                    coreFile.Entities.Add(mdrescountry);
                }
                    
                Logger.AddDbgLog($"Добавлено определение модификаторов: {mdresstate.Id} в core_objects.");
            }
        }
    }
}