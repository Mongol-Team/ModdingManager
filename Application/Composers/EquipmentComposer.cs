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
using Models.Interfaces;
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
    public class EquipmentComposer
    {
        /// <summary>
        /// Парсит все файлы снаряжения и возвращает список файлов (ConfigFile<EquipmentConfig>)
        /// </summary>
        public static List<ConfigFile<EquipmentConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var equipmentFiles = new List<ConfigFile<EquipmentConfig>>();

            string[] possiblePathes =
            {
                ModPathes.EquipmentsPath,
                GamePathes.EquipmentsPath
            };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    bool isOverride = path.StartsWith(ModPathes.EquipmentsPath);

                    var configFile = ParseFile(hoiFuncFile, file, isOverride);

                    if (configFile.Entities.Any())
                    {
                        equipmentFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл снаряжения: {configFile.FileName} → {configFile.Entities.Count} единиц");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг снаряжения завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {equipmentFiles.Count}, единиц всего: {equipmentFiles.Sum(f => f.Entities.Count)}");

            return equipmentFiles;
        }

        private static ConfigFile<EquipmentConfig> ParseFile(HoiFuncFile hoiFuncFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<EquipmentConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket equipmentsBr in hoiFuncFile.Brackets.Where(b => b.Name == "equipments"))
            {
                foreach (Bracket br in equipmentsBr.SubBrackets)
                {
                    var config = new EquipmentConfig
                    {
                        Id = new Identifier(br.Name),
                        FileFullPath = fileFullPath,
                        IsOverride = isOverride
                    };

                    // Подблоки
                    foreach (Bracket subBr in br.SubBrackets)
                    {
                        switch (subBr.Name)
                        {
                            case "resources":
                                foreach (Var v in subBr.SubVars)
                                {
                                    var resource = ModDataStorage.Mod.Resources.SearchConfigInFile(v.Name);
                                    if (resource != null)
                                    {
                                        config.Cost.AddSafe(resource, v.Value.ToInt());
                                    }
                                    else
                                    {
                                        Logger.AddDbgLog($"Неверное имя ресурса в стоимости {br.Name}: {v.Name} (файл: {fileFullPath})");
                                    }
                                }
                                break;

                            case "can_be_produced":
                                config.CanBeProduced = subBr.ToString(); // todo: raw trigger data
                                break;
                        }
                    }

                    // Массивы
                    foreach (HoiArray arr in br.Arrays)
                    {
                        if (arr.Name == "type")
                        {
                            foreach (var item in arr.Values)
                            {
                                if (Enum.TryParse<IternalUnitType>(item.ToString().SnakeToPascal(), out var unitType))
                                {
                                    config.Type.AddSafe(unitType);
                                }
                            }
                        }
                    }

                    // Переменные
                    foreach (Var v in br.SubVars)
                    {
                        switch (v.Name)
                        {
                            case "year":
                                config.Year = v.Value.ToInt();
                                break;

                            case "is_archetype":
                                config.IsArchetype = v.Value.ToBool();
                                break;

                            case "is_buildable":
                                config.IsBuidable = v.Value.ToBool();
                                break;

                            case "is_active":
                                config.IsActive = v.Value.ToBool();
                                break;

                            case "type":
                                if (Enum.TryParse<IternalUnitType>(v.Value.ToString().SnakeToPascal(), out var t))
                                {
                                    config.Type.AddSafe(t);
                                }
                                break;

                            case "picture":
                                string gfxName = v.Value != null
                                    ? $"GFX_{v.Value}_medium"
                                    : $"GFX_{v.Value?.ToString() ?? "_small"}";

                                config.Gfx = ModDataStorage.Mod.Gfxes.SearchConfigInFile(gfxName);
                                break;

                            case "archetype":
                                config.Archetype = ModDataStorage.Mod.Equipments.SearchConfigInFile(v.Value?.ToString());
                                break;

                            case "interface_category":
                                if (Enum.TryParse<EquipmentInterfaceCategory>(v.Value.ToString().SnakeToPascal(), out var cat))
                                {
                                    config.InterfaceType = cat;
                                }
                                break;
                        }
                    }

                    configFile.Entities.Add(config);
                    Logger.AddDbgLog($"  → добавлено снаряжение: {config.Id}");
                }
            }

            return configFile;
        }

     
    }
}