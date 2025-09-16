using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types.HtmlFilesData;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;

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
            string[] possibleDocPaths = {
                ModPathes.ModifierDefSecondPath,
                GamePathes.ModifierDefSecondPath,
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
                    foreach (var bracket in hoiFuncFile.Brackets)
                    {
                        ModifierDefinitionConfig cfg = ParseModifierDefConfig(bracket);
                        if (cfg != null)
                            res.Add(cfg);
                    }
                }
                if (res.Count > 0)
                    break;
            }
            List<ModifierDefinitionConfig> docmodif = new List<ModifierDefinitionConfig>();
            ModifierParser parser = new ModifierParser();
            foreach (string path in possibleDocPaths)
            {
                if (!File.Exists(path))
                    continue;
                docmodif = (parser.Parse(path) as ModifierDefinitionFile).ModifierDefinitions;
                foreach (var modifier in docmodif)
                {
                    var corecfg = modifier;
                    corecfg.IsCore = true;
                    res.Add(corecfg);
                }
                if (docmodif.Count != 0)
                {
                    break;
                }
            }

            return res;
        }
        public static ModifierDefinitionConfig ParseModifierDefConfig(Bracket bracket)
        {
            ModifierDefinitionConfig config = new ModifierDefinitionConfig();
            config.Id = new Identifier(bracket.Name);
            foreach (var var in bracket.SubVars)
            {
                switch (var.Name)
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
