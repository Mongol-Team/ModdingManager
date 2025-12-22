using ModdingManager.classes.utils;
using ModdingManagerClassLib.Composers;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.Loaders;
using ModdingManagerClassLib.utils;
using ModdingManagerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerClassLib
{
    public static class ModDataStorage
    {
        public static ModConfig Mod = new();
        public static LocalisationRegistry Localisation = new();

        public static void ComposeMod()
        {
            Mod = new ModConfig();
            Localisation = new LocalisationRegistry();
            Logger.AddLog($"Localisation Intalized:{Localisation.OtherLocalisation.Data.Count}, some rng obj:{Localisation.VictoryPointsLocalisation.Data.Count}");
            Mod.Gfxes = GfxLoader.LoadAll();
            Logger.AddLog($"GFXes Intalized:{Mod.Gfxes.Count}, some rng obj:{Mod.Gfxes.Random().Id.ToString()}");
            Mod.ModifierDefinitions = ModifierDefComposer.Parse().Cast<ModifierDefinitionConfig>().ToList();
            Logger.AddLog($"ModDefs Intalized:{Mod.ModifierDefinitions.Count}, some rng obj:{Mod.ModifierDefinitions.Random().Id.ToString()}, has custom mods?:{Mod.ModifierDefinitions.Any(m => m.IsCore == false)}: {Mod.ModifierDefinitions.Where(m => m.IsCore == false).ToList().Random().Id.ToString()}");
            Mod.StateCathegories = StateCathegoryComposer.Parse().Cast<StateCathegoryConfig>().ToList();
            Logger.AddLog($"StateCathegories Intalized:{Mod.StateCathegories.Count}, some rng obj:{Mod.StateCathegories.Random().Id.ToString()}");
            Mod.Rules = RuleComposer.Parse().Cast<RuleConfig>().ToList();
            Logger.AddLog($"Rules Intalized:{Mod.Rules.Count}, some rng obj:{Mod.Rules.Random().Id.ToString()}");
            Mod.StaticModifiers = StaticModifierComposer.Parse().Cast<StaticModifierConfig>().ToList();
            Logger.AddLog($"StaticModifiers Intalized:{Mod.StaticModifiers.Count}, some rng obj:{Mod.StaticModifiers.Random().Id.ToString()}");
            Mod.DynamicModifiers = DynamicModifierComposer.Parse().Cast<DynamicModifierConfig>().ToList();
            Logger.AddLog($"DynamicModifiers Intalized:{Mod.DynamicModifiers.Count}, some rng obj:{Mod.DynamicModifiers.Random().Id.ToString()}");
            Mod.IdeaSlots = IdeaSlotComposer.Parse().Cast<IdeaSlotConfig>().ToList();
            Logger.AddLog($"IdeaSlots Intalized:{Mod.IdeaSlots.Count}, some rng obj:{Mod.IdeaSlots.Random().Id.ToString()}");
            Mod.Ideas = IdeaComposer.Parse().Cast<IdeaConfig>().ToList();
            Logger.AddLog($"Ideas Intalized:{Mod.Ideas.Count}, some rng obj:{Mod.Ideas.Random().Id.ToString()}");
            Mod.IdeaTags = IdeaTagComposer.Parse().Cast<IdeaTagConfig>().ToList();
            Logger.AddLog($"IdeaTags Intalized:{Mod.IdeaTags.Count}, some rng obj:{Mod.IdeaTags.Random().Id.ToString()}");

            Mod.Ideologies = IdeologyComposer.Parse().Cast<IdeologyConfig>().ToList();
            Logger.AddLog($"Ideologies Intalized:{Mod.Ideologies.Count}, some rng obj:{Mod.Ideologies.Random().Id.ToString()}");

            Mod.TechTreeLedgers = TechnologyComposer.Parse().Cast<TechTreeConfig>().ToList();
            Logger.AddLog($"TechTreeLedgers Intalized:{Mod.Ideologies.Count}, some rng obj:{Mod.Ideologies.Random().Id.ToString()}");
            Mod.CharacterTraits = CharacterTraitComposer.Parse().Cast<CharacterTraitConfig>().ToList();
            Logger.AddLog($"CharacterTraits Intalized:{Mod.CharacterTraits.Count}, some rng obj:{Mod.CharacterTraits.Random().Id.ToString()}");
            Mod.Characters = CharacterComposer.Parse().Cast<CountryCharacterConfig>().ToList();
            Logger.AddLog($"Characters Intalized:{Mod.Characters.Count}, some rng obj:{Mod.Characters.Random().Id.ToString()}");


            //Logger.AddLog($"Ideas Intalized:{Mod.Ideas.Count}, some rng obj:{Mod.Ideas.Random().Id.ToString()}");
            //Mod.Regiments = RegimentComposer.Parse().Cast<RegimentConfig>().ToList();


            //Mod.OpinionModifiers = OpinionModifierComposer.Parse().Cast<OpinionModifierConfig>().ToList();



            //Mod.Countries = CountryComposer.Parse().Cast<CountryConfig>().ToList();
            //Mod.Map = MapComposer.Parse().FirstOrDefault() as MapConfig;
        }
    }
}
