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
using Models.Types.ObectCacheData;
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
    public class SubUnitComposer 
    {
        /// <summary>
        /// Парсит все файлы подтипов подразделений и возвращает список файлов (ConfigFile<SubUnitConfig>)
        /// </summary>
        public static List<ConfigFile<SubUnitConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var subunitFiles = new List<ConfigFile<SubUnitConfig>>();

            string[] possiblePathes =
            {
                ModPathes.RegimentsPath,
                GamePathes.RegimentsPath
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

                        bool isOverride = path.StartsWith(ModPathes.RegimentsPath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            subunitFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл подтипов подразделений: {configFile.FileName} → {configFile.Entities.Count} подтипов");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[SubUnitComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            // Динамические модификаторы опыта
            PaseDynamicModifierDefenitions(subunitFiles);

            stopwatch.Stop();
            Logger.AddLog($"Парсинг подтипов подразделений завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {subunitFiles.Count}, подтипов всего: {subunitFiles.Sum(f => f.Entities.Count)}");

            return subunitFiles;
        }

        private static ConfigFile<SubUnitConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<SubUnitConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            var subunitsBracket = hoiFuncFile.Brackets.FirstOrDefault(b => b.Name == "sub_units");
            if (subunitsBracket == null) return configFile;

            foreach (Bracket unitBr in subunitsBracket.SubBrackets)
            {
                var config = ParseSubUnit(unitBr, fileFullPath, isOverride);
                if (config != null)
                {
                    configFile.Entities.Add(config);
                    Logger.AddDbgLog($"  → добавлен подтип подразделения: {config.Id}");
                }
            }

            return configFile;
        }

        private static SubUnitConfig ParseSubUnit(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var config = new SubUnitConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                TerrainModifiers = new Dictionary<ProvinceTerrain, Dictionary<ModifierDefinitionConfig, object>>(),
                Need = new Dictionary<EquipmentConfig, int>(),
                Types = new List<IternalUnitType>(),
                Chategories = new List<SubUnitCategoryConfig>()
            };

            foreach (Var v in bracket.SubVars)
            {
                switch (v.Name)
                {
                    case "sprite":
                        config.EntitySprite = v.Value?.ToString(); // todo: entity sprite class
                        break;

                    case "active":
                        config.Active = v.Value.ToBool();
                        break;

                    case "priority":
                        config.Priority = v.Value.ToInt();
                        break;

                    case "map_icon_category":
                        config.MapIconCategory = v.Value.ToString().SnakeToPascal().ToEnum<UnitMapIconType>(default);
                        break;

                    case "affects_speed":
                        config.AffectsSpeed = v.Value.ToBool();
                        break;

                    case "use_transport_speed":
                        var transportEq = ModDataStorage.Mod.Equipments.SearchConfigInFile(v.Value?.ToString());
                        if (transportEq != null)
                            config.UseTransportSpeed = transportEq;
                        break;

                    case "group":
                        var group = ModDataStorage.Mod.SubUnitGroups.SearchConfigInFile(v.Value?.ToString());
                        if (group != null)
                        {
                            config.Group = group;
                        }
                        else
                        {
                            Logger.AddDbgLog($"Группа {v.Value} не найдена для подтипа {config.Id} (файл: {fileFullPath})");
                        }
                        break;

                    case "ai_priority":
                        config.AiPriority = v.Value.ToInt();
                        break;

                    case "can_exfiltrate_from_coast":
                        config.CanExfiltrateFromCoast = v.Value.ToBool();
                        break;

                    default:
                        var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                        if (modDef != null)
                        {
                            config.Modifiers.Add(modDef, v.Value);
                        }
                        break;
                }
            }

            // Подблоки
            foreach (Bracket subb in bracket.SubBrackets)
            {
                switch (subb.Name)
                {
                    case "need":
                        foreach (Var needVar in subb.SubVars)
                        {
                            var equipment = ModDataStorage.Mod.Equipments.SearchConfigInFile(needVar.Name);
                            if (equipment != null)
                            {
                                config.Need.Add(equipment, needVar.Value.ToInt());
                            }
                        }
                        break;

                    default:
                        if (Enum.TryParse<ProvinceTerrain>(subb.Name.SnakeToPascal(), out var terrain))
                        {
                            var terrMods = new Dictionary<ModifierDefinitionConfig, object>();

                            foreach (Var terrVar in subb.SubVars)
                            {
                                var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(terrVar.Name);
                                if (modDef != null)
                                    terrMods.Add(modDef, terrVar.Value);
                            }

                            if (terrMods.Any())
                                config.TerrainModifiers[terrain] = terrMods;
                        }
                        break;
                }
            }

            // Массивы
            foreach (HoiArray arr in bracket.Arrays)
            {
                switch (arr.Name)
                {
                    case "types":
                        config.Types = arr.Values
                            .Select(v => v.ToString().SnakeToPascal().ToEnum<IternalUnitType>(default))
                            .Where(t => t != default)
                            .ToList();
                        break;

                    case "chategories":
                        config.Chategories = arr.Values
                            .Select(v => ModDataStorage.Mod.SubUnitChategories.SearchConfigInFile(v.ToString()))
                            .Where(c => c != null)
                            .ToList();
                        break;
                }
            }

            return config;
        }

        public static void PaseDynamicModifierDefenitions(List<ConfigFile<SubUnitConfig>> subunitFiles)
        {
            // Ищем или создаём виртуальный файл core_objects
            var coreFile = ModDataStorage.Mod.ModifierDefinitions?
                .FirstOrDefault(f => f.FileName == "core_objects");

            if (coreFile == null)
            {
                coreFile = new ConfigFile<ModifierDefinitionConfig>
                {
                    FileFullPath = "core_objects",
                    IsCore = true,
                    IsOverride = false,
                    Entities = new List<ModifierDefinitionConfig>()
                };
                Logger.AddDbgLog("Создан виртуальный core_objects для динамических модификаторов подтипов подразделений");
            }

            int created = 0;

            foreach (var file in subunitFiles)
            {
                foreach (SubUnitConfig config in file.Entities)
                {
                    if (config.Id == null) continue;

                    // experience_gain_{id}_training_factor
                    var trainingMod = new ModifierDefinitionConfig
                    {
                        Id = new Identifier($"experience_gain_{config.Id}_training_factor"),
                        Cathegory = ModifierDefinitionCathegoryType.Army,
                        ColorType = ModifierDefenitionColorType.Good,
                        ScopeType = ScopeTypes.Country,
                        ValueType = ModifierDefenitionValueType.Percent,
                        Precision = 2,
                        IsCore = true,
                        FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                        Localisation = new ConfigLocalisation
                        {
                            Language = ModManagerSettings.CurrentLanguage
                        }
                    };
                    trainingMod.Localisation.Data.AddPair(
                        ModDataStorage.Localisation.GetLocalisationByKey(trainingMod.Id.ToString())
                    );

                    coreFile.Entities.Add(trainingMod);
                    created++;

                    // experience_gain_{id}_combat_factor
                    var combatMod = new ModifierDefinitionConfig
                    {
                        Id = new Identifier($"experience_gain_{config.Id}_combat_factor"),
                        Cathegory = ModifierDefinitionCathegoryType.Army,
                        ColorType = ModifierDefenitionColorType.Good,
                        ScopeType = ScopeTypes.Country,
                        ValueType = ModifierDefenitionValueType.Percent,
                        Precision = 2,
                        IsCore = true,
                        FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                        Localisation = new ConfigLocalisation
                        {
                            Language = ModManagerSettings.CurrentLanguage
                        }
                    };
                    combatMod.Localisation.Data.AddPair(
                        ModDataStorage.Localisation.GetLocalisationByKey(combatMod.Id.ToString())
                    );

                    coreFile.Entities.Add(combatMod);
                    created++;

                    var designcostMod = new ModifierDefinitionConfig
                    {
                        Id = new Identifier($"unit_{config.Id}_design_cost_factor"),
                        Cathegory = ModifierDefinitionCathegoryType.Army,
                        ColorType = ModifierDefenitionColorType.Bad,
                        ScopeType = ScopeTypes.Country,
                        ValueType = ModifierDefenitionValueType.Percent,
                        Precision = 2,
                        IsCore = true,
                        FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                        Localisation = new ConfigLocalisation
                        {
                            Language = ModManagerSettings.CurrentLanguage
                        }
                    };
                    designcostMod.Localisation.Data.AddPair(
                        ModDataStorage.Localisation.GetLocalisationByKey(designcostMod.Id.ToString())
                    );

                    coreFile.Entities.Add(designcostMod);
                    created++;
                }
            }

            Logger.AddLog($"Создано {created} динамических модификаторов опыта для подтипов подразделений");
        }
    }
}