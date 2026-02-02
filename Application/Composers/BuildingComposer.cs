using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.Enums;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;

namespace Application.Composers
{
    public class BuildingComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new();
            string[] possiblePathes =
            {
                ModPathes.BuildingsPath,
                GamePathes.BuildingsPath
            };
            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string fileContent = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);
                        List<IConfig> buildingConfigs = ParseFile(hoiFuncFile);
                        foreach (BuildingConfig buildingConfig in buildingConfigs)
                        {
                            buildingConfig.FileFullPath = file;
                            if (!configs.Any(c => c.Id == buildingConfig.Id))
                            {
                                configs.Add(buildingConfig);
                            }
                        }
                    }
                }
            }
            PaseDynamicModifierDefenitions(configs);
            return configs;
        }

        public static List<IConfig> ParseFile(HoiFuncFile hoiFuncFil)
        {
            List<IConfig> cfgs = new();
            foreach (Bracket buildsBrk in hoiFuncFil.Brackets.Where(b => b.Name == "buildings"))
            {
                
                foreach (Bracket bracket in buildsBrk.SubBrackets)
                {
                    BuildingConfig buildingConfig = new();
                    buildingConfig.Id = new(bracket.Name);
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
                                Enum.TryParse<IntelegenceType>(buidVar.Value.ToString(), out var intelRes);
                                buildingConfig.IntelType = intelRes;
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
                                //todo: understand what the fuck is this
                                break;
                            case "missing_tech_loc":
                                //todo: same as prev
                                break;
                            case "specialization":
                                //todo: special project, gotendamerung handling
                                break;
                            case "tags":
                                //todo: same as one before the previous
                                break;
                            case "province_damage_modifiers":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    ModifierDefinitionConfig modifierDefcon = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(x => x.Id.ToString() == pdm.Name);
                                    buildingConfig.ProvineDamageModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                            case "state_damage_modifier":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    ModifierDefinitionConfig modifierDefcon = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(x => x.Id.ToString() == pdm.Name);
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
                                            buildingConfig.ExcludeWith = ModDataStorage.Mod.Buildings.FirstOrDefault(b => b.Id.ToString() == lcvar.Value);
                                            break;

                                    }
                                }
                                break;
                            case "state_modifiers":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    ModifierDefinitionConfig modifierDefcon = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(x => x.Id.ToString() == pdm.Name);
                                    buildingConfig.StateModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                            case "country_modifiers":
                                foreach (Var pdm in buildBr.SubVars)
                                {
                                    ModifierDefinitionConfig modifierDefcon = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(x => x.Id.ToString() == pdm.Name);
                                    buildingConfig.CountryModifiers.AddSafe(modifierDefcon, pdm.Value);
                                }
                                break;
                        }
                    }
                    cfgs.Add(buildingConfig);
                }
            }

            return cfgs;
        }

        public static void PaseDynamicModifierDefenitions(List<IConfig> configs)
        {
            foreach (BuildingConfig bc in configs)
            {
                ModifierDefinitionConfig mdprod = new();
                mdprod.Id = new($"state_production_speed_{bc.Id}_factor");
                mdprod.FileFullPath = DataDefaultValues.ItemCreatedDynamically;
                mdprod.IsCore = true;
                mdprod.Cathegory = ModifierDefinitionCathegoryType.Country;
                mdprod.Precision = 2;
                mdprod.ScopeType = ScopeTypes.Country;
                mdprod.ColorType = ModifierDefenitionColorType.Good;
                mdprod.Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx);
                mdprod.Localisation = new ConfigLocalisation()
                {
                    Language = ModManagerSettings.CurrentLanguage,
                };
                mdprod.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdprod.Id.ToString()));

                ModifierDefinitionConfig mdrepair = mdprod;
                mdrepair.Id = new($"state_repair_speed_{bc.Id}_factor");
                mdrepair.Localisation = new ConfigLocalisation()
                {
                    Language = ModManagerSettings.CurrentLanguage,
                };
                mdrepair.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(mdrepair.Id.ToString()));

                ModDataStorage.Mod.ModifierDefinitions.Add(mdrepair);
                ModDataStorage.Mod.ModifierDefinitions.Add(mdprod);
            }
        }
    }
}
