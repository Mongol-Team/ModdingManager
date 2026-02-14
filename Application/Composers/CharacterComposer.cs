using Application.Debugging;
using Application.Extensions;
using Application.extentions;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using Data.Data;
using Models.Configs;
using Models.EntityFiles;
using Models.Enums;
using Models.GfxTypes;
using Models.Interfaces;
using Models.SubModels;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Diagnostics;
using System.Linq;

namespace Application.Composers
{
    public class CharacterComposer
    {
        /// <summary>
        /// Парсит все файлы персонажей и возвращает список файлов (ConfigFile<CountryCharacterConfig>)
        /// </summary>
        public static List<ConfigFile<CountryCharacterConfig>> Parse()
        {
            var stopwatch = Stopwatch.StartNew();
            var characterFiles = new List<ConfigFile<CountryCharacterConfig>>();

            string[] possiblePathes =
            {
                ModPathes.CommonCharacterPath,
                GamePathes.TraitsPath   // возможно здесь должно быть GamePathes.CommonCharacterPath или CharactersPath — проверь
            };

            foreach (string path in possiblePathes)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string fileContent = File.ReadAllText(file);
                    HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);

                    bool isOverride = path.StartsWith(ModPathes.CommonCharacterPath); // или более точная логика

                    var configFile = ParseFile(hoiFuncFile, file, isOverride);

                    if (configFile.Entities.Any())
                    {
                        characterFiles.Add(configFile);
                        Logger.AddDbgLog($"Добавлен файл персонажей: {configFile.FileName} ({configFile.Entities.Count} шт.)");
                    }
                }
            }


            stopwatch.Stop();
            Logger.AddLog($"Парсинг персонажей завершён. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {characterFiles.Count}, персонажей: {characterFiles.Sum(f => f.Entities.Count)}");

            return characterFiles;
        }

        private static ConfigFile<CountryCharacterConfig> ParseFile(HoiFuncFile funcFile, string fileFullPath, bool isOverride)
        {
            var configFile = new ConfigFile<CountryCharacterConfig>
            {
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            foreach (var topBracket in funcFile.Brackets.Where(b => b.Name == "characters"))
            {
                foreach (var charBr in topBracket.SubBrackets)
                {
                    var cfg = ParseCharacter(charBr, fileFullPath, isOverride);
                    if (cfg != null)
                    {
                        configFile.Entities.Add(cfg);
                        Logger.AddDbgLog($"  → добавлен персонаж: {cfg.Id}");
                    }
                }
            }

            return configFile;
        }

        private static CountryCharacterConfig ParseCharacter(Bracket charBr, string fileFullPath, bool isOverride)
        {
            var cfg = new CountryCharacterConfig
            {
                Id = new Identifier(charBr.Name),
                FileFullPath = fileFullPath,
                IsOverride = isOverride
            };

            string nameKey = string.Empty;
            string descKey = string.Empty;

            foreach (var v in charBr.SubVars)
            {
                switch (v.Name)
                {
                    case "name": nameKey = v.Value.ToString(); break;
                    case "desc": descKey = v.Value.ToString(); break;
                }
            }

            // TODO: реализовать instance branches (см. https://hoi4.paradoxwikis.com/Character_modding#Instances)

            foreach (var br in charBr.SubBrackets)
            {
                switch (br.Name)
                {
                    case "portraits":
                        cfg.Gfx = ParsePortraits(br);
                        break;

                    case "country_leader":
                        var leader = CreateCharacterType<CountryLeaderCharacterType>("country_leader", br);
                        string ideologyKey = br.SubVars.FirstOrDefault(v => v.Name == "ideology")?.Value?.ToString();
                        leader.Ideology = ModDataStorage.Mod.Ideologies.FileEntitiesToList().SelectMany(t => t.SubTypes).FirstOrDefault(t => t.Id.ToString() == ideologyKey);
                        cfg.Types.AddSafe(leader);
                        break;

                    case "advisor":
                        var advisor = CreateCharacterType<AdvisorCharacterType>("advisor", br);
                        ParseAdvisorData(br, advisor);
                        cfg.Types.AddSafe(advisor);
                        break;

                    case "corps_commander":
                        var corps = CreateCharacterType<CorpsCommanderCharacterType>("corps_commander", br);
                        var corpsData = ParseCommanderData(br);
                        ApplyCommanderData(corps, corpsData);   // ← используем extension-метод ниже
                        cfg.Types.AddSafe(corps);
                        break;

                    case "field_marshal":
                        var marshal = CreateCharacterType<FieldMarshalCharacterType>("field_marshal", br);
                        var marshalData = ParseCommanderData(br);
                        ApplyCommanderData(marshal, marshalData);
                        cfg.Types.AddSafe(marshal);
                        break;

                    case "navy_leader":
                        var naval = CreateCharacterType<NavalLeaderCharacterType>("navy_leader", br);
                        var navalData = ParseCommanderData(br);
                        ApplyCommanderData(naval, navalData);
                        cfg.Types.AddSafe(naval);
                        break;
                }
            }

            cfg.Localisation = new ConfigLocalisation
            {
                Language = ModManagerSettings.CurrentLanguage,
                Source = cfg
            };
            cfg.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(nameKey));
            cfg.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(descKey));

            return cfg;
        }


        private static void ApplyCommanderData(ICharacterType commander, CharaterCommanderData data)
        {
            if (commander is CorpsCommanderCharacterType corps)
            {
                corps.Skill = data.Skill;
                corps.Attack = data.Attack;
                corps.Defense = data.Defense;
                corps.Supply = data.Supply;
                corps.Planning = data.Planning;
            }
            else if (commander is FieldMarshalCharacterType marshal)
            {
                marshal.Skill = data.Skill;
                marshal.Attack = data.Attack;
                marshal.Defense = data.Defense;
                marshal.Supply = data.Supply;
                marshal.Planning = data.Planning;
            }
            else if (commander is NavalLeaderCharacterType naval)
            {
                naval.Skill = data.Skill;
                naval.Attack = data.Attack;
                naval.Defense = data.Defense;
                naval.Maneuver = data.Maneuvering;
                naval.Coordination = data.Coordination;
            }
        }

        private static T CreateCharacterType<T>(string typeName, Bracket br)
            where T : ICharacterType, new()
        {
            var common = ParseCommonCharacterData(br);
            var baseId = new Identifier(br.Name);

            return new T
            {
                Id = new Identifier(baseId + GetTraitPostfix(typeName)),
                Expire = common.Expire,
                Traits = common.Traits,
                Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null),
                Allowed = common.Allowed,
                Available = common.Available,
                Visible = common.Visible,
                Localisation = new ConfigLocalisation()
            };
        }

        private static string GetTraitPostfix(string typeName) => typeName switch
        {
            "country_leader" => ClassStaticValues.CountryLeaderTraitPostfix,
            "advisor" => ClassStaticValues.AdvisorTraitPostfix,
            "corps_commander" => ClassStaticValues.CorpseTraitPostfix,
            "field_marshal" => ClassStaticValues.FieldTraitPostfix,
            "navy_leader" => ClassStaticValues.NavyLeaderTraitPostfix,
            _ => throw new NotSupportedException($"Неизвестный тип персонажа: {typeName}")
        };

        private static IGfx ParsePortraits(Bracket portraitsBr)
        {
            foreach (var portBr in portraitsBr.SubBrackets)
            {
                if (portBr.Name is "army" or "civilian")
                {
                    var largeVar = portBr.SubVars.FirstOrDefault(v => v.Name == "large");
                    if (largeVar != null)
                    {
                        var file = ModDataStorage.Mod.Gfxes
                            .FirstOrDefault(f => f.Entities.Any(e => e.Id.ToString() == largeVar.Value.ToString()));
                        return file?.FindById(largeVar.Value.ToString());
                    }
                    // можно добавить обработку small, если нужно
                }
            }
            return null;
        }

        private static void ParseAdvisorData(Bracket br, AdvisorCharacterType advisor)
        {
            foreach (var v in br.SubVars)
            {
                switch (v.Name)
                {
                    case "slot":
                        advisor.Slot = ModDataStorage.Mod.IdeaSlots.SearchConfigInFile(v.Value?.ToString());
                        break;
                    case "idea_token":
                        advisor.Idea = ModDataStorage.Mod.Ideas.SearchConfigInFile(v.Value?.ToString());
                        break;
                    case "cost":
                        advisor.AdvisorCost = v.Value.ToInt();
                        break;
                    case "ledger":
                        advisor.IdeaLedgerType = v.Value.ToString().SnakeToPascal().ToEnum(IdeaLedgerType.All);
                        break;
                }
            }
        }

     

        private static CharacterCommonData ParseCommonCharacterData(Bracket br)
        {
            var data = new CharacterCommonData();

            foreach (var v in br.SubVars)
            {
                if (v.Name == "expire")
                    data.Expire = v.Value.ToString();
            }

            foreach (var arr in br.Arrays)
            {
                if (arr.Name == "traits")
                {
                    var traitIds = new HashSet<string>(arr.Values.ToListString());
                    data.Traits = ModDataStorage.Mod.CharacterTraits
                        .SelectMany(f => f.Entities)
                        .Where(t => traitIds.Contains(t.Id.ToString()))
                        .ToList();
                }
            }

            foreach (var sbr in br.SubBrackets)
            {
                switch (sbr.Name)
                {
                    case "visible": data.Visible = sbr.ToString(); break;
                    case "available": data.Available = sbr.ToString(); break;
                    case "allowed": data.Allowed = sbr.ToString(); break;
                }
            }

            return data;
        }

        private static CharaterCommanderData ParseCommanderData(Bracket br)
        {
            var data = new CharaterCommanderData();

            foreach (var v in br.SubVars)
            {
                switch (v.Name)
                {
                    case "skill": data.Skill = v.Value.ToInt(); break;
                    case "attack_skill": data.Attack = v.Value.ToInt(); break;
                    case "defense_skill": data.Defense = v.Value.ToInt(); break;
                    case "logistics_skill": data.Supply = v.Value.ToInt(); break;
                    case "planning_skill": data.Planning = v.Value.ToInt(); break;
                    case "maneuvering_skill": data.Maneuvering = v.Value.ToInt(); break;
                    case "coordination_skill": data.Coordination = v.Value.ToInt(); break;
                }
            }

            return data;
        }

        // Вспомогательные классы (лучше вынести в отдельные файлы позже)
        private class CharaterCommanderData
        {
            public int Skill { get; set; } = 1;
            public int Attack { get; set; } = 1;
            public int Defense { get; set; } = 1;
            public int Supply { get; set; } = 1;
            public int Planning { get; set; } = 1;
            public int Maneuvering { get; set; } = 1;
            public int Coordination { get; set; } = 1;
        }

        private class CharacterCommonData
        {
            public string Expire { get; set; } = DataDefaultValues.Null;
            public List<CharacterTraitConfig> Traits { get; set; } = new();
            public string Allowed { get; set; } = DataDefaultValues.Null;
            public string Available { get; set; } = DataDefaultValues.Null;
            public string Visible { get; set; } = DataDefaultValues.Null;
        }
    }
}