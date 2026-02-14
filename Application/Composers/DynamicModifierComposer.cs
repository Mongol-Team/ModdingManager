using Application.Debugging;
using Application.Extensions;
using Application.extentions;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles;
using Models.Interfaces;
using Models.Types;
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
    public class DynamicModifierComposer
    {
        /// <summary>
        /// Парсит все файлы динамических модификаторов → возвращает список файлов (ConfigFile<DynamicModifierConfig>)
        /// </summary>
        public static List<ConfigFile<DynamicModifierConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var modifierFiles = new List<ConfigFile<DynamicModifierConfig>>();

            string[] possiblePaths =
            {
                ModPathes.DynamicModifiersPath,
                GamePathes.DynamicModifiersPath
            };

            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    HoiFuncFile funcFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    bool isOverride = path.StartsWith(ModPathes.DynamicModifiersPath);

                    var configFile = ParseFile(funcFile, file, isOverride);

                    if (configFile.Entities.Any())
                    {
                        modifierFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл динамических модификаторов: {configFile.FileName} " +
                                         $"({configFile.Entities.Count} модификаторов)");
                    }
                }
            }


            stopwatch.Stop();
            Logger.AddLog($"Парсинг динамических модификаторов завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {modifierFiles.Count}, модификаторов всего: {modifierFiles.Sum(f => f.Entities.Count)}");

            return modifierFiles;
        }

        private static ConfigFile<DynamicModifierConfig> ParseFile(HoiFuncFile funcFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<DynamicModifierConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket bracket in funcFile.Brackets)
            {
                var cfg = ParseSingleModifier(bracket, fileFullPath, isOverride);
                if (cfg != null)
                {
                    configFile.Entities.Add(cfg);
                    Logger.AddDbgLog($"  → добавлен динамический модификатор: {cfg.Id}");
                }
            }

            return configFile;
        }

        private static DynamicModifierConfig ParseSingleModifier(Bracket bracket, string fileFullPath, bool isOverride)
        {
            var cfg = new DynamicModifierConfig
            {
                Id = new Identifier(bracket.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>()
            };

            foreach (Var v in bracket.SubVars)
            {
                if (v.Name == "icon" && v.Value is HoiReference refIcon)
                {
                    cfg.Gfx = ModDataStorage.Mod.Gfxes.SearchConfigInFile(refIcon.Value?.ToString());
                }
                else if (v.Name == "attacker_modifier" && v.Value is bool hasAttacker)
                {
                    cfg.HasAttackerEffect = hasAttacker;
                }
                else
                {
                    // Ищем модификатор по имени переменной
                    var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                    if (modDef != null)
                    {
                        object value = v.Value switch
                        {
                            int i => i,
                            double d => d,
                            bool b => b,
                            string s => s,
                            HoiReference r => r,
                            _ => v.Value?.ToString() ?? string.Empty
                        };

                        cfg.Modifiers[modDef] = value;
                    }
                    else
                    {
                        Logger.AddLog($"Неизвестный модификатор или свойство в {bracket.Name}: {v.Name}");
                    }
                }
            }

            return cfg;
        }

       
    }
}