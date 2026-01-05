
using Application.Debugging;
using Application.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using Models.Enums;
using Models.Types.ObjectCacheData;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Typography.OpenFont.CFF;
using Models.Configs;

namespace Application.Composers
{
    public class IdeaTagComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            string[] pathes = new string[]
            {
                ModPathes.IdeaTagsPath,
                GamePathes.IdeaTagsPath
            };
            List<IConfig> res = new List<IConfig>();
            foreach (var path in pathes)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        try
                        {
                            HoiFuncFile parsedfile = new TxtParser(new TxtPattern()).Parse(file) as HoiFuncFile;
                            var parsed = ParseFile(parsedfile);
                            if (parsed != null)
                                res.AddRange(parsed);
                        }
                        catch (Exception ex)
                        {
                            Logger.AddDbgLog($"[IdeaTagComposer] On parse exception: {ex.Message}{ex.StackTrace}", "IdeaTagComposer");
                        }
                    }
                }
                if (res.Count > 0)
                    break;
            }
            return res;
        }

        public static List<IConfig> ParseFile(HoiFuncFile file)
        {
            List<IConfig> res = new List<IConfig>();
            foreach (var bracket in file.Brackets)
            {
                if (bracket.Name == "idea_categories")
                {
                    foreach (var subbracket in bracket.SubBrackets)
                    {
                        IdeaTagConfig cfg = ParseObject(subbracket) as IdeaTagConfig;
                        res.Add(cfg);
                    }
                }
            }
            return res;
        }

        public static IConfig ParseObject(Bracket br)
        {
            IdeaTagConfig cfg = new IdeaTagConfig();
            cfg.Id = new(br.Name);
            foreach (Var var in br.SubVars)
            {
                switch (var.Name)
                {
                    case "type":
                        switch (var.Value.ToString())
                        {
                            case "army_spirit":
                                cfg.Type = IdeaType.ArmySpirit;
                                break;
                            case "navy_spirit":
                                cfg.Type = IdeaType.NavySpirit;
                                break;
                            case "air_spirit":
                                cfg.Type = IdeaType.AirSpirit;
                                break;
                            case "national_spirit":
                                cfg.Type = IdeaType.NationalSpirit;
                                break;
                        }
                        break;
                    case "slot":
                        IdeaGroupConfig scfg = ModDataStorage.Mod.IdeaSlots.FirstOrDefault(s => s.Id.ToString() == var.Value.ToString());
                        if (scfg != null)
                            cfg.Slots.Add(scfg);
                        break;
                    case "character_slot":
                        cfg.CharacterSlots.Add(var.Value.ToString());

                        break;
                    case "cost":
                        if (int.TryParse(var.Value.ToString(), out int cost))
                            cfg.Cost = cost;
                        break;
                    case "removal_cost":
                        if (int.TryParse(var.Value.ToString(), out int rcost))
                            cfg.RemovalCost = rcost;
                        break;
                    case "ledger":
                        switch (var.Value.ToString())
                        {
                            case "civilian":
                                cfg.Ledger = IdeaLedgerType.Civilian;
                                break;
                            case "army":
                                cfg.Ledger = IdeaLedgerType.Army;
                                break;
                            case "air":
                                cfg.Ledger = IdeaLedgerType.Air;
                                break;
                            case "navy":
                                cfg.Ledger = IdeaLedgerType.Navy;
                                break;
                            case "military":
                                cfg.Ledger = IdeaLedgerType.Military;
                                break;
                            case "all":
                                cfg.Ledger = IdeaLedgerType.All;
                                break;
                            case "hidden":
                                cfg.Ledger = IdeaLedgerType.Hidden;
                                break;
                            case "invalid":
                                cfg.Ledger = IdeaLedgerType.Invalid;
                                break;
                            default:
                                cfg.Ledger = IdeaLedgerType.Invalid;
                                break;
                        }
                        break;
                    case "hidden":
                        if (var.Value is bool hval)
                            cfg.Hidden = hval;
                        break;
                    case "politics_tab":
                        if (var.Value is bool pval)
                            cfg.PoliticsTab = pval;
                        break;
                    default:
                        break;
                }
            }

            return cfg;
        }
    }
}
