using Application.Debugging;
using Application.Extensions;
using Application.extentions;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Models.Configs;
using Models.EntityFiles;
using Models.GfxTypes;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DDF = Data.DataDefaultValues;

namespace Application.Composers
{
    internal class IdeaComposer
    {
        /// <summary>
        /// Парсит все файлы идей и возвращает список файлов (ConfigFile<IdeaConfig>)
        /// </summary>
        public static List<ConfigFile<IdeaConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var ideaFiles = new List<ConfigFile<IdeaConfig>>();

            string[] possiblePathes =
            {
                ModPathes.IdeasPath,
                GamePathes.IdeasPath
            };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(content);

                    bool isOverride = path.StartsWith(ModPathes.IdeasPath);

                    var configFile = ParseFile(hoiFuncFile, file, isOverride);

                    if (configFile.Entities.Any())
                    {
                        ideaFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл идей: {configFile.FileName} → {configFile.Entities.Count} идей");
                    }
                }
            }

            stopwatch.Stop();
            Logger.AddLog($"Парсинг идей завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {ideaFiles.Count}, идей всего: {ideaFiles.Sum(f => f.Entities.Count)}");

            return ideaFiles;
        }

        private static ConfigFile<IdeaConfig> ParseFile(HoiFuncFile file, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<IdeaConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (Bracket ideaBrTop in file.Brackets.Where(b => b.Name == "ideas"))
            {
                // Обычно структура: ideas → slot → idea
                foreach (Bracket slotBr in ideaBrTop.SubBrackets)
                {
                    foreach (Bracket ideaBr in slotBr.SubBrackets)
                    {
                        var idea = ParseSingleIdea(ideaBr, fileFullPath, isOverride);
                        if (idea != null)
                        {
                            configFile.Entities.Add(idea);
                            Logger.AddDbgLog($"  → добавлена идея: {idea.Id}");
                        }
                    }
                }
            }

            return configFile;
        }

        private static IdeaConfig ParseSingleIdea(Bracket ideaBr, string fileFullPath, bool isOverride)
        {
            var idea = new IdeaConfig
            {
                Id = new Identifier(ideaBr.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride,

                Localisation = new ConfigLocalisation { Language = ModManagerSettings.CurrentLanguage },
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                Gfx = new SpriteType(DDF.NullImageSource, DDF.Null),

                Tag = DDF.Null,
                RemovalCost = DDF.NullInt,
                Available = DDF.Null,
                AvailableCivilWar = DDF.Null,
                Allowed = DDF.Null,
                AllowedToRemove = DDF.Null,
                Cost = DDF.NullInt,
                OnAdd = DDF.Null
            };

            // Переменные верхнего уровня
            foreach (Var v in ideaBr.SubVars)
            {
                switch (v.Name)
                {
                    case "picture":
                        idea.PictureName = v.Value?.ToString();
                        break;

                    case "cost":
                        idea.Cost = v.Value.ToInt();
                        break;

                    case "removal_cost":
                        idea.RemovalCost = v.Value.ToInt();
                        break;

                    case "on_add":
                        idea.OnAdd = v.Value?.ToString();
                        break;
                }
            }

            // Подблоки
            foreach (Bracket br in ideaBr.SubBrackets)
            {
                switch (br.Name)
                {
                    case "allowed":
                        idea.Allowed = br.ToString(); // todo: триггеры
                        break;

                    case "allowed_to_remove":
                        idea.AllowedToRemove = br.ToString(); // todo: триггеры
                        break;

                    case "available":
                        idea.Available = br.ToString(); // todo: триггеры
                        break;

                    case "available_civil_war":
                        idea.AvailableCivilWar = br.ToString(); // todo: триггеры
                        break;

                    case "modifier":
                        foreach (Var v in br.SubVars)
                        {
                            var modDef = ModDataStorage.Mod.ModifierDefinitions.SearchConfigInFile(v.Name);
                            if (modDef != null)
                            {
                                idea.Modifiers.TryAdd(modDef, v.Value);
                                Logger.AddDbgLog($"Идея {idea.Id} → добавлен модификатор {modDef.Id}");
                            }
                        }
                        break;
                }
            }

            // Картинка по умолчанию
            idea.PictureName ??= $"GFX_idea_{idea.Id}";

            // Локализация (имя и описание обычно по одному ключу)
            var nameLoc = ModDataStorage.Localisation.GetLocalisationByKey(idea.Id.ToString());
            idea.Localisation.Data.AddPair(nameLoc);

            var descLoc = ModDataStorage.Localisation.GetLocalisationByKey(idea.Id.ToString() + "_desc");
            if (!string.IsNullOrEmpty(descLoc.Value))
                idea.Localisation.Data.AddPair(descLoc);

            // Графика
            string gfxKey1 = $"GFX_idea_{idea.PictureName}";
            string gfxKey2 = $"GFX_idea_{idea.Id}";

            idea.Gfx = ModDataStorage.Mod.Gfxes.SearchConfigInFile(gfxKey1) ?? ModDataStorage.Mod.Gfxes.SearchConfigInFile(gfxKey2) ?? new SpriteType(DDF.NullImageSource, DDF.Null);

            return idea;
        }

       
    }
}