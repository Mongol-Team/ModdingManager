using ModdingManager.managers.@base;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Types;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class DynamicModifierComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            string[] possiblePaths = {
                ModPathes.DynamicModifiersPath,
                GamePathes.DynamicModifiersPath
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
       
        public static DynamicModifierConfig ParseSingleModifer(Bracket bracket)
        {
            DynamicModifierConfig cfg = new DynamicModifierConfig();
            cfg.Id = new Identifier(bracket.Name);
            cfg.Modifiers = new Dictionary<ModifierDefinitionConfig, object>();
            foreach (var var in bracket.SubVars)
            {
                if (var.Name == "icon" && var.PossibleCsType == typeof(HoiReference))
                {
                    cfg.Icon = ModManager.Mod.Gfxes.FirstOrDefault(gf => gf.Id.ToString() == (var.Value as HoiReference).Value.ToString());
                }
                else if (var.Name == "attacker_modifier" && var.PossibleCsType == typeof(bool))
                {
                    cfg.HasAttackerEffect = (bool)var.Value;
                }
                else
                {
                    var modDef = ModManager.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
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
                        else if(var.Value is HoiReference refVal) 
                        {
                            cfg.Modifiers[modDef] = refVal;
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
            }
            return cfg;
        }
    }
}


