
using Application.Debugging;
using Application.Extentions;
using Application.utils;
using Application.utils.Pathes;
using Data;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models.GfxTypes;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.TableCacheData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DDF = Data.DataDefaultValues;
using Models.Configs;

namespace Application.Composers
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
            configs.AddRange(ParseCoreRules());
            foreach (string path in possiblePaths)
            {
                List<string> files = null;
                try
                {
                    files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories).ToList();
                }
                catch (Exception ex)
                {
                    files = new List<string>();
                }
                foreach (string file in files)
                {
                    var funcfile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                    foreach (var br in funcfile.Brackets)
                    {
                        var cfg = ParseSingleRule(br);
                        cfg.FileFullPath = file;
                        if (cfg != null)
                            configs.Add(cfg);
                    }
                }
                if (configs.Count > 0) break;

            }
            return configs;
        }
        public static List<RuleConfig> ParseCoreRules()
        {
            List<RuleConfig> configs = new List<RuleConfig>();
            HoiTable tbl = new CsvParser(new RulesDataPattern()).Parse(DataLib.RulesCoreDefenitions) as HoiTable;
            foreach (List<object> row in tbl.Values)
            {
                RuleConfig cfg = new()
                {
                    Id = new(row[0].ToString())
                };
                configs.Add(cfg);
            }

            return configs;
        }
        public static RuleConfig ParseSingleRule(Bracket ruleBr)
        {
            if (ruleBr == null)
            {
                return null;
            }
            RuleConfig res = new RuleConfig();
            try
            {
                res = new RuleConfig
                {
                    Id = new(ruleBr.Name),
                    GroupId = ruleBr.SubVars.FirstOrDefault(v => v.Name == "group") == null ? DDF.Null : ruleBr.SubVars.FirstOrDefault(v => v.Name == "group").Value.ToString(),
                    RequiredDlc = ruleBr.SubVars.FirstOrDefault(v => v.Name == "required_dlc") == null ? DDF.Null : ruleBr.SubVars.FirstOrDefault(v => v.Name == "required_dlc").Value.ToString(),
                    ExcludedDlc = ruleBr.SubVars.FirstOrDefault(v => v.Name == "excluded_dlc") == null ? DDF.Null : ruleBr.SubVars.FirstOrDefault(v => v.Name == "excluded_dlc").Value.ToString(),
                    Options = new List<BaseConfig>(),
                    Gfx = ruleBr.SubVars.FirstOrDefault(v => v.Name == "icon")?.Value is var val && val != null ? ModDataStorage.Mod.Gfxes.FindById(val.ToString()) ?? new SpriteType() : new SpriteType(),
                    Default = new BaseConfig(),
                    IsCore = false
                };
                res.Localisation = new ConfigLocalisation();
                string locKey = ruleBr.SubVars.FirstOrDefault(v => v.Name == "name") == null ? DDF.Null : ruleBr.SubVars.FirstOrDefault(v => v.Name == "name").Value.ToString();
                KeyValuePair<string, string> loc = ModDataStorage.Localisation.GetLocalisationByKey(locKey);

                res.Localisation.Data.Add(loc.Key, loc.Value);
            }
            catch (Exception ex)
            {
                return null;
            }
            foreach (Bracket br in ruleBr.SubBrackets)
            {

                if (br.Name == "option")
                {
                    var option = new BaseConfig
                    {
                        Id = new Identifier(br.SubVars.FirstOrDefault(v => v.Name == "name").Value.ToString()) ?? new(DDF.Null),
                        Localisation = new ConfigLocalisation()
                    };

                    var nameloc = ModDataStorage.Localisation.GetLocalisationByKey(br.SubVars.FirstOrDefault(v => v.Name == "text").Value.ToString());
                    var descloc = ModDataStorage.Localisation.GetLocalisationByKey(br.SubVars.FirstOrDefault(v => v.Name == "desc").Value.ToString());
                    option.Localisation.Data.Add(nameloc.Key, nameloc.Value);
                    option.Localisation.Data.Add(descloc.Key, descloc.Value);
                    res.Options.Add(option);
                }
                else if (br.Name == "default")
                {
                    res.Default = new BaseConfig
                    {
                        Id = new Identifier(br.SubVars.First(v => v.Name == "name").Value.ToString()) ?? new(DDF.Null),
                        Localisation = new ConfigLocalisation()
                    };

                    var nameloc = ModDataStorage.Localisation.GetLocalisationByKey(br.SubVars.FirstOrDefault(v => v.Name == "text").Value.ToString());
                    var descloc = ModDataStorage.Localisation.GetLocalisationByKey(br.SubVars.FirstOrDefault(v => v.Name == "desc").Value.ToString());
                    res.Default.Localisation.Data.Add(nameloc.Key, nameloc.Value);
                    res.Default.Localisation.Data.Add(descloc.Key, descloc.Value);
                }
            }

            return res;

        }
    }
}
