using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using ModdingManagerModels.Types.ObectCacheData;
using ModdingManagerModels.Types.ObjectCacheData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Composers
{
    public class CharacterTraitComposer : IComposer
    {
        public static List<IConfig> Parse()
        {
            List<IConfig> configs = new List<IConfig>();
            string[] possiblePathes =
            {
                ModPathes.TraitsPath,
                GamePathes.TraitsPath
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
                        List<CharacterTraitConfig> traitConfigs = ParseFile(hoiFuncFile);
                        foreach (CharacterTraitConfig traitConfig in traitConfigs)
                        {
                            if (!configs.Any(c => c.Id.ToString() == traitConfig.Id.ToString()))
                            {
                                configs.Add(traitConfig);
                            }
                        }
                    }
                }
            }
            return configs;
        }

        public static List<CharacterTraitConfig> ParseFile(HoiFuncFile file)
        {
            List<CharacterTraitConfig> traitConfigs = new List<CharacterTraitConfig>();
            foreach (Bracket bracket in file.Brackets)
            {
                if (bracket.Name == "leader_traits")
                {
                    foreach (Bracket traitBracket in bracket.SubBrackets)
                    {
                        CharacterTraitConfig traitConfig = ParseTraitConfig(traitBracket);
                        traitConfigs.Add(traitConfig);
                    }
                }
                else
                {
                    Logger.AddLog($"Unknown braket found: {bracket.Name} when trait parsing {file.FilePath}.");
                }
            }

            return traitConfigs;
        }
        public static CharacterTraitConfig ParseTraitConfig(Bracket bracket)
        {
            CharacterTraitConfig characterTrait = new CharacterTraitConfig();
            characterTrait.Id = new(bracket.Name);
            foreach (Var var in bracket.SubVars)
            {
                switch (var.Name)
                {
                    case "random":
                        break;
                    case "trait_type":
                        Enum.TryParse<TraitType>(var.Value?.ToString(), out var traitType);
                        characterTrait.TraitType = traitType;
                        break;
                    case "show_in_combat":
                        characterTrait.ShowInCombat = (bool)var.Value;
                        break;
                    case "slot":
                        characterTrait.CharacterSlot = ModDataStorage.Mod.IdeaSlots.FirstOrDefault(isl => isl.Id.ToString() == var.Value.ToString());
                        break;
                    case "specialist_advisor_trait":
                        characterTrait.SpecialistAdvisorTrait = ModDataStorage.Mod.CharacterTraits.FirstOrDefault(ct => ct.Id.ToString() == var.Value.ToString());
                        break;
                    case "expert_advisor_trait":
                        characterTrait.ExpertAdvisorTrait = ModDataStorage.Mod.CharacterTraits.FirstOrDefault(ct => ct.Id.ToString() == var.Value.ToString());
                        break;
                    case "genius_advisor_trait":
                        characterTrait.GeniusAdvisorTrait = ModDataStorage.Mod.CharacterTraits.FirstOrDefault(ct => ct.Id.ToString() == var.Value.ToString());
                        break;
                    case "enable_ability":
                        //todo: implement ability handling
                        break;
                    case "mutually_exclusive":
                        characterTrait.MutuallyExclusives = new List<CharacterTraitConfig>();
                        characterTrait.MutuallyExclusives.AddRange(var.Value.ToString().Split(',').Select(me => ModDataStorage.Mod.CharacterTraits.FirstOrDefault(ct => ct.Id.ToString() == me.Trim())));
                        break;
                    case "parent":
                        characterTrait.Parents = new List<CharacterTraitConfig>();
                        characterTrait.Parents.AddRange(var.Value.ToString().Split(',').Select(p => ModDataStorage.Mod.CharacterTraits.FirstOrDefault(ct => ct.Id.ToString() == p.Trim())));
                        break;
                    case "num_parents_needed":
                        characterTrait.NumParentsRequired = (int)var.Value;
                        break;
                    case "gui_row":
                        characterTrait.GuiRow = (int)var.Value;
                        break;
                    case "gui_column":
                        characterTrait.GuiColumn = (int)var.Value;
                        break;
                    case "cost":
                        characterTrait.Cost = (double)var.Value;
                        break;
                    case "gain_xp_on_spotting":
                        characterTrait.GainXpOnSpotting = (double)var.Value;
                        break;
                    default:
                        ModifierDefinitionConfig modifierDefinitionConfig = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                        if (modifierDefinitionConfig != null)
                        {
                            characterTrait.Modifiers.Add(modifierDefinitionConfig, var.Value);
                        }
                        else if (var.Name.EndsWith("skill"))
                        {
                            switch(var.Name)
                            {
                                case "skill":
                                    characterTrait.SkillTypes.Add(CharacterSkillType.Skill, (int)var.Value);
                                    break;
                                case "attack_skill":
                                    characterTrait.SkillTypes.Add(CharacterSkillType.Attack, (int)var.Value);
                                    break;
                                case "defense_skill":
                                    characterTrait.SkillTypes.Add(CharacterSkillType.Defense, (int)var.Value);
                                    break;
                                case "planning_skill":
                                    characterTrait.SkillTypes.Add(CharacterSkillType.Planning, (int)var.Value);
                                    break;
                                case "logistics_skill":
                                    characterTrait.SkillTypes.Add(CharacterSkillType.Logistics, (int)var.Value);
                                    break;
                                case "maneuvering_skill":
                                    characterTrait.SkillTypes.Add(CharacterSkillType.Maneuvering, (int)var.Value);
                                    break;
                                
                            }
                        }
                        else
                        {
                            Logger.AddLog($"Unknown var found: {var.Name} when trait parsing {bracket.Name}.");
                        }
                        
                        break;

                }
            }
            foreach(Bracket br in bracket.SubBrackets)
            {
                switch (br.Name)
                {
                    case "allowed":
                        characterTrait.Allowed = br.ToString(); //todo: implement triggers parsing
                        break;
                    case "ai_will_do":
                        characterTrait.AiWillDo = br.ToString(); //todo: implement ai will do type handling, check https://hoi4.paradoxwikis.com/AI_modding#AI_will_do
                        break;
                    case "unit_type":
                        foreach (Var var in br.SubVars)
                        {
                            if (var.Name != "type")
                            {
                                Logger.AddLog($"Unknown var found: {var.Name} when unit_type parsing {bracket.Name}.");
                                continue;
                            } 
                            RegimentConfig regiment = ModDataStorage.Mod.Regiments.FirstOrDefault(r => r.Id.ToString() == var.Value.ToString());
                            if (regiment != null)
                            {
                                characterTrait.UnitType.Add(regiment);
                            }
                        }
                        break;
                    case "unit_trigger":
                        characterTrait.UnitTrigger = br.ToString(); //todo: implement triggers parsing
                        break;
                    case "on_add":
                        characterTrait.OnAdd = br.ToString(); //todo: implement effects parsing
                        break;
                    case "on_remove":
                        characterTrait.OnRemove = br.ToString(); //todo: implement effects parsing
                        break;
                    case "daily_effect":
                        characterTrait.DailyEffect = br.ToString(); //todo: implement effects parsing
                        break;
                    case "modifier":
                        foreach (Var var in br.SubVars)
                        {
                            ModifierDefinitionConfig modifierDefinitionConfig = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                            if (modifierDefinitionConfig != null)
                            {
                                characterTrait.ArmyComanderModifiers.Add(modifierDefinitionConfig, var.Value);
                            }
                        }
                        break;
                    case "non_shared_modifier":
                        foreach (Var var in br.SubVars)
                        {
                            ModifierDefinitionConfig modifierDefinitionConfig = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                            if (modifierDefinitionConfig != null)
                            {
                                characterTrait.NonSharedModifiers.Add(modifierDefinitionConfig, var.Value);
                            }
                        }
                        break;
                    case "corps_commander_modifier":
                        foreach (Var var in br.SubVars)
                        {
                            ModifierDefinitionConfig modifierDefinitionConfig = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                            if (modifierDefinitionConfig != null)
                            {
                                characterTrait.CorpsCommanderModifiers.Add(modifierDefinitionConfig, var.Value);
                            }
                        }
                        break;
                    case "field_marshal_modifier":
                        foreach (Var var in br.SubVars)
                        {
                            ModifierDefinitionConfig modifierDefinitionConfig = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                            if (modifierDefinitionConfig != null)
                            {
                                characterTrait.FieldMarshalModifiers.Add(modifierDefinitionConfig, var.Value);
                            }
                        }
                        break;
                    case "sub_unit_modifier":
                        foreach (Var var in br.SubVars)
                        {
                            ModifierDefinitionConfig modifierDefinitionConfig = ModDataStorage.Mod.ModifierDefinitions.FirstOrDefault(m => m.Id.ToString() == var.Name);
                            if (modifierDefinitionConfig != null)
                            {
                                characterTrait.SubUnitModifiers.Add(modifierDefinitionConfig, var.Value);
                            }
                        }
                        break;
                    case "prerequisites":
                        //todo: implement triggers parsing
                        characterTrait.Prerequisites = br.ToString();
                        break;
                    case "gain_xp":
                        //todo: implement triggers parsing
                        characterTrait.GainXp = br.ToString();
                        break;
                    case "trait_xp_factor":
                        //todo: implement triggers parsing
                        characterTrait.TraitXpFactor = br.ToString();
                        break;

                }
            }
            foreach (HoiArray array in bracket.Arrays)
            {
                switch(array.Name)
                {
                    case "type":
                        foreach (var value in array.Values)
                        {
                            if (Enum.TryParse<CharacterType>(value.ToString(), out var characterType))
                            {
                                characterTrait.CharacterTypes.Add(characterType);
                            }
                        }
                        break;
                }
            }
            return characterTrait;
        }
    }
}
