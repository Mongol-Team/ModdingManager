using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.Settings;
using ModdingManagerClassLib.utils;
using ModdingManagerClassLib.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.GfxTypes;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDF = ModdingManagerData.DataDefaultValues;



namespace ModdingManagerClassLib.Composers
{
    public class TechnologyComposer : IComposer
    {
        public TechnologyComposer() { }
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.TechTreePath,
                GamePathes.TechTreePath
            };
            string[] possibleDefPathes =
            {
                ModPathes.TechDefPath,
                GamePathes.TechDefPath
            };
            HashSet<string> seenDefIds = new();

            foreach (string path in possibleDefPathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    HoiFuncFile funcFile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                    if (funcFile == null) continue;

                    foreach (Bracket defbr in funcFile.Brackets.Where(b => b.Name == "technology_folders"))
                    {
                        TechTreeConfig tree = ParseTechnologyConfig(defbr);
                        if (tree == null) continue;

                        string id = tree.Id.ToString();
                        if (seenDefIds.Contains(id)) continue;

                        seenDefIds.Add(id);
                        configs.Add(tree);
                    }
                }
            }

            return configs;
        }
        public static TechTreeConfig ParseTechnologyConfig(Bracket bracket)
        {
            TechTreeConfig config = new TechTreeConfig();
            config.Id = new Identifier(bracket.Name);
            Var leger = bracket.SubVars.FirstOrDefault(v => v.Name == "ledger");
            if (leger != null)
            {
                config.Ledger = (TechTreeLedgerType)Enum.Parse(typeof(TechTreeLedgerType), leger.Value.ToString());

            }
            else
            {
                config.Ledger = TechTreeLedgerType.Null;
            }
            Bracket allowed = bracket.SubBrackets.FirstOrDefault(v => v.Name == "available");
            if (allowed != null)
            {
                config.Available = allowed.ToString();
            }
            else
            {
                config.Available = DDF.Null;
            }
            IGfx gfx = ModDataStorage.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == $"GFX_{config.Id.ToString()}_tab");
            if (gfx != null)
            {
                config.Gfx = gfx;
            }
            else
            {
                config.Gfx = new SpriteType(DDF.NullImageSource, DDF.Null);
            }
            KeyValuePair<string, string> nameloc = ModDataStorage.Localisation.GetLocalisationByKey(config.Id.ToString());
            KeyValuePair<string, string> descloc = ModDataStorage.Localisation.GetLocalisationByKey(config.Id.ToString() + "_desc");
            if (nameloc.Equals(DDF.NullLocalistion) && descloc.Equals(DDF.NullLocalistion))
            {
                config.Localisation.IsConfigLocNull = true;
            }
            config.Localisation.Data.AddPair(nameloc);
            config.Localisation.Data.AddPair(descloc);

            return config;
        }
        
    }
}
