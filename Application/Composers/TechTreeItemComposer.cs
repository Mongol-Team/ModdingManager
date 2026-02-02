using Application.Debugging;
using Application.Extensions;
using Application.Extentions;
using Application.utils.Pathes;
using Models.Configs;
using Models.Types.ObectCacheData;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;


namespace Application.Composers
{
    public class TechTreeItemComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new();
            string[] possiblePathes =
            {
                ModPathes.TechTreePath,
                GamePathes.TechTreePath
            };
            HashSet<string> seenIds = new();
            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (new TxtParser(new TxtPattern()).Parse(file) is not HoiFuncFile funcFile) continue;

                    foreach (var bracket in funcFile.Brackets)
                    {
                        if (bracket.Name == "technologies")
                        {
                            List<TechTreeItemConfig> configList = ParseTechnologyConfigItemsFromFile(bracket);
                            if (configList == null) continue;

                            foreach (var config in configList)
                            {
                                config.FileFullPath = file;
                                string id = config.Id.ToString();
                                if (!seenIds.Contains(id))
                                {
                                    configs.Add(config);
                                    seenIds.Add(id);
                                }
                            }
                        }
                    }
                }
            }
            return configs;
        }

        public static List<TechTreeItemConfig> ParseTechnologyConfigItemsFromFile(Bracket bracket)
        {
            List<TechTreeItemConfig> res = new();

            foreach (Bracket techBr in bracket.SubBrackets)
            {
                string id = techBr.Name;
                TechTreeItemConfig item = ModDataStorage.Mod.TechTreeItems.FindById(id);
                if (item == null)
                {
                    item = new TechTreeItemConfig();
                    item.Id = new Identifier(id);
                }
                else
                {
                    res.Add(item);
                    continue;
                }
                foreach (Var argVar in techBr.SubVars)
                {
                    switch (argVar.Name.ToLowerInvariant())
                    {
                        case "research_cost":
                            item.Cost = argVar.Value.ToInt();
                            break;
                        case "start_year":
                            item.StartYear = argVar.Value.ToInt();
                            break;
                        case "force_use_small_tech_layout":
                            item.IsBig = argVar.Value.ToBool();
                            break;
                        case "show_equipment_icon":
                            item.ShowEqIcon = argVar.Value.ToBool();
                            break;
                        case "desc":
                            item.SpecialDescKey = argVar.Value.ToString();
                            break;
                        default:
                            item.Modifiers.AddSafe(new(ModDataStorage.Mod.ModifierDefinitions.FindById(argVar.Name), argVar.Value));
                            break;
                    }
                }
                TechTreeConfig fl = null;
                foreach (Bracket argBr in techBr.SubBrackets)
                {
                    switch (argBr.Name.ToLowerInvariant())
                    {
                        case "path":
                            Var coef = argBr.SubVars.FirstOrDefault(v => v.Name == "research_cost_coeff");
                            if (coef != null)
                            {
                                item.ModifCost = coef.Value.ToInt();
                            }
                            Var childV = argBr.SubVars.FirstOrDefault(v => v.Name == "leads_to_tech");
                            if (childV == null)
                            {
                                break;
                            }
                            TechTreeItemConfig cildTech = ModDataStorage.Mod.TechTreeLedgers.GetTreeItem(childV.Value.ToString());
                            if (cildTech == null)
                            {
                                cildTech = new TechTreeItemConfig();
                                cildTech.Id = new Identifier(childV.Value.ToString());
                            }
                            cildTech.ChildOf.AddSafe(item);

                            break;
                        case "folder":
                            Var flname = argBr.SubVars.FirstOrDefault(v => v.Name == "name");
                            if (flname == null)
                            {
                                Logger.AddLog($"[Warning] Item {item.Id.ToString} has folder br without name.");
                            }
                            else
                            {
                                fl = ModDataStorage.Mod.TechTreeLedgers.FirstOrDefault(t => t.Id.ToString() == flname.Value.ToString());
                                if (fl == null)
                                {
                                    Logger.AddLog($"[Warning] Item {item.Id.ToString} has null folder.");
                                }
                                else
                                {
                                    fl.Items.AddSafe(item);
                                }
                            }

                            Bracket posbr = argBr.SubBrackets.FirstOrDefault(b => b.Name == "position");
                            if (posbr == null)
                            {
                                Logger.AddLog($"[Warning] Item {item.Id.ToString} has no coords br.");
                            }
                            else
                            {
                                Var x = posbr.SubVars.FirstOrDefault(v => v.Name == "x");
                                Var y = posbr.SubVars.FirstOrDefault(v => v.Name == "y");
                                if (y == null && x == null)
                                {
                                    Logger.AddLog($"[Warning] Item {item.Id.ToString} has no coords in br.");
                                    break;
                                }
                                item.GridX = x.ToInt();
                                item.GridY = y.ToInt();
                            }
                            break;
                        case "enable_building":
                            Var building = argBr.SubVars.FirstOrDefault(v => v.Name == "building ");
                            Var level = argBr.SubVars.FirstOrDefault(v => v.Name == "level");
                            if (building == null || level == null)
                            {
                                Logger.AddLog($"[Warning] Enable building br of {item} is missing building or level var.");
                                break;
                            }
                            BuildingConfig bitem = ModDataStorage.Mod.Buildings.FirstOrDefault(b => b.Id.ToString() == building.Value.ToString());
                            if (bitem == null)
                            {
                                break;
                            }
                            item.EnableBuildings.Add(bitem, level.Value);
                            break;
                        case "on_research_complete":
                            item.Effects = argBr.ToString();
                            break;
                        case "ai_will_do":
                            item.AiWillDo = argBr.ToString();
                            break;
                        case "allow_branch":
                            item.AllowBranch = argBr.ToString();
                            break;
                        case "allowed":
                            item.Allowed = argBr.ToString();
                            break;
                        case "xor":

                            break;
                    }
                }
                foreach (HoiArray arr in techBr.Arrays)
                {
                    switch (arr.Name.ToLowerInvariant())
                    {
                        case "xor":
                            if (arr.Values == null)
                            {
                                Logger.AddLog($"[Warning] XOR br of {item} is empty.");
                                break;
                            }
                            foreach (var value in arr.Values)
                            {
                                TechTreeItemConfig itm = ModDataStorage.Mod.TechTreeItems.FindById(value.ToString());
                                if (item == null)
                                {
                                    Logger.AddDbgLog($"[Warning] Item {value.ToString()} is no existng. Creating empty...", "TechTreeItemComposer");
                                    itm = new TechTreeItemConfig();
                                    itm.Id = new(value.ToString());
                                }
                                item.Mutal.Add(itm);
                            }
                            break;
                        case "categories":
                            if (arr.Values == null)
                            {
                                Logger.AddLog($"[Warning] Item {item.Id} has no categories defined.");
                                break;
                            }
                            foreach (object val in arr.Values)
                            {
                                TechCategoryConfig category = ModDataStorage.Mod.TechCategories.FirstOrDefault(c => c.Id.ToString() == val.ToString());
                                if (category == null)
                                {
                                    Logger.AddLog($"[Warning] Item {item.Id} has {val.ToString()} that undef category");
                                    continue;
                                }
                                item.Categories.AddSafe(ModDataStorage.Mod.TechCategories.FirstOrDefault(c => c.Id.ToString() == val.ToString()));
                            }
                            break;
                        case "enable_equipments":
                            if (arr.Values == null)
                            {
                                Logger.AddLog($"[Warning] Item {item.Id} has no enablig eq defined.");
                                break;
                            }
                            foreach (object val in arr.Values)
                            {
                                EquipmentConfig equip = ModDataStorage.Mod.Equipments.FirstOrDefault(e => e.Id.ToString() == val.ToString());
                                if (equip == null)
                                {
                                    Logger.AddDbgLog($"[Warning] Equpment {equip.Id.ToString()} is undefiend.");
                                    continue;
                                }
                                item.EnableEquipments.Add(equip);
                            }
                            break;
                        case "enable_subunits":
                            if (arr.Values == null)
                            {
                                Logger.AddLog($"[Warning] Item {item.Id} has no enablig eq defined.");
                                break;
                            }
                            foreach (object val in arr.Values)
                            {
                                SubUnitConfig reg = ModDataStorage.Mod.SubUnits.FirstOrDefault(e => e.Id.ToString() == val.ToString());
                                if (reg == null)
                                {
                                    Logger.AddDbgLog($"[Warning] Regiment {reg.Id.ToString()} is undefiend.");
                                    continue;
                                }
                                item.EnableUnits.Add(reg);
                            }
                            break;
                        case "enable_equipment_modules":
                            //todo: modules
                            break;
                    }
                }
                res.Add(item);
            }
            return res;
        }
    }
}
