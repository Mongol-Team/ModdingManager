using Application.Debugging;
using Application.Extentions;
using Application.Settings;
using Application.utils.Pathes;
using Models.Configs;
using Models.GfxTypes;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using DDF = Data.DataDefaultValues;

namespace Application.Composers
{
    internal class IdeaComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> result = new();
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
                    foreach (IConfig cf in cfgs)
                    {
                        cf.FileFullPath = fileStr;
                    }
                    if (cfgs != null && cfgs.Count > 0)
                    {
                        result.AddRange(cfgs);
                    }
                }

            }
            return result;
        }
        public static List<IConfig> ParseFile(string path)
        {
            HoiFuncFile file = new TxtParser(new TxtPattern()).Parse(path) as HoiFuncFile;
            List<Bracket> ideaBrs = file.Brackets.Where(b => b.Name == "ideas").ToList();
            List<IConfig> res = new();
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
            IdeaConfig idea = new()
            {
                Id = new Identifier(ideaBr.Name),
                Localisation = new ConfigLocalisation() { Language = ModManagerSettings.CurrentLanguage },
                Modifiers = new Dictionary<ModifierDefinitionConfig, object>(),
                Gfx = new SpriteType(DDF.NullImageSource, DDF.Null),
                Tag = DDF.Null,
                RemovalCost = DDF.NullInt, //+
                Available = DDF.Null, //+
                AvailableCivilWar = DDF.Null, //+
                Allowed = DDF.Null, //+
                AllowedToRemove = DDF.Null, //+
                Cost = DDF.NullInt, //+
                OnAdd = DDF.Null, //+
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
                            ModifierDefinitionConfig config = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(md => md.Id.ToString() == name);
                            if (config != null)
                            {
                                idea.Modifiers.TryAdd(config, v.Value);
                                Logger.AddDbgLog($"idea: {idea.Id.ToString()}, mod:{config.Id.ToString()}", "IdeaComposer");
                            }
                        }
                        break;
                }
            }
            idea.PictureName = idea.PictureName ?? $"GFX_idea_{idea.Id.ToString()}";
            string keyname = idea.Id.ToString();
            KeyValuePair<string, string> idloc = ModDataStorage.Localisation.GetLocalisationByKey(keyname);
            idea.Localisation.Data.AddPair(idloc);
            KeyValuePair<string, string> descloc = ModDataStorage.Localisation.GetLocalisationByKey(keyname);
            idea.Localisation.Data.AddPair<string, string>(descloc);
            var dfg = idea.Id.ToString();
            var sdgfg = idea.PictureName;
            IGfx gfx = ModDataStorage.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == $"GFX_idea_{idea.PictureName}")
           ?? ModDataStorage.Mod.Gfxes.FirstOrDefault(g => g.Id.ToString() == $"GFX_idea_{idea.Id.ToString()}") ?? new SpriteType(DDF.NullImageSource, DDF.Null);

            if (gfx.Id.ToString() == "GFX_idea_AZM_anarchist_economics")
            {
                float dd = 0;
            }
            idea.Gfx = gfx;

            return idea;
        }
    }
}
