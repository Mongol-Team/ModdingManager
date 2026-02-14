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
using System.Diagnostics;
using System.Linq;

namespace Application.Composers
{
    public class CharacterTraitComposer 
    {
        /// <summary>
        /// Парсит все файлы с лидер-трайтами и возвращает список файлов (ConfigFile<CharacterTraitConfig>)
        /// </summary>
        public static List<ConfigFile<CharacterTraitConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var traitFiles = new List<ConfigFile<CharacterTraitConfig>>();

            string[] possiblePathes =
            {
                ModPathes.TraitsPath,
                GamePathes.TraitsPath
            };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string fileContent = File.ReadAllText(file);
                    HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);

                    bool isOverride = path.StartsWith(ModPathes.TraitsPath); // или более точная логика

                    var configFile = ParseFile(hoiFuncFile, file, isOverride);

                    if (configFile.Entities.Any())
                    {
                        traitFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл трейтов: {configFile.FileName} ({configFile.Entities.Count} трейтов)");
                    }
                }
            }


            stopwatch.Stop();
            Logger.AddLog($"Парсинг лидер-трейтов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {traitFiles.Count}, трейтов всего: {traitFiles.Sum(f => f.Entities.Count)}");

            return traitFiles;
        }

        private static ConfigFile<CharacterTraitConfig> ParseFile(HoiFuncFile file, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<CharacterTraitConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (var bracket in file.Brackets)
            {
                if (bracket.Name == "leader_traits")
                {
                    foreach (var traitBracket in bracket.SubBrackets)
                    {
                        var traitConfig = ParseTraitObject(traitBracket, fileFullPath, isOverride);
                        if (traitConfig != null)
                        {
                            configFile.Entities.Add(traitConfig);
                            Logger.AddDbgLog($"  → добавлен трейт: {traitConfig.Id}");
                        }
                    }
                }
                else
                {
                    Logger.AddLog($"Неизвестный блок при парсинге трейтов: {bracket.Name} в файле {fileFullPath}");
                }
            }

            return configFile;
        }

        private static CharacterTraitConfig ParseTraitObject(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var trait = new CharacterTraitConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (var v in bracket.SubVars)
            {
                switch (v.Name)
                {
                    case "random":
                        trait.Random = v.Value.ToString().ToLower() == "yes";
                        break;

                    case "sprite":
                        trait.Sprite = v.Value.ToInt();
                        break;

                    case "trait_type":
                        trait.TraitType = v.Value.ToString().SnakeToPascal().ToEnum<TraitType>(default);
                        break;

                    case "show_in_combat":
                        trait.ShowInCombat = v.Value.ToBool();
                        break;

                    case "slot":
                        trait.CharacterSlot = ModDataStorage.Mod.IdeaSlots.SearchConfigInFile(v.Value?.ToString());
                        break;

                    case "specialist_advisor_trait":
                        trait.SpecialistAdvisorTrait = ModDataStorage.Mod.CharacterTraits.SearchConfigInFile(v.Value?.ToString());
                        break;

                    case "expert_advisor_trait":
                        trait.ExpertAdvisorTrait = ModDataStorage.Mod.CharacterTraits.SearchConfigInFile(v.Value?.ToString());
                        break;

                    case "genius_advisor_trait":
                        trait.GeniusAdvisorTrait = ModDataStorage.Mod.CharacterTraits.SearchConfigInFile(v.Value?.ToString());
                        break;

                    case "enable_ability":
                        // TODO: реализация обработки ability
                        break;

                    case "mutually_exclusive":
                        trait.MutuallyExclusives = v.Value.ToString()
                            .Split(',')
                            .Select(me => ModDataStorage.Mod.CharacterTraits.SearchConfigInFile(me.Trim()))
                            .Where(t => t != null)
                            .ToList();
                        break;

                    case "parent":
                        trait.Parents = v.Value.ToString()
                            .Split(',')
                            .Select(p => ModDataStorage.Mod.CharacterTraits.SearchConfigInFile(p.Trim()))
                            .Where(t => t != null)
                            .ToList();
                        break;

                    case "num_parents_needed":
                        trait.NumParentsRequired = v.Value.ToInt();
                        break;

                    case "gui_row":
                        trait.GuiRow = v.Value.ToInt();
                        break;

                    case "gui_column":
                        trait.GuiColumn = v.Value.ToInt();
                        break;

                    case "cost":
                        trait.Cost = v.Value.ToDouble();
                        break;

                    case "gain_xp_on_spotting":
                        trait.GainXpOnSpotting = v.Value.ToDouble();
                        break;

                    default:
                        // Пытаемся найти модификатор
                        var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                        if (modDef != null)
                        {
                            trait.Modifiers.Add(modDef, v.Value);
                        }
                        // Или skill
                        else if (v.Name.EndsWith("skill"))
                        {
                            var skillType = v.Name switch
                            {
                                "skill" => CharacterSkillType.Skill,
                                "attack_skill" => CharacterSkillType.Attack,
                                "defense_skill" => CharacterSkillType.Defense,
                                "planning_skill" => CharacterSkillType.Planning,
                                "logistics_skill" => CharacterSkillType.Logistics,
                                "maneuvering_skill" => CharacterSkillType.Maneuvering,
                                _ => default
                            };
                            if (skillType != default)
                            {
                                trait.SkillTypes.Add(skillType, v.Value.ToInt());
                            }
                        }
                        else
                        {
                            Logger.AddDbgLog($"Неизвестная переменная при парсинге трейта {bracket.Name}: {v.Name}");
                        }
                        break;
                }
            }

            foreach (var br in bracket.SubBrackets)
            {
                switch (br.Name)
                {
                    case "allowed":
                        trait.Allowed = br.ToString(); // TODO: парсинг триггеров
                        break;

                    case "ai_will_do":
                        trait.AiWillDo = br.ToString(); // TODO: ai will do
                        break;

                    case "unit_type":
                        foreach (var var in br.SubVars)
                        {
                            if (var.Name == "type")
                            {
                                var subUnit = ModDataStorage.Mod.SubUnits.SearchConfigInFile(var.Value?.ToString());
                                if (subUnit != null)
                                    trait.UnitType.Add(subUnit);
                            }
                            else
                            {
                                Logger.AddDbgLog($"Неизвестная переменная в unit_type трейта {bracket.Name}: {var.Name}");
                            }
                        }
                        break;

                    case "unit_trigger":
                        trait.UnitTrigger = br.ToString(); // TODO: триггеры
                        break;

                    case "on_add":
                        trait.OnAdd = br.ToString(); // TODO: эффекты
                        break;

                    case "on_remove":
                        trait.OnRemove = br.ToString(); // TODO: эффекты
                        break;

                    case "daily_effect":
                        trait.DailyEffect = br.ToString(); // TODO: эффекты
                        break;

                    case "modifier":
                        foreach (var var in br.SubVars)
                        {
                            var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(var.Name);
                            if (modDef != null)
                                trait.ArmyComanderModifiers.Add(modDef, var.Value);
                        }
                        break;

                    case "non_shared_modifier":
                        foreach (var var in br.SubVars)
                        {
                            var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(var.Name);
                            if (modDef != null)
                                trait.NonSharedModifiers.Add(modDef, var.Value);
                        }
                        break;

                    case "corps_commander_modifier":
                        foreach (var var in br.SubVars)
                        {
                            var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(var.Name);
                            if (modDef != null)
                                trait.CorpsCommanderModifiers.Add(modDef, var.Value);
                        }
                        break;

                    case "field_marshal_modifier":
                        foreach (var var in br.SubVars)
                        {
                            var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(var.Name);
                            if (modDef != null)
                                trait.FieldMarshalModifiers.Add(modDef, var.Value);
                        }
                        break;

                    case "sub_unit_modifier":
                        foreach (var var in br.SubVars)
                        {
                            var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(var.Name);
                            if (modDef != null)
                                trait.SubUnitModifiers.Add(modDef, var.Value);
                        }
                        break;

                    case "prerequisites":
                        trait.Prerequisites = br.ToString(); // TODO: триггеры
                        break;

                    case "gain_xp":
                        trait.GainXp = br.ToString(); // TODO: триггеры
                        break;

                    case "trait_xp_factor":
                        trait.TraitXpFactor = br.ToString(); // TODO: триггеры
                        break;
                }
            }

            foreach (var array in bracket.Arrays)
            {
                if (array.Name == "type")
                {
                    foreach (var value in array.Values)
                    {
                        if (value != null && Enum.TryParse<CharacterType>(value.ToString().SnakeToPascal(), out var charType))
                        {
                            trait.CharacterTypes.Add(charType);
                        }
                    }
                }
            }

            // Локализация (если есть name/desc — добавляем, хотя для трейтов обычно через локализацию по id)
            trait.Localisation = new ConfigLocalisation
            {
                Language = ModManagerSettings.CurrentLanguage,
                Source = trait
            };
            // Можно добавить:
            trait.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(trait.Id.ToString()));
            trait.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(trait.Id + "_desc"));

            return trait;
        }

    }
}