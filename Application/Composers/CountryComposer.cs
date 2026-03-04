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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Application.Composers
{
    public class CountryComposer
    {
        public static List<ConfigFile<CountryConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var countryFiles = new List<ConfigFile<CountryConfig>>();

            // Множество тегов, которые переопределены в моде (имеют приоритет)
            var overriddenByMod = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // ────────────────────────────────────────────────
            // 1. Сначала обрабатываем мод (самый высокий приоритет)
            // ────────────────────────────────────────────────
            string[] modTagFiles = Array.Empty<string>();
            if (Directory.Exists(ModPathes.CountryTagsPath))
            {
                 modTagFiles = Directory.GetFiles(ModPathes.CountryTagsPath, "*.txt", SearchOption.AllDirectories);
                foreach (string tagFilePath in modTagFiles)
                {
                    string content = File.ReadAllText(tagFilePath);
                    var tagFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    foreach (Var tagVar in tagFile.Vars)
                    {
                        if (tagVar.Name == "dynamic_tags" && tagVar.Value.ToBool())
                            continue;

                        string tag = tagVar.Name;
                        string countryFileRelative = tagVar.Value?.ToString() ?? string.Empty;

                        string actualHistoryPath = FindCountryHistoryPath(tag, countryFileRelative, tagFilePath, preferMod: true);
                        if (actualHistoryPath == null)
                            continue;

                        string historyContent = File.ReadAllText(actualHistoryPath);
                        var historyFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(historyContent);

                        var country = ParseCountryHistory(tag, countryFileRelative, actualHistoryPath, historyFile);
                        if (country == null)
                            continue;

                        var configFile = new ConfigFile<CountryConfig>
                        {
                            FileFullPath = actualHistoryPath,
                            IsOverride = true,
                            Entities = { country }
                        };

                        countryFiles.Add(configFile);
                        overriddenByMod.Add(tag);

                        Logger.AddDbgLog($"[MOD] Добавлена страна: {tag} ({country.Id}) из {Path.GetFileName(actualHistoryPath)}");
                    }
                }
            }

            // ────────────────────────────────────────────────
            // 2. Теперь обрабатываем ваниль, но пропускаем уже переопределённые модом теги
            // ────────────────────────────────────────────────
            if (Directory.Exists(GamePathes.CountryTagsPath))
            {
                string[] vanillaTagFiles = Directory.GetFiles(GamePathes.CountryTagsPath, "*.txt", SearchOption.AllDirectories);
                foreach (string tagFilePath in vanillaTagFiles)
                {
                    // Проверка: если файл с таким же именем уже есть в modTagFiles → пропускаем
                    string fileName = Path.GetFileName(tagFilePath);
                    if (modTagFiles.Any(mf => Path.GetFileName(mf).Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Logger.AddDbgLog($"[VANILLA skipped] Файл {fileName} переопределён модом → пропущен");
                        continue;
                    }


                    string content = File.ReadAllText(tagFilePath);
                    var tagFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    foreach (Var tagVar in tagFile.Vars)
                    {
                        if (tagVar.Name == "dynamic_tags" && tagVar.Value.ToBool())
                            continue;

                        string tag = tagVar.Name;

                        // ← Ключевая проверка: если тег уже переопределён модом → пропускаем
                        if (overriddenByMod.Contains(tag))
                        {
                            Logger.AddDbgLog($"[VANILLA skipped] Страна {tag} переопределена в моде → пропущена");
                            continue;
                        }

                        string countryFileRelative = tagVar.Value?.ToString() ?? string.Empty;

                        string actualHistoryPath = FindCountryHistoryPath(tag, countryFileRelative, tagFilePath, preferMod: false);
                        if (actualHistoryPath == null)
                            continue;

                        string historyContent = File.ReadAllText(actualHistoryPath);
                        var historyFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(historyContent);

                        var country = ParseCountryHistory(tag, countryFileRelative, actualHistoryPath, historyFile);
                        if (country == null)
                            continue;

                        var configFile = new ConfigFile<CountryConfig>
                        {
                            FileFullPath = actualHistoryPath,
                            IsOverride = false,
                            Entities = { country }
                        };

                        countryFiles.Add(configFile);

                        Logger.AddDbgLog($"[VANILLA] Добавлена страна: {tag} ({country.Id}) из {Path.GetFileName(actualHistoryPath)}");
                    }
                }
            }


            stopwatch.Stop();
            Logger.AddLog($"Парсинг стран завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Стран: {countryFiles.Sum(f => f.Entities.Count)} (файлов: {countryFiles.Count}) " +
                          $"(мод: {overriddenByMod.Count}, ваниль: {countryFiles.Count - overriddenByMod.Count})");

            return countryFiles;
        }


        private static string FindCountryHistoryPath(string tag, string relativePath, string tagFilePath, bool preferMod)
        {
            var candidates = new List<string>();

            // Приоритет мода → выше
            if (preferMod)
            {
                candidates.Add(Path.Combine(ModPathes.HistoryPath, relativePath.Replace("/", "\\")));
                candidates.Add(Path.Combine(ModPathes.HistoryCountriesPath, relativePath.Replace("/", "\\")));
            }

            candidates.Add(Path.Combine(GamePathes.HistoryPath, relativePath.Replace("/", "\\")));
            candidates.Add(Path.Combine(GamePathes.HistoryCountriesPath, relativePath.Replace("/", "\\")));

            // Если не preferMod — модовые пути всё равно можно добавить в конец (fallback)
            if (!preferMod)
            {
                candidates.Insert(0, Path.Combine(ModPathes.HistoryPath, relativePath.Replace("/", "\\")));
                candidates.Insert(1, Path.Combine(ModPathes.HistoryCountriesPath, relativePath.Replace("/", "\\")));
            }

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            // Fallback-поиск по имени файла (содержит тег)
            string[] searchDirs =
            {
            ModPathes.HistoryCountriesPath,
            GamePathes.HistoryCountriesPath
        };

            foreach (var dir in searchDirs.Where(Directory.Exists))
            {
                var file = Directory.GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Contains(tag, StringComparison.OrdinalIgnoreCase));

                if (file != null)
                    return file;
            }

            return null;
        }
        /// <summary>
        /// Парсит файл истории одной страны по относительному пути из tags
        /// </summary>
        private static CountryConfig ParseCountryHistory(string tag, string relativePath, string actualHistoryPath, HoiFuncFile file)
        {
           

            var country = new CountryConfig
            {
                Id = new Identifier(tag),
                FileFullPath = actualHistoryPath,
                CountryFileName = Path.GetFileName(relativePath)
            };

            // set_technology
            var techBracket = file.Brackets.FirstOrDefault(b => b.Name == "set_technology");
            if (techBracket != null)
            {
                foreach (Var v in techBracket.SubVars)
                {
                    if (string.IsNullOrEmpty(v.Name) || v.Name.Length < 4) continue;

                    string techId = v.Name.Substring(4);
                    var techItem = ModDataStorage.Mod.TechTreeItems.SearchConfigInFile(techId);
                    if (techItem != null && int.TryParse(v.Value?.ToString(), out int level))
                    {
                        country.Technologies[techItem] = level;
                    }
                }
            }

            // set_popularities
            var popBracket = file.Brackets.FirstOrDefault(b => b.Name == "set_popularities");
            if (popBracket != null)
            {
                foreach (Var v in popBracket.SubVars)
                {
                    var ideology = ModDataStorage.Mod.Ideologies.SearchConfigInFile(v.Name);
                    if (ideology != null && int.TryParse(v.Value?.ToString(), out int pop))
                    {
                        country.PartyPopularities[ideology] = pop;
                    }
                }
            }

            // add_ideas
            var ideasBracket = file.Brackets.FirstOrDefault(b => b.Name == "add_ideas");
            if (ideasBracket != null)
            {
                foreach (Var v in ideasBracket.SubVars)
                {
                    var idea = ModDataStorage.Mod.Ideas.SearchConfigInFile(v.Name);
                    if (idea != null)
                        country.Ideas.Add(idea);
                }
            }

            // add_character
            var charBracket = file.Brackets.FirstOrDefault(b => b.Name == "add_character");
            if (charBracket != null)
            {
                foreach (Var v in charBracket.SubVars)
                {
                    var character = ModDataStorage.Mod.Characters.SearchConfigInFile(v.Name);
                    if (character != null)
                        country.Characters.Add(character);
                }
            }

            foreach(StateConfig state in ModDataStorage.Mod.Map.States.SelectMany(s => s.Entities).Where(s => s.OwnerTag == country.Id.ToString()))
            {
                country.States.Add(state);
            }
            // Простые переменные
            country.Convoys = file.Vars.FirstOrDefault(v => v.Name == "set_convoys")?.Value.ToInt() ?? 0;
            country.Stab = file.Vars.FirstOrDefault(v => v.Name == "set_stability")?.Value.ToDouble() ?? 0.0;
            country.WarSup = file.Vars.FirstOrDefault(v => v.Name == "set_war_support")?.Value.ToDouble() ?? 0.0;
            country.ResearchSlots = file.Vars.FirstOrDefault(v => v.Name == "set_research_slots")?.Value.ToInt() ?? 1;

            // set_politics
            var politics = file.Brackets.FirstOrDefault(b => b.Name == "set_politics");
            if (politics != null)
            {
                foreach (Var v in politics.SubVars)
                {
                    switch (v.Name)
                    {
                        case "ruling_party":
                            country.RulingParty = ModDataStorage.Mod.Ideologies.SearchConfigInFile(v.Value?.ToString());
                            break;
                        case "last_election":
                            if (DateOnly.TryParse(v.Value?.ToString(), out DateOnly dt))
                                country.LastElection = dt;
                            break;
                        case "election_frequency":
                            country.ElectionFrequency = v.Value.ToInt();
                            break;
                        case "elections_allowed":
                            country.ElectionsAllowed = v.Value.ToBool();
                            break;
                    }
                }
            }

            // capital
            var capitalVar = file.Vars.FirstOrDefault(v => v.Name == "capital");
            country.Capital = capitalVar?.Value.ToInt() ?? -1;

            // oob
            var oobVar = file.Vars.FirstOrDefault(v => v.Name == "oob");
            country.OOB = oobVar?.Value?.ToString() ?? string.Empty;

            return country;
        }
    }
}