using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class ModifierDefComposer : IComposer
    {
        public ModifierDefComposer() { }
        public static List<IConfig> Parse()
        {
            string[] possibleDefPaths = {
                ModPathes.ModifierDefFirstPath,
                GamePathes.ModifierDefSecondPath,
            };
            string[] possibleDocPath = {
                GamePathes.ModifierDefSecondPath,
                ModPathes.ModifierDefSecondPath,
            };
            List<IConfig> res = new List<IConfig>();   
            foreach (string defPath in possibleDefPaths)
            {
                var files = Directory.GetFiles(defPath, "*.txt", SearchOption.AllDirectories).ToList();
                if (files.Count == 0)
                    continue;
                foreach (var file in files)
                {
                    HoiFuncFile hoiFuncFile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                    foreach(var bracket in hoiFuncFile.Brackets)
                    {
                        ModifierDefenitionConfig cfg = ParseModifierDefConfig(bracket);
                        if( cfg != null)
                            res.Add(cfg);
                    }
                }
                if (res.Count > 0)
                    return res;
            }
            return res;
        }
        public static ModifierDefenitionConfig ParseModifierDefConfig(Bracket bracket)
        {
            ModifierDefenitionConfig config = new ModifierDefenitionConfig();
            config.Name = bracket.Name;
            foreach(var var in bracket.SubVars)
            {
                switch(var.Name)
                {
                    case "value_type":
                        if (Enum.TryParse<ModifierDefenitionValueType>(var.Value.ToString(), true, out var valueType))
                            config.ValueType = valueType;
                        break;
                    case "precision":
                        if (int.TryParse(var.Value.ToString(), out var precision))
                            config.Precision = precision;
                        break;
                    case "cathegory":
                        if (Enum.TryParse<ModifierDefinitionCathegoryType>(var.Value.ToString(), true, out var cathegory))
                            config.Cathegory = cathegory;
                        break;
                    case "color_type":
                        if (Enum.TryParse<ModifierDefenitionColorType>(var.Value.ToString(), true, out var colorType))
                            config.ColorType = colorType;
                        break;
                }
            }
            return config;
        }
    }
}
