using Application.Composers;
using Application.Debugging;
using Application.Extentions;
using Application.Loaders;
using Application.utils;
using ModdingManager.classes.utils;
using Models;

namespace Application;

public static class ModDataStorage
{
    public static ModConfig Mod = new();
    public static LocalisationRegistry Localisation = new();

    private static string GetRandomId<T>(List<T> items)
    {
        if (items == null || items.Count == 0)
        {
            return "none";
        }

        var item = items.Random();
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
        {
            return "unknown";
        }

        var idValue = idProperty.GetValue(item);
        return idValue?.ToString() ?? "unknown";
    }

    public static void ComposeMod()
    {
        Mod = new ModConfig();
        Localisation = new LocalisationRegistry();
        Logger.AddLog($"Localisation Intalized:{Localisation.OtherLocalisation.Data.Count}, some rng obj:{Localisation.VictoryPointsLocalisation.Data.Count}");
        Mod.Gfxes = GfxLoader.LoadAll();
        Logger.AddLog($"GFXes Intalized:{Mod.Gfxes.Count}, some rng obj:{GetRandomId(Mod.Gfxes)}");
        Mod.ModifierDefinitions = ModifierDefComposer.Parse().Cast<ModifierDefinitionConfig>().ToList();
        var customModifiers = Mod.ModifierDefinitions.Where(m => m.IsCore == false).ToList();
        Logger.AddLog($"ModDefs Intalized:{Mod.ModifierDefinitions.Count}, some rng obj:{GetRandomId(Mod.ModifierDefinitions)}, has custom mods?:{customModifiers.Count > 0}: {GetRandomId(customModifiers)}");
        Mod.StateCathegories = StateCathegoryComposer.Parse().Cast<StateCathegoryConfig>().ToList();
        Logger.AddLog($"StateCathegories Intalized:{Mod.StateCathegories.Count}, some rng obj:{GetRandomId(Mod.StateCathegories)}");
        Mod.Rules = RuleComposer.Parse().Cast<RuleConfig>().ToList();
        Logger.AddLog($"Rules Intalized:{Mod.Rules.Count}, some rng obj:{GetRandomId(Mod.Rules)}");
        Mod.StaticModifiers = StaticModifierComposer.Parse().Cast<StaticModifierConfig>().ToList();
        Logger.AddLog($"StaticModifiers Intalized:{Mod.StaticModifiers.Count}, some rng obj:{GetRandomId(Mod.StaticModifiers)}");
        Mod.DynamicModifiers = DynamicModifierComposer.Parse().Cast<DynamicModifierConfig>().ToList();
        Logger.AddLog($"DynamicModifiers Intalized:{Mod.DynamicModifiers.Count}, some rng obj:{GetRandomId(Mod.DynamicModifiers)}");
        Mod.IdeaSlots = IdeaSlotComposer.Parse().Cast<IdeaSlotConfig>().ToList();
        Logger.AddLog($"IdeaSlots Intalized:{Mod.IdeaSlots.Count}, some rng obj:{GetRandomId(Mod.IdeaSlots)}");
        Mod.Ideas = IdeaComposer.Parse().Cast<IdeaConfig>().ToList();
        Logger.AddLog($"Ideas Intalized:{Mod.Ideas.Count}, some rng obj:{GetRandomId(Mod.Ideas)}");
        Mod.IdeaTags = IdeaTagComposer.Parse().Cast<IdeaTagConfig>().ToList();
        Logger.AddLog($"IdeaTags Intalized:{Mod.IdeaTags.Count}, some rng obj:{GetRandomId(Mod.IdeaTags)}");

        Mod.Ideologies = IdeologyComposer.Parse().Cast<IdeologyConfig>().ToList();
        Logger.AddLog($"Ideologies Intalized:{Mod.Ideologies.Count}, some rng obj:{GetRandomId(Mod.Ideologies)}");

        Mod.TechTreeLedgers = TechnologyComposer.Parse().Cast<TechTreeConfig>().ToList();
        Logger.AddLog($"TechTreeLedgers Intalized:{Mod.TechTreeLedgers.Count}, some rng obj:{GetRandomId(Mod.TechTreeLedgers)}");
        Mod.CharacterTraits = CharacterTraitComposer.Parse().Cast<CharacterTraitConfig>().ToList();
        Logger.AddLog($"CharacterTraits Intalized:{Mod.CharacterTraits.Count}, some rng obj:{GetRandomId(Mod.CharacterTraits)}");
        Mod.Characters = CharacterComposer.Parse().Cast<CountryCharacterConfig>().ToList();
        Logger.AddLog($"Characters Intalized:{Mod.Characters.Count}, some rng obj:{GetRandomId(Mod.Characters)}");


        //Logger.AddLog($"Ideas Intalized:{Mod.Ideas.Count}, some rng obj:{Mod.Ideas.Random().Id.ToString()}");
        //Mod.Regiments = RegimentComposer.Parse().Cast<RegimentConfig>().ToList();


        //Mod.OpinionModifiers = OpinionModifierComposer.Parse().Cast<OpinionModifierConfig>().ToList();



        //Mod.Countries = CountryComposer.Parse().Cast<CountryConfig>().ToList();
        //Mod.Map = MapComposer.Parse().FirstOrDefault() as MapConfig;
    }
}
