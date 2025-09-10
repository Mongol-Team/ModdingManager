using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class CountryComposer : IComposer
    {
        public CountryComposer() { }
        public static List<IConfig> Parse()
        {
            
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
                
                CountryConfig countryConfig = new CountryConfig
                {
                    Tag = tag,
                    Capital = file.Vars.First(v => v.Name == "capital").Value as int? ?? -1,
                    CountryFileName = System.IO.Path.GetFileName(path),
                    GraphicalCulture = null,
                    Color = null,
                    Technologies = new Dictionary<string, int>(),
                    Convoys = null,
                    OOB = null,
                    Stab = null,
                    WarSup = null,
                    ResearchSlots = null,
                    RulingParty = null,
                    LastElection = null,
                    ElectionFrequency = null,
                    ElectionsAllowed = null,

                    States = new List<StateConfig>(),
                    CountryFlags = new Dictionary<string, Bitmap>(),
                    PartyPopularities = new Dictionary<string, int>(),
                    Ideas = new List<string>(),
                    Characters = new List<string>(),
                    StateCores = new Dictionary<int, bool>(),
                };

                return countryConfig;
            }
        }
    }
}
