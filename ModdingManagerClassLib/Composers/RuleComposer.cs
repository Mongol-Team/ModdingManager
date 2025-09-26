using ModdingManager.managers.@base;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.GfxTypes;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModdingManagerClassLib.Composers
{
    public class RuleComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            string[] possiblePaths = {
                ModPathes.RulesPath,
                GamePathes.RulesPath
            };
            List<IConfig> configs = new List<IConfig>();
            foreach (string path in possiblePaths)
            {
                List<string> files = null;
                try
                {
                    files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories).ToList();
                }
                catch (Exception ex)
                {
                    // Можно логировать ex.Message при необходимости
                    files = new List<string>(); // или files = null;
                }
                foreach (string file in files)
                {
                    var funcfile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                    foreach (var br in funcfile.Brackets)
                    {
                        var cfg = ParseSingleRule(br);
                        if (cfg != null)
                            configs.Add(cfg);
                    }
                }
                if (configs.Count > 0) break;

            }
            return configs;
        }
        public static RuleConfig ParseSingleRule(Bracket ruleBr)
        {
            RuleConfig res = new RuleConfig
            {
                Id = new Identifier(ruleBr.SubVars.First(v => v.Name == "name").Name) ?? new("Null"),
                GroupId = ruleBr.SubVars.First(v => v.Name == "group").Name ?? "Null",
                RequiredDlc = ruleBr.SubVars.First(v => v.Name == "required_dlc").Name ?? "Null",
                ExcludedDlc = ruleBr.SubVars.First(v => v.Name == "excluded_dlc").Name ?? "Null",
                Options = new List<BaseConfig>(),
                Icon = ModManager.Mod.Gfxes.FindById(ruleBr.SubVars.First(v => v.Name == "icon").Value.ToString()) ?? new SpriteType(),
                Default = new BaseConfig { Id = new("default"), }
            };
            foreach(Bracket br in ruleBr.SubBrackets)
            {
                if (br.Name == "option")
                {
                    var option = new BaseConfig
                    {
                        Id = new Identifier(br.SubVars.First(v => v.Name == "name").Value.ToString()) ?? new("Null"),
                    };
                    ConfigLocalisation configLocalisation = new ConfigLocalisation();
                    var nameloc = ModManager.Localisation.GetLocalisationByKey(br.SubVars.First(v => v.Name == "text").Value.ToString());
                    var descloc = ModManager.Localisation.GetLocalisationByKey(br.SubVars.First(v => v.Name == "desc").Value.ToString());
                    configLocalisation.Data.Add(nameloc.Key, nameloc.Value);
                    configLocalisation.Data.Add(descloc.Key, descloc.Value);
                   
                }
                else if (br.Name == "default")
                {
                    res.Default = new BaseConfig
                    {
                        Id = new Identifier(br.SubVars.First(v => v.Name == "name").Value.ToString()) ?? new("Null"),
                    };
                    ConfigLocalisation configLocalisation = new ConfigLocalisation();
                    var nameloc = ModManager.Localisation.GetLocalisationByKey(br.SubVars.First(v => v.Name == "text").Value.ToString());
                    var descloc = ModManager.Localisation.GetLocalisationByKey(br.SubVars.First(v => v.Name == "desc").Value.ToString());
                    configLocalisation.Data.Add(nameloc.Key, nameloc.Value);
                    configLocalisation.Data.Add(descloc.Key, descloc.Value);
                    res.Default.Localisation = configLocalisation;
                }
            }
            return res;
        }
    }
}
