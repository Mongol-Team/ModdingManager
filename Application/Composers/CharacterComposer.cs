using Microsoft.Win32.SafeHandles;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Data;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models.Enums;
using Models.GfxTypes;
using Models.Interfaces;
using Models.SubModels;
using Models.Types.LocalizationData;
using Models.Types.ObectCacheData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using Pfim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Configs;

namespace Application.Composers
{
    public class CharacterComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.CommonCharacterPath,
                GamePathes.TraitsPath
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
                        List<CountryCharacterConfig> charConfigs = ParseFile(hoiFuncFile).Cast<CountryCharacterConfig>().ToList();
                        foreach (CountryCharacterConfig charConfig in charConfigs)
                        {
                            if (!configs.Any(c => c.Id == charConfig.Id))
                            {
                                configs.Add(charConfig);
                            }
                        }
                    }
                }
            }
            return configs;
        }

        public static IEnumerable<IConfig> ParseFile(HoiFuncFile funcFile)
        {
            List<CountryCharacterConfig> configs = new List<CountryCharacterConfig>();
            foreach (Bracket bracket in funcFile.Brackets.Where(b => b.Name == "characters"))
            {
                foreach (Bracket br in bracket.SubBrackets)
                {
                    CountryCharacterConfig cfg = (CountryCharacterConfig)ParseObject(br);
                    configs.AddSafe(cfg);
                }
            }
           
            return configs;
        }

        public static IConfig ParseObject(Bracket charBr)
        {
            CountryCharacterConfig cfg = new CountryCharacterConfig();
            cfg.Id = new Identifier(charBr.Name);
            string nameKey = string.Empty;
            string descKey = string.Empty;
            foreach (Var v in charBr.SubVars)
            {
                switch (v.Name)
                {
                    case "name":
                        nameKey = v.Value.ToString();
                        break;
                    case "desc":
                        descKey = v.Value.ToString();
                        break;
                }
            }
            //todo: instance branches implementation https://hoi4.paradoxwikis.com/Character_modding#Instances
            foreach (Bracket br in charBr.SubBrackets)
            {
                switch (br.Name)
                {
                    case "portraits":
                        foreach(Bracket portBr in br.SubBrackets)
                        { 
                            if (portBr.Name == "army" || portBr.Name == "civilian")
                            {
                                foreach (Var v in br.SubVars)
                                {
                                    if (v.Name == "large")
                                    {
                                        IGfx gfx = ModDataStorage.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == v.Value.ToString());
                                    }
                                    else if (v.Name == "small")
                                    {
                                        IGfx small = ModDataStorage.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == v.Value.ToString());
                                    }
                                }
                            }
                            else
                            {
                                //todo: exception for unknown portrait type
                            }
                        }
                        break;
                    case "country_leader":
                        CountryLeaderCharacterType cleder = (CountryLeaderCharacterType)CreateCharacterType("country_leader", br);

                        cleder.Ideology  = ModDataStorage.Mod.Ideologies
                         .SelectMany(i => i.SubTypes)
                         .FirstOrDefault(sub => sub.Name == br.SubVars.FirstOrDefault(v => v.Name == "ideology")?.Value);
                        cfg.Types.AddSafe(cleder);
                        break;
                    case "advisor":
                        AdvisorCharacterType advisor = (AdvisorCharacterType)CreateCharacterType("advisor", br);
                        foreach(Var advVar in br.SubVars)
                        {
                            switch(advVar.Name)
                            {
                                case "slot":
                                    advisor.Slot = ModDataStorage.Mod.IdeaSlots.FirstOrDefault(s => s.Id.ToString() == advVar.Value.ToString());
                                    break;
                                case "idea_token":
                                    advisor.Idea = ModDataStorage.Mod.Ideas.FirstOrDefault(i => i.Id.ToString() == advVar.Value.ToString());
                                    break;
                                case "cost":
                                    if (int.TryParse(advVar.Value.ToString(), out int cost))
                                    {
                                        advisor.AdvisorCost = cost;
                                    }
                                    break;
                                case "ledger":
                                    advisor.IdeaLedgerType = Enum.TryParse<IdeaLedgerType>(advVar.Value.ToString(), out var ledgerType) ? ledgerType : IdeaLedgerType.All;
                                    break;
                            }
                        }
                        cfg.Types.AddSafe(advisor);
                        break;
                    case "corps_commander":
                        CorpsCommanderCharacterType corps = (CorpsCommanderCharacterType)CreateCharacterType("corps_commander", br);
                        CharaterCommanderData corpseData = ParseCommanderData(br);
                        corps.Skill = corpseData.Skill;
                        corps.Attack = corpseData.Attack;
                        corps.Defense = corpseData.Defense;
                        corps.Supply = corpseData.Supply;
                        corps.Planning = corpseData.Planning;
                        cfg.Types.AddSafe(corps);
                        break;
                    case "field_marshal":
                        FieldMarshalCharacterType field = (FieldMarshalCharacterType)CreateCharacterType("field_marshal", br);
                        CharaterCommanderData fieldData = ParseCommanderData(br);
                        field.Skill = fieldData.Skill;
                        field.Attack = fieldData.Attack;
                        field.Defense = fieldData.Defense;
                        field.Supply = fieldData.Supply;
                        field.Planning = fieldData.Planning;
                        cfg.Types.AddSafe(field);
                        break;
                    case "navy_leader":
                        NavalLeaderCharacterType naval = (NavalLeaderCharacterType)CreateCharacterType("navy_leader", br);
                        CharaterCommanderData navalData = ParseCommanderData(br);
                        naval.Skill = navalData.Skill;
                        naval.Attack = navalData.Attack;
                        naval.Defense = navalData.Defense;
                        naval.Maneuver = navalData.Maneuvering;
                        naval.Coordination = navalData.Coordination;
                        cfg.Types.AddSafe(naval);
                        break;

                }

            }
            cfg.Localisation = new ConfigLocalisation()
            {
                Language = ModManagerSettings.CurrentLanguage,
                Source = cfg,
                Data = new Dictionary<string, string>()
            };
            cfg.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(nameKey));
            cfg.Localisation.Data.AddPair(ModDataStorage.Localisation.GetLocalisationByKey(descKey));
            return cfg;
        }

        private static ICharacterType CreateCharacterType(string typeName, Bracket br)
        {
            var common = ParseCommonCharacterData(br);
            Identifier baseId = new Identifier(br.Name);
            switch (typeName)
            {
                case "country_leader":
                  
                    return new CountryLeaderCharacterType
                    {
                        Id = new Identifier(baseId.ToString() + ClassStaticValues.CountryLeaderTraitPostfix),
                        Expire = common.Expire,
                        Traits = common.Traits,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null),
                        Allowed = common.Allowed,
                        Available = common.Available,
                        Visible = common.Visible,
                        Localisation = new()
                    };

                case "advisor":
                    return new AdvisorCharacterType
                    {
                        Id = new Identifier(baseId.ToString() + ClassStaticValues.AdvisorTraitPostfix),
                        Expire = common.Expire,
                        Traits = common.Traits,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null),
                        Allowed = common.Allowed,
                        Available = common.Available,
                        Visible = common.Visible,
                         Localisation = new()
                    };
                case "corps_commander":
                    return new CorpsCommanderCharacterType
                    {
                        Id = new Identifier(baseId.ToString() + ClassStaticValues.CorpseTraitPostfix),
                        Expire = common.Expire,
                        Traits = common.Traits,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null),
                        Allowed = common.Allowed,
                        Available = common.Available,
                        Visible = common.Visible,
                        Localisation = new()
                    };
                case "field_marshal":
                    return new FieldMarshalCharacterType
                    {
                        Id = new Identifier(baseId.ToString() + ClassStaticValues.FieldTraitPostfix),
                        Expire = common.Expire,
                        Traits = common.Traits,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null),
                        Allowed = common.Allowed,
                        Available = common.Available,
                        Visible = common.Visible,
                        Localisation = new()
                    };
                case "navy_leader":
                    return new NavalLeaderCharacterType
                    {
                        Id = new Identifier(baseId.ToString() + ClassStaticValues.NavyLeaderTraitPostfix),
                        Expire = common.Expire,
                        Traits = common.Traits,
                        Gfx = new SpriteType(DataDefaultValues.ItemWithNoGfxImage, DataDefaultValues.Null),
                        Allowed = common.Allowed,
                        Available = common.Available,
                        Visible = common.Visible,
                        Localisation = new()
                    };
                default:
                    throw new NotSupportedException($"Unknown character type: {typeName}");
            }
        }
        private static CharacterCommonData ParseCommonCharacterData(Bracket br)
        {
            string expire = DataDefaultValues.Null;
            string allowed = DataDefaultValues.Null;
            string available = DataDefaultValues.Null;
            string visible = DataDefaultValues.Null;
            List<CharacterTraitConfig> traits = new List<CharacterTraitConfig>();

            foreach (Var v in br.SubVars)
            {
                if (v.Name == "expire")
                    expire = v.Value.ToString();
            }

            foreach (HoiArray arr in br.Arrays)
            {
                if (arr.Name == "traits")
                {
                    var idSet = new HashSet<string>(arr.Values.ToListString());
                    traits = ModDataStorage.Mod.CharacterTraits
                        .Where(c => idSet.Contains(c.Id.ToString()))
                        .ToList();
                }
            }

            foreach (Bracket sbr in br.SubBrackets)
            {
                switch (sbr.Name)
                {
                    case "visible": visible = sbr.ToString(); break;
                    case "available": available = sbr.ToString(); break;
                    case "allowed": allowed = sbr.ToString(); break;
                }
            }

            return new CharacterCommonData
            {
                Expire = expire,
                Traits = traits,
                Allowed = allowed,
                Available = available,
                Visible = visible
            };
        }
        private static CharaterCommanderData ParseCommanderData(Bracket br)
        {
            int skill = 1;
            int attack = 1;
            int defense = 1;
            int supply = 1;
            int planning = 1;
            int maneuvering = 1;
            int coordination = 1;
            foreach (Var v in br.SubVars)
            {
                switch (v.Name)
                {
                    case "skill":
                        int.TryParse(v.Value.ToString(), out skill);
                        break;
                    case "attack_skill":
                        int.TryParse(v.Value.ToString(), out attack);
                        break;
                    case "defense_skill":
                        int.TryParse(v.Value.ToString(), out defense);
                        break;
                    case "logistics_skill":
                        int.TryParse(v.Value.ToString(), out supply);
                        break;
                    case "planning_skill":
                        int.TryParse(v.Value.ToString(), out planning);
                        break;
                    case "maneuvering_skill":
                        int.TryParse(v.Value.ToString(), out maneuvering);
                        break;
                    case "coordination_skill":
                        int.TryParse(v.Value.ToString(), out coordination);
                        break;
                }
            }
            return new CharaterCommanderData
            {
                Skill = skill,
                Attack = attack,
                Defense = defense,
                Supply = supply,
                Planning = planning,
                Maneuvering = maneuvering,
                Coordination = coordination
            };
        }
        class CharaterCommanderData
        {
            public int Skill { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int Supply { get; set; }
            public int Planning { get; set; }
            public int Maneuvering { get; set; }
            public int Coordination { get; set; }
        }
        class CharacterCommonData
        {
            public string Expire { get; set; }
            public List<CharacterTraitConfig> Traits { get; set; }
            public string Allowed { get; set; }
            public string Available { get; set; }
            public string Visible { get; set; }
        }
    }
}
