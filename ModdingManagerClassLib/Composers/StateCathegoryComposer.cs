using ModdingManager.managers.@base;
using ModdingManagerClassLib.Extentions;
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
    public class StateCathegoryComposer : IComposer
    {
        public StateCathegoryComposer() { }
        public static List<IConfig> Parse()
        {
            string[] possiblePaths = {
                ModPathes.StateCathegoryPath,
                GamePathes.StateCathegoryPath
            };
            foreach (string path in possiblePaths)
            {
                if (!File.Exists(path))
                    continue;
                return ParseStateCathegoryFile(path).Cast<IConfig>().ToList();
            }
            return new List<IConfig>();
        }
        public static List<StateCathegoryConfig> ParseStateCathegoryFile(string filePath)
        {
            List<StateCathegoryConfig> result = new();
            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(filePath) as HoiFuncFile;
            foreach (var bracket in file.Brackets.FirstOrDefault(b => b.Name == "state_categories").SubBrackets)
            {
                if (bracket.Name != null)
                {

                    StateCathegoryConfig cfg = new()
                    {
                        Color = bracket.Arrays.FirstOrDefault(a => a.Name == "color").AsColor(),
                        Id = bracket.Name as string ?? "unknown",
                    };
                    foreach (Var mod in bracket.SubVars)
                    {
                        cfg.Modifiers[ModManager.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == mod.Name)] = mod.Value;
                    }
                    result.Add(cfg);
                }
               
            }
            return result;
        }
    }
}
