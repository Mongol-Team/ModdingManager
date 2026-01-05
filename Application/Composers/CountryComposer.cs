
using Application.Extentions;
using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using System.Drawing;
using static OpenCvSharp.FileStorage;
using Models.Configs;

namespace Application.Composers
{
    public class CountryComposer : IComposer
    {
        public CountryComposer() { }
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possibleHistoryPathes =
            {
                ModPathes.HistoryCountriesPath,
                GamePathes.HistoryCountriesPath
            };
            string[] possibleTagPathes =
            {
                ModPathes.CountryTagsPath,
                GamePathes.CountryTagsPath
            };
            foreach (string path in possibleTagPathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        string tag = line.Split('=')[0].Trim();
                        string countryFilePath = line.Split('=')[1].Trim();
                        CountryConfig countryConfig = ParseCountryConfig(tag, countryFilePath);
                        configs.Add(countryConfig);
                    }
                }
            }
            return configs;
        }
        public static CountryConfig ParseCountryConfig(string tag, string path)
        {
            string[] possibleHistoryPaths = {
               Path.Combine(ModPathes.RootPath, path),
               Path.Combine(GamePathes.RootPath, path)
            };
            string[] possibleCommonPaths = {
               Path.Combine(ModPathes.RootPath, path),
               Path.Combine(GamePathes.RootPath, path)
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
                        var techItem = ModDataStorage.Mod.TechTreeLedgers.GetTreeItem(techId);
                        if (techItem != null && int.TryParse(var.Value.ToString(), out int level))
                        {
                            techs[techItem] = level;
                        }
                    }
                }
                var states = new List<StateConfig>();
                foreach (var state in techs.Values)
                {
                    var stateConfig = ModDataStorage.Mod.Map.States.FirstOrDefault(s => s.Id.ToInt() == state);
                    if (stateConfig != null)
                        states.Add(stateConfig);
                }

                var countryFlags = new Dictionary<IdeologyConfig, Bitmap>();

                //fima
                var partyPopularities = new Dictionary<IdeologyConfig, int>();
                foreach (var var in file.Brackets.FindById("set_popularities").SubVars)
                {
                    if (var != null)
                    {
                        string ideologyId = var.Name;
                        var ideology = ModDataStorage.Mod.Ideologies.FirstOrDefault(i => i.Id.ToString() == ideologyId);
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
                        ideas.Add(ModDataStorage.Mod.Ideas.FindById(var.Name));
                    }
                }
                var characters = new List<CountryCharacterConfig>();
                foreach (var var in file.Brackets.FindById("add_character")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var != null)
                    {
                        characters.Add(ModDataStorage.Mod.Characters.FindById(var.Name));
                    }
                }
                var stateCores = new Dictionary<StateConfig, bool>();
                foreach (var var in file.Brackets.FindById("add_core")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var != null && int.TryParse(var.Value.ToString(), out int isCore))
                    {
                        var state = ModDataStorage.Mod.Map.States.FirstOrDefault(s => s.Id.ToInt() == int.Parse(var.Name));
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
                            rulingParty = ModDataStorage.Mod.Ideologies.FirstOrDefault(i => i.Id.ToString() == ideologyId);
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
