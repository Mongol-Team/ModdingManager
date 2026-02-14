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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class IdeologyComposer
    {
        /// <summary>
        /// Парсит все файлы идеологий и возвращает список файлов (ConfigFile<IdeologyConfig>)
        /// </summary>
        public static List<ConfigFile<IdeologyConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var ideologyFiles = new List<ConfigFile<IdeologyConfig>>();

            string[] possiblePaths =
            {
                ModPathes.IdeologyPath,
                GamePathes.IdeologyPath
            };

            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path))
                {
                    Logger.AddLog($"Директория не найдена: {path}");
                    continue;
                }

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        HoiFuncFile parsedFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                        bool isOverride = path.StartsWith(ModPathes.IdeologyPath);

                        var configFile = ParseFile(parsedFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            ideologyFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл идеологий: {configFile.FileName} → {configFile.Entities.Count} идеологий");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[IdeologyComposer] Ошибка парсинга файла {file}: {ex.Message}");
                        Logger.AddDbgLog($"Стек: {ex.StackTrace}");
                    }
                }
            }

            // Динамические модификаторы создаём после парсинга всех файлов
            PaseDynamicModifierDefinitions(ideologyFiles);

            stopwatch.Stop();
            Logger.AddLog($"Парсинг идеологий завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {ideologyFiles.Count}, идеологий всего: {ideologyFiles.Sum(f => f.Entities.Count)}");

            return ideologyFiles;
        }

        private static ConfigFile<IdeologyConfig> ParseFile(HoiFuncFile file, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<IdeologyConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            var ideologiesBracket = file.Brackets.FirstOrDefault(b => b.Name == "ideologies");
            if (ideologiesBracket == null)
            {
                Logger.AddDbgLog($"Блок 'ideologies' не найден в файле: {fileFullPath}");
                return configFile;
            }

            foreach (Bracket ideologyBr in ideologiesBracket.SubBrackets)
            {
                var config = ParseIdeologyConfig(ideologyBr.Name, ideologyBr, fileFullPath, isOverride);
                if (config != null)
                {
                    configFile.Entities.Add(config);
                    Logger.AddDbgLog($"  → добавлена идеология: {config.Id}");
                }
            }

            return configFile;
        }

        private static IdeologyConfig ParseIdeologyConfig(string name, Bracket bracket, string fileFullPath, bool isOverride)
        {
            var config = new IdeologyConfig
            {
                Id = new Identifier(name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                SubTypes = new List<IdeologyType>(),
                Rules = new Dictionary<RuleConfig, bool>(),
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                FactionModifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                DynamicFactionNames = new List<string>()
            };

            // Подтипы (types)
            var typesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "types");
            if (typesBracket != null)
            {
                foreach (Bracket typeBr in typesBracket.SubBrackets)
                {
                    var type = new IdeologyType
                    {
                        Id = new(typeBr.Name),
                        Parrent = name
                    };

                    var canBeRandom = typeBr.SubVars.FirstOrDefault(v => v.Name == "can_be_randomly_selected");
                    type.CanBeRandomlySelected = canBeRandom?.Value.ToBool() ?? false;

                    var colorVar = typeBr.SubVars.FirstOrDefault(v => v.Name == "color");
                    if (colorVar?.Value is Color colorObj)
                        type.Color = colorObj;

                    config.SubTypes.Add(type);
                }
            }

            // dynamic_faction_names
            var namesArr = bracket.Arrays.FirstOrDefault(a => a.Name == "dynamic_faction_names");
            if (namesArr != null)
            {
                config.DynamicFactionNames = namesArr.Values
                    .SelectMany(line => line.ToString().ParseQuotedStrings())
                    .ToList();
            }

            // color (основной)
            var colorMain = bracket.SubVars.FirstOrDefault(v => v.Name == "color");
            if (colorMain?.Value is Color mainColor)
                config.Color = mainColor;

            // rules
            var rulesBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "rules");
            if (rulesBracket != null)
            {
                foreach (Var v in rulesBracket.SubVars)
                {
                    var rule = ModDataStorage.Mod.Rules.SearchConfigInFile(v.Name);
                    if (rule != null)
                    {
                        config.Rules.TryAdd(rule, v.Value.ToBool());
                    }
                    else
                    {
                        Logger.AddDbgLog($"Правило {v.Name} не найдено для идеологии {name}");
                    }
                }
            }

            // modifiers
            var modsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "modifiers");
            if (modsBracket != null)
            {
                foreach (Var v in modsBracket.SubVars)
                {
                    var mod = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                    if (mod != null)
                    {
                        config.Modifiers.TryAdd(mod, v.Value);
                    }
                    else
                    {
                        Logger.AddDbgLog($"Модификатор {v.Name} не найден для идеологии {name}");
                    }
                }
            }

            // faction_modifiers
            var factionModsBracket = bracket.SubBrackets.FirstOrDefault(b => b.Name == "faction_modifiers");
            if (factionModsBracket != null)
            {
                foreach (Var v in factionModsBracket.SubVars)
                {
                    var mod = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                    if (mod != null)
                    {
                        config.FactionModifiers.TryAdd(mod, v.Value);
                    }
                    else
                    {
                        Logger.AddDbgLog($"Фракционный модификатор {v.Name} не найден для идеологии {name}");
                    }
                }
            }

            // Остальные переменные
            foreach (Var v in bracket.SubVars)
            {
                if (v.Name.StartsWith("ai_"))
                {
                    string aiName = v.Name.Substring(3).ToLower();
                    config.AiIdeologyName = aiName switch
                    {
                        "neutrality" => IdeologyAIType.Neutrality,
                        "democracy" => IdeologyAIType.Democracy,
                        "fascism" => IdeologyAIType.Fascism,
                        "communism" => IdeologyAIType.Communism,
                        _ => IdeologyAIType.None
                    };
                }

                switch (v.Name)
                {
                    case "can_host_government_in_exile":
                        config.CanFormExileGoverment = v.Value.ToBool();
                        break;
                    case "war_impact_on_world_tension":
                        config.WarImpactOnTension = v.Value.ToDouble();
                        break;
                    case "faction_impact_on_world_tension":
                        config.FactionImpactOnTension = v.Value.ToDouble();
                        break;
                    case "can_be_boosted":
                        config.CanBeBoosted = v.Value.ToBool();
                        break;
                    case "can_collaborate":
                        config.CanColaborate = v.Value.ToBool();
                        break;
                }
            }

            return config;
        }

        public static void PaseDynamicModifierDefinitions(List<ConfigFile<IdeologyConfig>> ideologyFiles)
        {
            // Ищем или создаём виртуальный core_objects
            var coreFile = ModDataStorage.Mod.ModifierDefinitions?
                .FirstOrDefault(f => f.FileName == "core_objects");

            if (coreFile == null)
            {
                coreFile = new ConfigFile<ModifierDefinitionConfig>
                {
                    FileFullPath = "core_objects",
                    IsCore = true,
                    Entities = new List<ModifierDefinitionConfig>()
                };
                ModDataStorage.Mod.ModifierDefinitions.Add(coreFile);
                Logger.AddDbgLog("Создан виртуальный core_objects для динамических модификаторов идеологий");
                
            }

            int created = 0;

            foreach (var file in ideologyFiles)
            {
                foreach (IdeologyConfig ideology in file.Entities)
                {
                    if (ideology.Id == null) continue;

                    // drift
                    var drift = new ModifierDefinitionConfig
                    {
                        Id = new Identifier($"{ideology.Id}_drift"),
                        IsCore = true,
                        Cathegory = ModifierDefinitionCathegoryType.Country,
                        ValueType = ModifierDefenitionValueType.Number,
                        ScopeType = ScopeTypes.Country,
                        ColorType = ModifierDefenitionColorType.Good,
                        Precision = 2,
                        FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                        Localisation = new ConfigLocalisation
                        {
                            Language = ModManagerSettings.CurrentLanguage
                        }
                    };
                    drift.Localisation.Data.AddPair(
                        ModDataStorage.Localisation.GetLocalisationByKey(drift.Id.ToString())
                    );

                    coreFile.Entities.Add(drift);
                    created++;

                    // acceptance
                    var acceptance = new ModifierDefinitionConfig
                    {
                        Id = new Identifier($"{ideology.Id}_acceptance"),
                        IsCore = true,
                        Cathegory = ModifierDefinitionCathegoryType.Country,
                        ValueType = ModifierDefenitionValueType.Number,
                        ScopeType = ScopeTypes.Country,
                        ColorType = ModifierDefenitionColorType.Good,
                        Precision = 2,
                        FileFullPath = DataDefaultValues.ItemCreatedDynamically,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.ItemWithNoGfx),
                        Localisation = new ConfigLocalisation
                        {
                            Language = ModManagerSettings.CurrentLanguage
                        }
                    };
                    acceptance.Localisation.Data.AddPair(
                        ModDataStorage.Localisation.GetLocalisationByKey(acceptance.Id.ToString())
                    );

                    coreFile.Entities.Add(acceptance);
                    created++;
                }
            }

            Logger.AddLog($"Создано {created} динамических модификаторов для идеологий (drift и acceptance)");
        }

     
    }
}