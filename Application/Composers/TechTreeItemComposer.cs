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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class TechTreeItemComposer 
    {
        /// <summary>
        /// Парсит все файлы технологий (research_*.txt) и возвращает список файлов (ConfigFile<TechTreeItemConfig>)
        /// </summary>
        public static List<ConfigFile<TechTreeItemConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var techItemFiles = new List<ConfigFile<TechTreeItemConfig>>();

            string[] possiblePathes =
            {
                ModPathes.TechTreePath,
                GamePathes.TechTreePath
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

                        bool isOverride = path.StartsWith(ModPathes.TechTreePath);

                        var configFile = ParseFile(hoiFuncFile, file, isOverride);

                        if (configFile.Entities.Any())
                        {
                            techItemFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлен файл технологий: {configFile.FileName} → {configFile.Entities.Count} технологий");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddLog($"[TechTreeItemComposer] Ошибка парсинга файла {file}: {ex.Message}");
                    }
                }
            }

            // Динамические модификаторы здесь не создаются (их нет в этом композере)

            stopwatch.Stop();
            Logger.AddLog($"Парсинг технологий завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {techItemFiles.Count}, технологий всего: {techItemFiles.Sum(f => f.Entities.Count)}");

            return techItemFiles;
        }

        private static ConfigFile<TechTreeItemConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<TechTreeItemConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            var technologiesBracket = hoiFuncFile.Brackets.FirstOrDefault(b => b.Name == "technologies");
            if (technologiesBracket == null) return configFile;

            foreach (Bracket techBr in technologiesBracket.SubBrackets)
            {
                var config = ParseTechItem(techBr, fileFullPath, isOverride);
                if (config != null)
                {
                    configFile.Entities.Add(config);
                    Logger.AddDbgLog($"  → добавлена технология: {config.Id}");
                }
            }

            return configFile;
        }

        private static TechTreeItemConfig ParseTechItem(Bracket techBr, string fileFullPath, bool isOverride)
        {
            string id = techBr.Name;
            if (string.IsNullOrWhiteSpace(id)) return null;

            var item = new TechTreeItemConfig
            {
                Id = new Identifier(id),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                EnableBuildings = new Dictionary<BuildingConfig, object>(),
                EnableEquipments = new List<EquipmentConfig>(),
                EnableUnits = new List<SubUnitConfig>(),
                Categories = new List<TechCategoryConfig>(),
                Mutal = new List<TechTreeItemConfig>(),
                ChildOf = new List<TechTreeItemConfig>()
            };

            // Простые переменные
            foreach (Var v in techBr.SubVars)
            {
                switch (v.Name.ToLowerInvariant())
                {
                    case "research_cost":
                        item.Cost = v.Value.ToInt();
                        break;

                    case "start_year":
                        item.StartYear = v.Value.ToInt();
                        break;

                    case "force_use_small_tech_layout":
                        item.IsBig = !v.Value.ToBool(); // в оригинале IsBig = !force_small
                        break;

                    case "show_equipment_icon":
                        item.ShowEqIcon = v.Value.ToBool();
                        break;

                    case "desc":
                        item.SpecialDescKey = v.Value?.ToString();
                        break;

                    default:
                        var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                        if (modDef != null)
                            item.Modifiers.Add(modDef, v.Value);
                        break;
                }
            }

            // Подблоки
            foreach (Bracket subBr in techBr.SubBrackets)
            {
                switch (subBr.Name.ToLowerInvariant())
                {
                    case "path":
                        var coef = subBr.SubVars.FirstOrDefault(v => v.Name == "research_cost_coeff");
                        if (coef != null)
                            item.ModifCost = coef.Value.ToInt();

                        var childV = subBr.SubVars.FirstOrDefault(v => v.Name == "leads_to_tech");
                        if (childV != null)
                        {
                            var childTech = ModDataStorage.Mod.TechTreeItems.SearchConfigInFile(childV.Value?.ToString());
                            if (childTech != null)
                                childTech.ChildOf.Add(item);
                        }
                        break;

                    case "folder":
                        var flName = subBr.SubVars.FirstOrDefault(v => v.Name == "name");
                        if (flName != null)
                        {
                            var folder = ModDataStorage.Mod.TechTreeLedgers.SearchConfigInFile(flName.Value?.ToString());
                            if (folder != null)
                                folder.Items.Add(item);
                        }

                        var posBr = subBr.SubBrackets.FirstOrDefault(b => b.Name == "position");
                        if (posBr != null)
                        {
                            var x = posBr.SubVars.FirstOrDefault(v => v.Name == "x")?.Value.ToInt() ?? 0;
                            var y = posBr.SubVars.FirstOrDefault(v => v.Name == "y")?.Value.ToInt() ?? 0;
                            item.GridX = x;
                            item.GridY = y;
                        }
                        break;

                    case "enable_building":
                        var buildingVar = subBr.SubVars.FirstOrDefault(v => v.Name == "building");
                        var levelVar = subBr.SubVars.FirstOrDefault(v => v.Name == "level");
                        if (buildingVar != null && levelVar != null)
                        {
                            var building = ModDataStorage.Mod.Buildings.SearchConfigInFile(buildingVar.Value?.ToString());
                            if (building != null)
                                item.EnableBuildings.Add(building, levelVar.Value.ToInt());
                        }
                        break;

                    case "on_research_complete":
                        item.Effects = subBr.ToString();
                        break;

                    case "ai_will_do":
                        item.AiWillDo = subBr.ToString();
                        break;

                    case "allow_branch":
                        item.AllowBranch = subBr.ToString();
                        break;

                    case "allowed":
                        item.Allowed = subBr.ToString();
                        break;

                    case "xor":
                        // обрабатывается в массивах ниже
                        break;
                }
            }

            // Массивы
            foreach (HoiArray arr in techBr.Arrays)
            {
                switch (arr.Name.ToLowerInvariant())
                {
                    case "xor":
                        foreach (var value in arr.Values)
                        {
                            var xorItem = ModDataStorage.Mod.TechTreeItems.SearchConfigInFile(value?.ToString());
                            if (xorItem != null)
                                item.Mutal.Add(xorItem);
                        }
                        break;

                    case "categories":
                        foreach (var val in arr.Values)
                        {
                            var category = ModDataStorage.Mod.TechCategories.SearchConfigInFile(val?.ToString());
                            if (category != null)
                                item.Categories.Add(category);
                        }
                        break;

                    case "enable_equipments":
                        foreach (var val in arr.Values)
                        {
                            var equip = ModDataStorage.Mod.Equipments.SearchConfigInFile(val?.ToString());
                            if (equip != null)
                                item.EnableEquipments.Add(equip);
                        }
                        break;

                    case "enable_subunits":
                        foreach (var val in arr.Values)
                        {
                            var subunit = ModDataStorage.Mod.SubUnits.SearchConfigInFile(val?.ToString());
                            if (subunit != null)
                                item.EnableUnits.Add(subunit);
                        }
                        break;

                    case "enable_equipment_modules":
                        // todo: modules (если нужно — добавь позже)
                        break;
                }
            }

            return item;
        }
    }
}