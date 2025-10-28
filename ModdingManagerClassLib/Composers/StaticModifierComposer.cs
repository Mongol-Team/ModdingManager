using ModdingManager.managers.@base;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class StaticModifierComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            string[] possiblePaths = {
                ModPathes.StaticModifiersPath,
                GamePathes.StaticModifiersPath
            };
            List<IConfig> configs = new List<IConfig>();
            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        HoiFuncFile funcfile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                        foreach (var bracket in funcfile.Brackets)
                        {
                            var cfg = ParseSingleModifer(bracket);
                            if (cfg != null)
                                configs.Add(cfg);
                        }

                    }
                }
                if (configs.Count > 0) break;
            }
            return configs;
        }
        public static StaticModifierConfig ParseSingleModifer(Bracket bracket)
        {
            StaticModifierConfig cfg = new StaticModifierConfig();
            cfg.Id = new Identifier(bracket.Name);
            cfg.Modifiers = new Dictionary<ModifierDefinitionConfig, object>();
            foreach (var var in bracket.SubVars)
            {

                var modDef = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                if (modDef != null)
                {
                    if (var.Value is int intValue)
                    {
                        cfg.Modifiers[modDef] = intValue;
                    }
                    else if (var.Value is double doubleValue)
                    {
                        cfg.Modifiers[modDef] = doubleValue;
                    }
                    else if (var.Value is string strValue)
                    {
                        cfg.Modifiers[modDef] = strValue;
                    }
                    else if (var.Value is bool boolValue)
                    {
                        cfg.Modifiers[modDef] = boolValue;
                    }
                }
            }
            return cfg;
        }
    }
}
