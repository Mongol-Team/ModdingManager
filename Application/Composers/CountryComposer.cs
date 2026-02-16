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
        /// <summary>
        /// Парсит теги стран и соответствующие файлы истории → возвращает список файлов с конфигами стран
        /// </summary>
        public static List<ConfigFile<CountryConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var countryFiles = new List<ConfigFile<CountryConfig>>();

            string[] possibleTagPathes =
            {
                ModPathes.CountryTagsPath,
                GamePathes.CountryTagsPath
            };

            foreach (string tagPath in possibleTagPathes)
            {
                if (!Directory.Exists(tagPath)) continue;

                string[] tagFiles = Directory.GetFiles(tagPath, "*.txt", SearchOption.AllDirectories);
                foreach (string tagFilePath in tagFiles)
                {
                    string content = File.ReadAllText(tagFilePath);
                    HoiFuncFile tagFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    bool isOverride = tagPath.StartsWith(ModPathes.CountryTagsPath);

                    foreach (Var tagVar in tagFile.Vars)
                    {
                        if (tagVar.Name == "dynamic_tags" && tagVar.Value.ToBool())
                        {
                            // динамические теги пока пропускаем
                            continue;
                        }

                        string tag = tagVar.Name;
                        string countryFileRelative = tagVar.Value?.ToString() ?? string.Empty;

                        // В Parse
                        CountryConfig country = null;
                        string actualHistoryPath = FindCountryHistoryPath(tag, countryFileRelative, tagFilePath);
                        if (actualHistoryPath != null)
                        {
                            string historyContent = File.ReadAllText(actualHistoryPath);
                            HoiFuncFile historyFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(historyContent);

                            country = ParseCountryHistory(tag, countryFileRelative, actualHistoryPath, historyFile);
                        }

                        if (country != null)
                        {
                            var configFile = new ConfigFile<CountryConfig>
                            {
                                FileFullPath = actualHistoryPath, // теперь реальный путь к истории
                                IsOverride = isOverride,
                                Entities = { country }
                            };

                            countryFiles.Add(configFile);
                            Logger.AddDbgLog($"Добавлена страна: {tag} ({country.Id}) из {Path.GetFileName(actualHistoryPath)}");
                        }

                    }
                }
            }


            stopwatch.Stop();
            Logger.AddLog($"Парсинг стран завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Стран: {countryFiles.Sum(f => f.Entities.Count)} (файлов: {countryFiles.Count})");

            return countryFiles;
        }
        // Вынесенная функция поиска пути
        private static string FindCountryHistoryPath(string tag, string relativePath, string tagFilePath)
        {
            string[] possibleHistoryPaths =
            {
        Path.Combine(ModPathes.HistoryPath, relativePath.Replace("/", "\\")),
        Path.Combine(GamePathes.HistoryPath, relativePath.Replace("/", "\\")),
        Path.Combine(ModPathes.HistoryCountriesPath, relativePath.Replace("/", "\\")),
        Path.Combine(GamePathes.HistoryCountriesPath, relativePath.Replace("/", "\\"))
    };

            foreach (string candidate in possibleHistoryPaths)
            {
                if (File.Exists(candidate))
                    return candidate;
            }
            string[] filesInFolder = Enumerable.Empty<string>()
                .Concat(Directory.Exists(ModPathes.HistoryCountriesPath) && !string.IsNullOrEmpty(ModPathes.HistoryCountriesPath)
                    ? Directory.GetFiles(ModPathes.HistoryCountriesPath)
                    : Array.Empty<string>())
                .Concat(Directory.Exists(GamePathes.HistoryCountriesPath) && !string.IsNullOrEmpty(GamePathes.HistoryCountriesPath)
                    ? Directory.GetFiles(GamePathes.HistoryCountriesPath)
                    : Array.Empty<string>())
                .ToArray();


            return filesInFolder.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Contains(tag, StringComparison.OrdinalIgnoreCase));
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

            // add_core / set_owner / set_controller и т.д. — здесь только add_core как в оригинале
            var coreBracket = file.Brackets.FirstOrDefault(b => b.Name == "add_core");
            if (coreBracket != null)
            {
                foreach (Var v in coreBracket.SubVars)
                {
                    if (int.TryParse(v.Name, out int stateId) &&
                        int.TryParse(v.Value?.ToString(), out int isCore))
                    {
                        var state = ModDataStorage.Mod.Map.States.FileEntitiesToList().FindById(stateId.ToString());
                        if (state != null)
                            country.StateCores[state] = isCore != 0;
                    }
                }
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