using Application.Extentions;
using Application.utils.Pathes;
using Models;
using Models.Enums;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Composers
{
    public class OpinionModifierComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.OpinionModifiersPath,
                GamePathes.OpinionModifiersPath
            };
            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach(string file in files)
                {
                    if (File.Exists(file))
                    {
                        string fileContent = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);
                        List<IConfig> fileConfigs = ParseFile(hoiFuncFile);
                        foreach (IConfig config in fileConfigs)
                        {
                            if (!configs.Any(c => c.Id.ToString() == config.Id.ToString()))
                            {
                                configs.Add(config);
                            }
                        }
                    }
                }
            }
            return configs;

        }

        public static List<IConfig> ParseFile(HoiFuncFile hoiFuncFile)
        {
            List<IConfig> configs = new List<IConfig>();
            foreach (Bracket br in hoiFuncFile.Brackets.Where(b => b.Name == "opinion_modifiers"))
            {
                foreach (Bracket opinBr in br.SubBrackets)
                {
                    configs.AddSafe(ParseObject(opinBr));
                }
            }
            return configs;
        }

        public static IConfig ParseObject(Bracket modbr)
        {
            OpinionModifierConfig config = new OpinionModifierConfig();
            config.Id = new Identifier(modbr.Name);
            foreach (Var item in modbr.SubVars)
            {
                switch (item.Name)
                {
                    case "name":
                        config.Name = item.Value as string;
                        break;
                    case "description":
                        config.Description = item.Value as string;
                        break;
                    case "is_trade":
                        config.IsTrade = item.ToBool();
                        break;
                    case "value":
                        config.Value = item.ToInt();
                        break;
                    case "decay":
                        config.Decay = item.ToInt();
                        break;
                    case "days":
                        config.RemovalTime.SumToKey(TimeUnit.Day, item.ToInt());
                        break;
                    case "months":
                        config.RemovalTime.SumToKey(TimeUnit.Day, item.ToInt() * 30);
                        break;
                    case "years":
                        config.RemovalTime.SumToKey(TimeUnit.Day, item.ToInt() * 365);
                        break;
                    case "min_trust":
                        config.MinTrust = item.ToInt();
                        break;
                    case "max_trust":
                        config.MaxTrust = item.ToInt();
                        break;
                }
            }
            return config;
        }
    }
}
