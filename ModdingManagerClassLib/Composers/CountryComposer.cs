using ModdingManager.managers.@base;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;
using static OpenCvSharp.FileStorage;

namespace ModdingManagerClassLib.Composers
{
    public class CountryComposer : IComposer
    {
        public CountryComposer() { }
        public static List<IConfig> Parse()
        {
            throw new NotImplementedException();
        }
        public static CountryConfig ParseCountryConfig(string tag, string path)
        {
            string[] possibleHistoryPaths = {
               Path.Combine(ModPathes.HistoryCountriesPath, path),
               Path.Combine(GamePathes.HistoryCountriesPath, path)
            };
            string[] possibleCommonPaths = {
               Path.Combine(ModPathes.CommonCountriesPath, path),
               Path.Combine(GamePathes.CommonCountriesPath, path)
            };
            foreach (var fullpath in possibleHistoryPaths)
            {
                HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(fullpath) as HoiFuncFile;
                
                Dictionary<TechTreeItemConfig, int> techs = new Dictionary<TechTreeItemConfig, int>();
                foreach (var var in file.Brackets.FindById("set_technology").SubVars)
                {
                    if (var != null)
                    {
                        string techId = var.Name.Substring(4);
                        var techItem = ModManager.Mod.TechTreeLedgers.GetTreeItem(techId);
                        if (techItem != null && int.TryParse(var.Value.ToString(), out int level))
                        {
                            techs[techItem] = level;
                        }
                    }
                }
                var states = new List<StateConfig>();
                foreach (var state in techs.Values)
                {
                    var stateConfig = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id.AsInt() == state);
                    if (stateConfig != null)
                        states.Add(stateConfig);
                }
                
                var countryFlags = new Dictionary<string, Bitmap>();
                //fima
                var partyPopularities = new Dictionary<IdeologyConfig, int>();
                foreach (var var in file.Brackets.FindById("set_popularities").SubVars)
                {
                    if (var != null)
                    {
                        string ideologyId = var.Name;
                        var ideology = ModManager.Mod.Ideologies.FirstOrDefault(i => i.Id.AsString() == ideologyId);
                        if (ideology != null && int.TryParse(var.Value.ToString(), out int popularity))
                        {
                            partyPopularities[ideology] = popularity;
                        }
                    }
                }
                var ideas = new List<IdeaConfig>();
                foreach (var var in file.Brackets.FindById("add_ideas")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var != null)
                    {
                        ideas.Add(ModManager.Mod.Ideas.FindById(var.Name));
                    }
                }
                var characters = new List<CountryCharacterConfig>();
                foreach (var var in file.Brackets.FindById("add_character")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var != null)
                    {
                        characters.Add(ModManager.Mod.Characters.FindById(var.Name));
                    }
                }
                var stateCores = new Dictionary<StateConfig, bool>();
                foreach (var var in file.Brackets.FindById("add_core")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var != null && int.TryParse(var.Value.ToString(), out int isCore))
                    {
                        var state = ModManager.Mod.Map.States.FirstOrDefault(s => s.Id.AsInt() == int.Parse(var.Name));
                        if (state != null)
                            stateCores[state] = isCore != 0;
                    }
                }
                string convoyValueStr = file.Vars.FindById("set_convoys")?.Value?.ToString();
                int convoys = 0;
                if (int.TryParse(convoyValueStr, out int result))
                {
                    convoys = result;
                }
                string stabValueStr = file.Vars.FindById("set_stability")?.Value?.ToString();
                double stab = 0;
                if (double.TryParse(stabValueStr, out double resultS))
                {
                    stab = resultS;
                }
                string wsValueStr = file.Vars.FindById("set_war_support")?.Value?.ToString();
                double ws = 0;
                if (double.TryParse(stabValueStr, out double resultW))
                {
                    ws = resultW;
                }
                string resSlotsValueStr = file.Vars.FindById("set_convoys")?.Value?.ToString();
                int resSlots = 1;
                if (int.TryParse(convoyValueStr, out int resultR))
                {
                    resSlots = resultR;
                }
                IdeologyConfig rulingParty = null;
                DateOnly? lastElection = null;
                int? electionFrequency = null;
                bool? electionsAllowed = null;
                foreach (Var politicsvar in file.Brackets.FindById("set_politics").SubVars)
                {
                    switch (politicsvar.Name)
                    {
                        case "ruling_party":
                            string ideologyId = politicsvar.Value.ToString();
                            rulingParty = ModManager.Mod.Ideologies.FirstOrDefault(i => i.Id.AsString() == ideologyId);
                            break;
                        case "last_election":
                            if (DateOnly.TryParse(politicsvar.Value.ToString(), out DateOnly dt))
                                lastElection = dt;
                            break;
                        case "election_frequency":
                            if (int.TryParse(politicsvar.Value.ToString(), out int ef))
                                electionFrequency = ef;
                            break;
                        case "elections_allowed":
                            if (int.TryParse(politicsvar.Value.ToString(), out int ea))
                                electionsAllowed = ea != 0;
                            break;
                    }
                }
                CountryConfig countryConfig = new CountryConfig()
                {
                    Id = new Identifier(tag),
                    Capital = file.Vars.First(v => v.Name == "capital").Value as int? ?? -1,
                    CountryFileName = System.IO.Path.GetFileName(path),
                    //GraphicalCulture = null,
                    //Color = null,
                    Technologies = techs,
                    Convoys = convoys,
                    OOB = file.Vars.FindById("oob").Value.ToString(),
                    Stab = stab,
                    WarSup = ws,
                    ResearchSlots = resSlots,
                    RulingParty = rulingParty,
                    LastElection = lastElection,
                    ElectionFrequency = electionFrequency,
                    ElectionsAllowed = electionsAllowed,

                    States = states,
                    CountryFlags = countryFlags,
                    PartyPopularities = partyPopularities,
                    Ideas = ideas,
                    Characters = characters,
                    StateCores = stateCores,
                };



                return countryConfig;
            }
            throw new NotImplementedException();
        }
    }
}
