using ModdingManagerClassLib.utils.Pathes;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModdingManagerClassLib.Composers
{
    public class SubUnitComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.RegimentsPath,
                GamePathes.RegimentsPath
            };
            foreach (string path in possiblePathes)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        string fileContent = File.ReadAllText(file);
                        HoiFuncFile hoiFuncFile = (HoiFuncFile)new TxtParser(new TxtPattern()).Parse(fileContent);
                        List<SubUnitConfig> traitConfigs = ParseFile(hoiFuncFile);
                        foreach (SubUnitConfig traitConfig in traitConfigs)
                        {
                            if (!configs.Any(c => c.Id == traitConfig.Id))
                            {
                                configs.Add(traitConfig);
                            }
                        }
                    }
                }
            }
            return configs;
        }

        public static List<SubUnitConfig> ParseFile(HoiFuncFile file)
        {
            List<SubUnitConfig> configs = new List<SubUnitConfig>();
            foreach (Bracket bracket in file.Brackets.Where(b => b.Name == "sub_units"))
            {
                SubUnitConfig config = new SubUnitConfig();
                foreach (Bracket unitbr in bracket.SubBrackets)
                {
                    ParseObject(unitbr);
                }
                configs.Add(config);
            }

            return configs;
        }

        public static SubUnitConfig ParseObject(Bracket bracket)
        {
            SubUnitConfig config = new SubUnitConfig();
            config.Id = new Identifier(bracket.Name);
            foreach(Var var in bracket.SubVars)
            {
                switch (var.Name)
                {
                    case "sprite":
                        config.EntitySprite = var.Value as string; //todo: entity sprite class
                        break;
                    case "active":
                        config.Active = Convert.ToBoolean(var.Value);
                        break;
                    case "priority":
                        config.Priority = Convert.ToInt32(var.Value);
                        break;
                    case "map_icon_category":
                        if (Enum.TryParse<UnitMapIconType>(var.Value as string, true, out var mapIconType))
                        {
                            config.MapIconCategory = mapIconType;
                        }
                        break;
                    case "affects_speed":
                        config.AffectsSpeed = Convert.ToBoolean(var.Value);
                        break;
                    case "use_transport_speed":
                        var equipment = ModDataStorage.Mod.Equipments.FirstOrDefault(e => e.Id.ToString() == var.Value as string);
                        if (equipment != null)
                        {
                            config.UseTransportSpeed = equipment;
                        }
                        break;
                    case "group":
                        var group = ModDataStorage.Mod.SubUnitGroups.FirstOrDefault(g => g.Id.ToString() == var.Value as string);
                        if (group != null)
                        {
                            config.Group = group;
                        }
                        break;
                    case "ai_priority":
                        config.AiPriority = Convert.ToInt32(var.Value);
                        break;
                    case "can_exfiltrate_from_coast":
                        config.CanExfiltrateFromCoast = Convert.ToBoolean(var.Value);
                        break;
                    
                    default:
                        var modDef = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                        if (modDef != null)
                        {
                            config.Modifiers.Add(modDef, var.Value);
                        }

                        else 
                        {
                            //unknown var
                        }
                        break;


                }

            }
            foreach(Bracket subb in bracket.SubBrackets)
            {
                switch (subb.Name)
                {
                    case "need":
                        foreach (Var needVar in subb.SubVars)
                        {
                            var equipment = ModDataStorage.Mod.Equipments.FirstOrDefault(e => e.Id.ToString() == needVar.Name);
                            if (equipment != null)
                            {
                                config.Need.Add(equipment, Convert.ToInt32(needVar.Value));
                            }
                        }
                        break;
                    default:
                        bool isTerrainModifier = Enum.TryParse<ProvinceTerrain>(subb.Name, out var terrMod);
                        if (isTerrainModifier)
                        {
                            Dictionary<ModifierDefinitionConfig, object> terrModDict = new Dictionary<ModifierDefinitionConfig, object>();
                            foreach (Var terrVar in subb.SubVars)
                            {
                                var modDef = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == terrVar.Name);
                                
                                if (modDef != null)
                                {
                                    terrModDict.Add(modDef, terrVar.Value);
                                }
                            }
                            config.TerrainModifiers.TryAdd(terrMod, terrModDict);
                        }
                        break;
                }
            }
            foreach (HoiArray arr in bracket.Arrays)
            {
                switch (arr.Name)
                {
                    case "types":
                        List<IternalUnitType> types = new List<IternalUnitType>();
                        foreach (var typeObj in arr.Values)
                        {
                            if (Enum.TryParse<IternalUnitType>(typeObj.ToString(), true, out var unitType))
                            {
                                types.Add(unitType);
                            }
                        }
                        config.Types = types;
                        break;
                    case "chategories":
                        List<SubUnitCategoryConfig> chategories = new List<SubUnitCategoryConfig>();
                        foreach (var chategoryObj in arr.Values)
                        {
                            var chategory = ModDataStorage.Mod.SubUnitChategories.FirstOrDefault(c => c.Id.ToString() == chategoryObj.ToString());
                            if (chategory != null)
                            {
                                chategories.Add(chategory);
                            }
                        }
                        config.Chategories = chategories;
                        break;

                }
            }
            return config;
        }
    }
}
