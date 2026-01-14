
using Application.Extentions;
using Application.utils.Pathes;
using Models.Configs;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Drawing;

namespace Application.Composers
{
    public class CountryComposer : IComposer
    {
        public CountryComposer() { }
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new();
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
                    HoiFuncFile tagFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(file);
                    foreach (var var in tagFile.Vars)
                    {
                        string tag = var.Name;
                        string countryFileName = var.Value?.ToString() ?? string.Empty;
                        CountryConfig countryConfig = ParseCountryConfig(tag, countryFileName);
                        countryConfig.FileFullPath = file;
                        if (countryConfig != null && !configs.Any(c => c.Id.ToString() == countryConfig.Id.ToString()))
                        {
                            configs.Add(countryConfig);
                        }
                    }
                }
            }
            return configs;
        }
        public static CountryConfig ParseCountryConfig(string tag, string path)
        {
            string[] possibleHistoryPaths =
            {
                Path.Combine(ModPathes.HistoryPath, path),
                Path.Combine(GamePathes.HistoryPath, path)
            };

            foreach (var fullpath in possibleHistoryPaths)
            {
                if (!File.Exists(fullpath))
                    continue;

                if (new TxtParser(new TxtPattern()).Parse(fullpath) is not HoiFuncFile file)
                    continue;

                var techs = new Dictionary<TechTreeItemConfig, int>();
                foreach (var var in file.Brackets.FindById("set_technology")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var?.Name == null || var.Name.Length < 4)
                        continue;

                    string techId = var.Name.Substring(4);
                    var techItem = ModDataStorage.Mod.TechTreeLedgers.GetTreeItem(techId);
                    if (techItem == null)
                        continue;

                    if (int.TryParse(var.Value?.ToString(), out int level))
                        techs.SumToKey(techItem, level);
                }

                var states = new List<StateConfig>();
                var countryFlags = new Dictionary<IdeologyConfig, Bitmap>();
                var partyPopularities = new Dictionary<IdeologyConfig, int>();
                foreach (var var in file.Brackets.FindById("set_popularities")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var?.Name == null)
                        continue;

                    var ideology = ModDataStorage.Mod.Ideologies.FirstOrDefault(i => i.Id.ToString() == var.Name);
                    if (ideology != null && int.TryParse(var.Value?.ToString(), out int popularity))
                        partyPopularities[ideology] = popularity;
                }

                var ideas = new List<IdeaConfig>();
                foreach (var var in file.Brackets.FindById("add_ideas")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var?.Name == null)
                        continue;

                    var idea = ModDataStorage.Mod.Ideas.FindById(var.Name);
                    if (idea != null)
                        ideas.Add(idea);
                }

                var characters = new List<CountryCharacterConfig>();
                foreach (var var in file.Brackets.FindById("add_character")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var?.Name == null)
                        continue;

                    var character = ModDataStorage.Mod.Characters.FindById(var.Name);
                    if (character != null)
                        characters.Add(character);
                }

                var stateCores = new Dictionary<StateConfig, bool>();
                foreach (var var in file.Brackets.FindById("add_core")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    if (var?.Name == null || !int.TryParse(var.Value?.ToString(), out int isCore))
                        continue;

                    if (int.TryParse(var.Name, out int stateId))
                    {
                        var state = ModDataStorage.Mod.Map.States.FirstOrDefault(s => s.Id.ToInt() == stateId);
                        if (state != null)
                            stateCores[state] = isCore != 0;
                    }
                }

                int convoys = int.TryParse(file.Vars.FindById("set_convoys")?.Value?.ToString(), out int resultConvoys) ? resultConvoys : 0;
                double stab = double.TryParse(file.Vars.FindById("set_stability")?.Value?.ToString(), out double resultStab) ? resultStab : 0;
                double ws = double.TryParse(file.Vars.FindById("set_war_support")?.Value?.ToString(), out double resultWs) ? resultWs : 0;
                int resSlots = int.TryParse(file.Vars.FindById("set_research_slots")?.Value?.ToString(), out int resultSlots) ? resultSlots : 1;

                IdeologyConfig rulingParty = null;
                DateOnly? lastElection = null;
                int? electionFrequency = null;
                bool? electionsAllowed = null;
                foreach (var politicsvar in file.Brackets.FindById("set_politics")?.SubVars ?? Enumerable.Empty<Var>())
                {
                    switch (politicsvar?.Name)
                    {
                        case "ruling_party":
                            rulingParty = ModDataStorage.Mod.Ideologies.FirstOrDefault(i => i.Id.ToString() == politicsvar.Value?.ToString());
                            break;
                        case "last_election":
                            if (DateOnly.TryParse(politicsvar.Value?.ToString(), out DateOnly dt))
                                lastElection = dt;
                            break;
                        case "election_frequency":
                            if (int.TryParse(politicsvar.Value?.ToString(), out int ef))
                                electionFrequency = ef;
                            break;
                        case "elections_allowed":
                            if (int.TryParse(politicsvar.Value?.ToString(), out int ea))
                                electionsAllowed = ea != 0;
                            break;
                    }
                }

                var capitalVar = file.Vars.FindById("capital");
                int capital = capitalVar != null && int.TryParse(capitalVar.Value?.ToString(), out int cap) ? cap : -1;

                var oobVar = file.Vars.FindById("oob");
                string oob = oobVar?.Value?.ToString() ?? string.Empty;

                return new CountryConfig
                {
                    Id = new Identifier(tag),
                    Capital = capital,
                    CountryFileName = Path.GetFileName(path),
                    Technologies = techs,
                    Convoys = convoys,
                    OOB = oob,
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
                    StateCores = stateCores
                };
            }

            return null;
        }
    }
}
