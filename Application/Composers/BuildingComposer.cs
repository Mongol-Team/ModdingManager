using Application.Debugging;
using Application.Extensions;
using Application.extentions;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles; // Добавлен для ConfigFile<T>
using Models.Enums;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Diagnostics;
using System.Windows.Input; // Для Stopwatch в отладке

namespace Application.Composers
{
    public class BuildingComposer
    {
        public static List<ConfigFile<BuildingConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew(); // Для отладки времени
            List<ConfigFile<BuildingConfig>> buildingFiles = new();
            string[] possiblePathes =
            {
                ModPathes.BuildingsPath,
                GamePathes.BuildingsPath
            };

            foreach (string path in possiblePathes)
            {
                if(path.HasFiles())
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            string fileContent = File.ReadAllText(file);
                            HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);
                            ConfigFile<BuildingConfig> configFile = ParseFile(hoiFuncFile, file, path == ModPathes.BuildingsPath); // Передаем путь для определения IsOverride
                            buildingFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл: {configFile.FileName} с {configFile.Entities.Count} конфигами.");
                        }
                    }
                }
                else
                {
                    Logger.AddDbgLog($"Пути {path} не существует.");
                }
            }


            PaseDynamicModifierDefenitions(buildingFiles); // Адаптировано под новую логику

            stopwatch.Stop();
            Logger.AddLog($"Парсинг зданий завершен. Время: {stopwatch.ElapsedMilliseconds} мс. Файлов: {buildingFiles.Count}, Конфигов всего: {buildingFiles.Sum(f => f.Entities.Count)}.");

            return buildingFiles; // Теперь возвращаем список файлов, а не конфигов
        }

        public static ConfigFile<BuildingConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            ConfigFile<BuildingConfig> configFile = new ConfigFile<BuildingConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket buildsBrk in hoiFuncFile.Brackets.Where(b => b.Name == "buildings"))
            {
                foreach (Bracket bracket in buildsBrk.SubBrackets)
                {
                    BuildingConfig buildingConfig = new BuildingConfig();
                    buildingConfig.Id = new Identifier(bracket.Name);
                    buildingConfig.FileFullPath = fileFullPath; // Устанавливаем путь для конфига
                    buildingConfig.IsOverride = isOverride;

                    foreach (Var buidVar in bracket.SubVars)
                    {
                        switch (buidVar.Name)
                        {
                            case "special_icon":
                                buildingConfig.SpecialIcon = buidVar.Value.ToInt();
                                break;
                            case "value":
                                buildingConfig.Health = buidVar.Value.ToInt();
                                break;
                            case "damage_factor":
                                buildingConfig.DamageFactor = buidVar.Value.ToInt();
                                break;
                            case "allied_build":
                                buildingConfig.AlliedBuild = buidVar.Value.ToBool();
                                break;
                            case "only_costal":
                                buildingConfig.OnlyCoastal = buidVar.Value.ToBool();
                                break;
                            case "disabled_in_dmz":
                                buildingConfig.DisabledInDmZones = buidVar.Value.ToBool();
                                break;
                            case "need_supply":
                                buildingConfig.NeedsSupply = buidVar.Value.ToBool();
                                break;
                            case "need_detection":
                                buildingConfig.NeedsDetection = buidVar.Value.ToBool();
                                break;
                            case "detecting_intel_type":
                                try
                                {
                                    buildingConfig.IntelType = Enum.Parse<IntelegenceType>(buidVar.Value.ToString().SnakeToPascal());
                                }
                                catch (Exception ex)
                                {
                                    Logger.AddLog($"[ParseFile] Cannot parse IntelType: '{buidVar.Value}' ({buidVar.Value?.GetType().Name}): {ex.Message}");
                                    buildingConfig.IntelType = default;
                                }
                                break;
                            case "only_display_if_exists":
                                buildingConfig.OnlyDisplayIfExists = buidVar.Value.ToBool();
                                break;
                            case "is_buildable":
                                buildingConfig.IsBuildable = buidVar.Value.ToBool();
                                break;
                            case "affects_energy":
                                buildingConfig.AffectsEnergy = buidVar.Value.ToBool();
                                break;
                            case "shares_slots":
                                buildingConfig.SharesSlots = buidVar.Value.ToBool();
                                break;
                            case "show_on_map":
                                buildingConfig.ShowOnMap = buidVar.Value.ToInt();
                                break;
                            case "show_on_map_meshes":
                                buildingConfig.ShowOnMapMeshes = buidVar.Value.ToInt();
                                break;
                            case "has_destroyed_mesh":
                                buildingConfig.HasDestroyedMesh = buidVar.Value.ToBool();
                                break;
                            case "centered":
                                buildingConfig.Centered = buidVar.Value.ToBool();
                                break;
                            case "base_cost":
                                buildingConfig.BaseCost = buidVar.Value.ToInt();
                                break;
                            case "per_level_extra_cost":
                                buildingConfig.PerLevelCost = buidVar.Value.ToInt();
                                break;
                            case "per_controlled_building_extra_cost":
                                buildingConfig.PerControlledBuildingExtraCost = buidVar.Value.ToInt();
                                break;
                            case "always_shown":
                                buildingConfig.AlwaysShown = buidVar.Value.ToBool();
                                break;
                            case "hide_if_missing_tech":
                                buildingConfig.HideIfMissingTech = buidVar.Value.ToBool();
                                break;
                        }
                    }

                    foreach (Bracket buildBr in bracket.SubBrackets)
                    {
                        switch (buildBr.Name)
                        {
                            case "dlc_allowed":
                                // TODO: understand what the fuck is this
                                break;
                            case "missing_tech_loc":
                                // TODO: same as prev
                                break;
                            case "specialization":
                                // TODO: special project, gotendamerung handling
                                break;
                            case "tags":
                                // TODO: same as one before the previous
                                break;
                            case "province_damage_modifiers":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    var file = ModDataStorage.Mod.ModifierDefinitions.
                                       FirstOrDefault(x => x.Entities.Any(e => e.Id.ToString() == pdm.Value.ToString()));
                                    ModifierDefinitionConfig modifierDefcon = file.FindById(pdm.Value.ToString());
                                    buildingConfig.ProvineDamageModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                            case "state_damage_modifier":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    var file = ModDataStorage.Mod.ModifierDefinitions.
                                        FirstOrDefault(x => x.Entities.Any(e => e.Id.ToString() == pdm.Value.ToString()));
                                    ModifierDefinitionConfig modifierDefcon = file.FindById(pdm.Value.ToString());

                                    buildingConfig.StateDamageModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                            case "level_cap":
                                foreach (Var lcvar in buildBr.SubVars)
                                {
                                    switch (lcvar.Name)
                                    {
                                        case "province_max":
                                            buildingConfig.MaxProvinceLevel = lcvar.Value.ToInt();
                                            break;
                                        case "state_max":
                                            buildingConfig.MaxStateLevel = lcvar.Value.ToInt();
                                            break;
                                        case "shares_slots":
                                            buildingConfig.SharesSlots = lcvar.Value.ToBool();
                                            break;
                                        case "group_by":
                                            buildingConfig.Group = lcvar.Value.ToString();
                                            break;
                                        case "exclusive_with":
                                            var building = ModDataStorage.Mod.Buildings
                                                .FirstOrDefault(b => b.Entities.Any(e => e.Id.ToString() == lcvar.Value));
                                            var entity = building?.Entities.FindById(lcvar.Value.ToString());

                                            buildingConfig.ExcludeWith = entity;
                                            break;
                                    }
                                }
                                break;
                            case "state_modifiers":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    ModifierDefinitionConfig modifierDefcon =ModDataStorage.Mod.ModifierDefinitions.FileEntitiesToList().FindById(pdm.Value.ToString());
                                    buildingConfig.StateModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                            case "country_modifiers":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    var file = ModDataStorage.Mod.ModifierDefinitions.
                                        FirstOrDefault(x => x.Entities.Any(e => e.Id.ToString() == pdm.Value.ToString()));
                                    ModifierDefinitionConfig modifierDefcon = file.FindById(pdm.Value.ToString());
                                    buildingConfig.CountryModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                        }
                    }

                    configFile.Entities.Add(buildingConfig);
                    Logger.AddDbgLog($"Добавлен конфиг: {buildingConfig.Id} в файл {configFile.FileName}.");
                }
            }

            return configFile;
        }

        public static void PaseDynamicModifierDefenitions(List<ConfigFile<BuildingConfig>> buildingFiles)
        {
            // Новая логика: добавляем в core_objects файл для модификаторов
            // Предполагаем, что есть Mod.ModifierDefinitionFiles; если нет - рекомендация: добавить List<ConfigFile<ModifierDefinitionConfig>> ModifierDefinitionFiles в ModDataStorage.Mod

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

            foreach (BuildingConfig bc in buildingFiles.SelectMany(f => f.Entities))
            {
                ModifierDefinitionConfig mdprod = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"state_production_speed_{bc.Id}_factor"),
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
                mdprod.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdprod.Id.ToString()));
                ModifierDefinitionConfig mdbs = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"production_speed_{bc.Id}_factor"),
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
                mdbs.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdbs.Id.ToString()));
                ModifierDefinitionConfig mdrepair = new ModifierDefinitionConfig
                {
                    Id = new Identifier($"state_repair_speed_{bc.Id}_factor"),
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
                mdrepair.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdrepair.Id.ToString()));

                coreFile.Entities.Add(mdprod);
                coreFile.Entities.Add(mdrepair);
                coreFile.Entities.Add(mdbs);
                Logger.AddDbgLog($"Добавлены определия модификаторов: {mdprod.Id}, {mdrepair.Id}, {mdbs.Id} в core_objects.");
            }
        }
    }
}