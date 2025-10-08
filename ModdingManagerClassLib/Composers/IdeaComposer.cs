using ModdingManager.managers.@base;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.LocalizationData;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    internal class IdeaComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> result = new List<IConfig>();
            string[] possiblePathes = {
                ModPathes.IdeasPath,
                GamePathes.IdeasPath,
            };
            foreach (string possiblePath in possiblePathes)
            {
                string[] fileStrs = Directory.GetFiles(possiblePath);
                foreach (string fileStr in fileStrs)
                {
                    List<IConfig> cfgs = ParseFile(fileStr);
                    if (cfgs != null && cfgs.Count > 0)
                    {
                        result.AddRange(cfgs);
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            return result;
        }
        public static List<IConfig> ParseFile(string path)
        {
            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(path) as HoiFuncFile;
            List<Bracket> ideaBrs = file.Brackets.Where(b => b.Name == "ideas").ToList();
            List<IConfig> res = new List<IConfig>();
            foreach (Bracket ideabr in ideaBrs)
            {
                foreach (Bracket slotbr in ideabr.SubBrackets)
                {
                    foreach (Bracket sideabr in slotbr.SubBrackets)
                    {
                        IConfig cfg = ParseSingleIdea(sideabr);
                        res.Add(cfg);
                    }
                }
            }
            return res;

        }
        public static IConfig ParseSingleIdea(Bracket ideaBr)
        {
            IdeaConfig idea = new IdeaConfig()
            {
                Id = new Identifier(ideaBr.Name),
                Localisation = new ConfigLocalisation() { Language = ModManager.CurrentLanguage },
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                Gfx = DataDefaultValues.NullImage,
                Tag = DataDefaultValues.Null,
                RemovalCost = DataDefaultValues.NullInt, //+
                Available = DataDefaultValues.Null, //+
                AvailableCivilWar = DataDefaultValues.Null, //+
                Allowed = DataDefaultValues.Null, //+
                AllowedToRemove = DataDefaultValues.Null, //+
                Cost = DataDefaultValues.NullInt, //+
                OnAdd = DataDefaultValues.Null, //+
            };
            foreach (Var var in ideaBr.SubVars)
            {
                switch (var.Name)
                {
                    case "picture":
                        idea.PictureName = var.Value.ToString();
                        break;
                    case "cost":
                        var costVar = ideaBr.SubVars.FirstOrDefault(v => v.Name == "cost");
                        if (costVar?.Value != null)
                            idea.Cost = costVar.Value.ToInt();
                        break;
                    case "removal_cost":
                        var removalVar = ideaBr.SubVars.FirstOrDefault(v => v.Name == "removal_cost");
                        if (removalVar?.Value != null)
                            idea.RemovalCost = removalVar.Value.ToInt();
                        break;
                   
                    case "on_add":
                        var onAddVar = ideaBr.SubVars.FirstOrDefault(v => v.Name == "on_add");
                        if (onAddVar?.Value != null)
                            idea.OnAdd = onAddVar.Value.ToString();
                        break;
                }
            }

            foreach (Bracket br in ideaBr.SubBrackets)
            {
                switch (br.Name)
                {
                    case "allowed":
                        var allowedBr = ideaBr.SubBrackets.FirstOrDefault(v => v.Name == "allowed");
                        if (allowedBr != null)
                            idea.Allowed = allowedBr.ToString();
                        break;

                    case "allowed_to_remove":
                        var allowedToRemoveBr = ideaBr.SubBrackets.FirstOrDefault(v => v.Name == "allowed_to_remove");
                        if (allowedToRemoveBr != null)
                            idea.AllowedToRemove = allowedToRemoveBr.ToString();
                        break;

                    case "available":
                        var availableBr = ideaBr.SubBrackets.FirstOrDefault(v => v.Name == "available");
                        if (availableBr != null)
                            idea.Available = availableBr.ToString();
                        break;
                    case "available_civil_war":
                        var availableCWBr = ideaBr.SubBrackets.FirstOrDefault(v => v.Name == "available_civil_war");
                        if (availableCWBr != null)
                            idea.AvailableCivilWar = availableCWBr.ToString();
                        break;
                    case "modifier":
                        foreach (Var v in br.SubVars)
                        {
                            string name = v.Name;
                            ModifierDefinitionConfig config = ModManager.Mod.ModifierDefinitions.FirstOrDefault(md => md.Id.ToString() == name);
                            if (config != null)
                            {
                                idea.Modifiers.TryAdd(config, v.Value);
                                //Logger.AddDbgLog($"idea: {idea.Id.ToString()}, mod:{config.Id.ToString()}");
                            }
                        }
                        break;
                }
            }
            string keyname = idea.Id.ToString();
            KeyValuePair<string, string> idloc = ModManager.Localisation.GetLocalisationByKey(keyname);
            idea.Localisation.Data.AddPair(idloc);
            KeyValuePair<string, string> descloc = ModManager.Localisation.GetLocalisationByKey(keyname);
            idea.Localisation.Data.AddPair<string, string>(descloc);
            IGfx gfx = ModManager.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == $"GFX_idea_{idea.PictureName}");
            if (gfx == null)
            {
                Logger.AddDbgLog($"Idea found:{keyname}, but GfX_{keyname} no");
            }
            if ( gfx != null)
            {
                Logger.AddDbgLog($"Idea found:{keyname}, but GfX_{keyname} yes");
            }
            idea.Gfx = gfx ?? DataDefaultValues.NullImage;
           
            return idea;
        }
    }
}
