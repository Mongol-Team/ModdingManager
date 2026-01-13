using Application.Composers;
using Application.Debugging;
using Application.Extentions;
using Application.Loaders;
using Application.utils;
using Models.Configs;

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

    public static void ComposeMod(Action<int, int, string>? progressCallback = null)
    {
        int currentStep = 0;
        int totalSteps = 4;

        void ReportProgress(string message)
        {
            currentStep++;
            progressCallback?.Invoke(currentStep, totalSteps, message);
        }

        Mod = new ModConfig();
        Localisation = new LocalisationRegistry();
        ReportProgress("Инициализация локализации...");
        
        Mod.Gfxes = GfxLoader.LoadAll();
        ReportProgress("Загрузка графики...");
       
        Mod.ModifierDefinitions = ModifierDefComposer.Parse().Cast<ModifierDefinitionConfig>().ToList();
        ReportProgress("Загрузка определений модификаторов...");
        var customMods = Mod.ModifierDefinitions.Where(m => m.IsCore == false).ToList();
       
        Mod.Ideologies = IdeologyComposer.Parse().Cast<IdeologyConfig>().ToList();
        ReportProgress("Загрузка идеологий...");
        
        OverrideManager.HandleOverride();
        Logger.AddLog($"Ideologies Intalized:{Mod.Ideologies.Count}, some rng obj:{(Mod.Ideologies.Count > 0 ? Mod.Ideologies.Random().Id.ToString() : "none")}");
        Logger.AddLog($"ModDefs Intalized:{Mod.ModifierDefinitions.Count}, some rng obj:{(Mod.ModifierDefinitions.Count > 0 ? Mod.ModifierDefinitions.Random().Id.ToString() : "none")}, has custom mods?:{Mod.ModifierDefinitions.Any(m => m.IsCore == false)}: {(customMods.Count > 0 ? customMods.Random().Id.ToString() : "none")}");
        Logger.AddLog($"GFXes Intalized:{Mod.Gfxes.Count}, some rng obj:{(Mod.Gfxes.Count > 0 ? Mod.Gfxes.Random().Id.ToString() : "none")}");
        Logger.AddLog($"Localisation Intalized:{Localisation.OtherLocalisation.Data.Count}, some rng obj:{Localisation.VictoryPointsLocalisation.Data.Count}");




        //Mod.Map.Provinces = ProvinceComposer.Parse().Cast<ProvinceConfig>().ToList();
        //ReportProgress("Загрузка провинций...");
        //Logger.AddLog($"Provinces Intalized:{Mod.Map.Provinces.Count}, some rng obj:{(Mod.Map.Provinces.Count > 0 ? Mod.Map.Provinces.Random().Id.ToString() : "none")}");

        //Mod.StateCathegories = StateCathegoryComposer.Parse().Cast<StateCathegoryConfig>().ToList();
        //ReportProgress("Загрузка категорий штатов...");
        //Logger.AddLog($"StateCathegories Intalized:{Mod.StateCathegories.Count}, some rng obj:{(Mod.StateCathegories.Count > 0 ? Mod.StateCathegories.Random().Id.ToString() : "none")}");

        //Mod.Rules = RuleComposer.Parse().Cast<RuleConfig>().ToList();
        //ReportProgress("Загрузка правил...");
        //Logger.AddLog($"Rules Intalized:{Mod.Rules.Count}, some rng obj:{(Mod.Rules.Count > 0 ? Mod.Rules.Random().Id.ToString() : "none")}");

        //Mod.StaticModifiers = StaticModifierComposer.Parse().Cast<StaticModifierConfig>().ToList();
        //ReportProgress("Загрузка статических модификаторов...");
        //Logger.AddLog($"StaticModifiers Intalized:{Mod.StaticModifiers.Count}, some rng obj:{(Mod.StaticModifiers.Count > 0 ? Mod.StaticModifiers.Random().Id.ToString() : "none")}");

        //Mod.DynamicModifiers = DynamicModifierComposer.Parse().Cast<DynamicModifierConfig>().ToList();
        //ReportProgress("Загрузка динамических модификаторов...");
        //Logger.AddLog($"DynamicModifiers Intalized:{Mod.DynamicModifiers.Count}, some rng obj:{(Mod.DynamicModifiers.Count > 0 ? Mod.DynamicModifiers.Random().Id.ToString() : "none")}");

        //Mod.IdeaSlots = IdeaGroupComposer.Parse().Cast<IdeaGroupConfig>().ToList();
        //ReportProgress("Загрузка слотов идей...");
        //Logger.AddLog($"IdeaSlots Intalized:{Mod.IdeaSlots.Count}, some rng obj:{(Mod.IdeaSlots.Count > 0 ? Mod.IdeaSlots.Random().Id.ToString() : "none")}");

        //Mod.Ideas = IdeaComposer.Parse().Cast<IdeaConfig>().ToList();
        //ReportProgress("Загрузка идей...");
        //Logger.AddLog($"Ideas Intalized:{Mod.Ideas.Count}, some rng obj:{(Mod.Ideas.Count > 0 ? Mod.Ideas.Random().Id.ToString() : "none")}");

        //Mod.IdeaTags = IdeaTagComposer.Parse().Cast<IdeaTagConfig>().ToList();
        //ReportProgress("Загрузка тегов идей...");
        //Logger.AddLog($"IdeaTags Intalized:{Mod.IdeaTags.Count}, some rng obj:{GetRandomId(Mod.IdeaTags)}");

        //Mod.TechTreeLedgers = TechnologyComposer.Parse().Cast<TechTreeConfig>().ToList();
        //ReportProgress("Загрузка технологий...");
        //Logger.AddLog($"TechTreeLedgers Intalized:{Mod.TechTreeLedgers.Count}, some rng obj:{(Mod.Ideologies.Count > 0 ? Mod.Ideologies.Random().Id.ToString() : "none")}");

        //Mod.CharacterTraits = CharacterTraitComposer.Parse().Cast<CharacterTraitConfig>().ToList();
        //ReportProgress("Загрузка черт персонажей...");
        //Logger.AddLog($"CharacterTraits Intalized:{Mod.CharacterTraits.Count}, some rng obj:{(Mod.CharacterTraits.Count > 0 ? Mod.CharacterTraits.Random().Id.ToString() : "none")}");

        //Mod.Characters = CharacterComposer.Parse().Cast<CountryCharacterConfig>().ToList();
        //ReportProgress("Загрузка персонажей...");
        //Logger.AddLog($"Characters Intalized:{Mod.Characters.Count}, some rng obj:{(Mod.Characters.Count > 0 ? Mod.Characters.Random().Id.ToString() : "none")}");

        //Mod.SubUnitGroups = SubUnitGroupComposer.Parse().Cast<SubUnitGroupConfig>().ToList();
        //ReportProgress("Загрузка групп подразделений...");

        //Mod.SubUnits = SubUnitComposer.Parse().Cast<SubUnitConfig>().ToList();
        //ReportProgress("Загрузка подразделений...");
        //Logger.AddLog($"SubUnitComposer Intalized:{Mod.SubUnits.Count}, some rng obj:{(Mod.SubUnits.Count > 0 ? Mod.SubUnits.Random().Id.ToString() : "none")}");

        //Mod.OpinionModifiers = OpinionModifierComposer.Parse().Cast<OpinionModifierConfig>().ToList();
        //ReportProgress("Загрузка модификаторов мнений...");
        //Logger.AddLog($"OpinionMods Intalized:{Mod.OpinionModifiers.Count}, some rng obj:{(Mod.OpinionModifiers.Count > 0 ? Mod.OpinionModifiers.Random().Id.ToString() : "none")}");

        //Mod.Countries = CountryComposer.Parse().Cast<CountryConfig>().ToList();
        //ReportProgress("Загрузка стран...");
        //Logger.AddLog($"Countries Intalized:{Mod.Countries.Count}, some rng obj:{(Mod.Countries.Count > 0 ? Mod.Countries.Random().Id.ToString() : "none")}");

        //Mod.Map = MapComposer.Parse().FirstOrDefault() as MapConfig;
        //ReportProgress("Загрузка карты...");
        //Logger.AddLog($"Map Intalized: \nProvinces:{Mod.Map.Provinces.Count} \nStates:{Mod.Map.States.Count} \nSRegions:{Mod.Map.StrategicRegions.Count}");
    }
}

