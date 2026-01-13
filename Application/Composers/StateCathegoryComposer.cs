
using Application.Extentions;
using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models.Interfaces;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Configs;

namespace Application.Composers
{
    public class StateCathegoryComposer : IComposer
    {
        public StateCathegoryComposer() { }
        public static List<IConfig> Parse()
        {
            List<IConfig> res = new List<IConfig>();
            string[] possiblePaths = {
                ModPathes.StateCathegoryPath,
                GamePathes.StateCathegoryPath
            };
            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path))
                    continue;
                string[] files = Directory.GetFiles(path, "*.txt");
                if (files.Length == 0)
                    continue;
                foreach (string file in files)
                {
                    res.AddRange(ParseStateCathegoryFile(file).Cast<IConfig>().ToList());
                    foreach (var cfg in res)
                    {
                        cfg.FileFullPath = file;
                    }
                }
                if (res.Count != 0)
                {
                    break;
                }

            }
            return res;
        }
        public static List<StateCathegoryConfig> ParseStateCathegoryFile(string FileFullPath)
        {
            List<StateCathegoryConfig> result = new();
            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(FileFullPath) as HoiFuncFile;
            foreach (var bracket in file.Brackets.FirstOrDefault(b => b.Name == "state_categories").SubBrackets)
            {
                if (bracket.Name != null)
                {

                    StateCathegoryConfig cfg = new()
                    {
                        Color = bracket.Arrays.FirstOrDefault(a => a.Name == "color").AsColor(),
                        Id = new Identifier(bracket.Name as string) ?? new("unknown"),

                    };
                    foreach (Var mod in bracket.SubVars)
                    {
                        ModifierDefinitionConfig? modDef = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == mod.Name);
                        cfg.Modifiers.Add(modDef, mod.Value);
                    }
                    result.Add(cfg);
                }

            }
            return result;
        }
    }
}
