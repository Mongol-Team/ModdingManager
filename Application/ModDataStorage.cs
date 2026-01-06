using Application.Composers;
using Application.Debugging;
using Application.Extentions;
using Application.utils;
using Models.Configs;
using System.Diagnostics;

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
        int totalSteps = 19;

        void ReportProgress(string message)
        {
            currentStep++;
            progressCallback?.Invoke(currentStep, totalSteps, message);
        }

        Mod = new ModConfig();
        var sw = Stopwatch.StartNew();
        Localisation = new LocalisationRegistry();
        sw.Stop();
        ReportProgress("Инициализация локализации...");
        Logger.AddLog($"Localisation Initialized: {Localisation.OtherLocalisation.Data.Count} items, sample: {Localisation.VictoryPointsLocalisation.Data.Count} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        //sw.Restart();
        //Mod.Gfxes = GfxLoader.LoadAll();
        //sw.Stop();
        //ReportProgress("Загрузка графики...");
        //Logger.AddLog($"GFXes Initialized: {Mod.Gfxes.Count} items, sample: {(Mod.Gfxes.Count > 0 ? Mod.Gfxes.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.ModifierDefinitions = ModifierDefComposer.Parse().Cast<ModifierDefinitionConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка определений модификаторов...");
        var customMods = Mod.ModifierDefinitions.Where(m => m.IsCore == false).ToList();
        Logger.AddLog($"ModDefs Initialized: {Mod.ModifierDefinitions.Count} items, sample: {(Mod.ModifierDefinitions.Count > 0 ? Mod.ModifierDefinitions.Random().Id.ToString() : "none")}, custom mods: {customMods.Count} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Ideologies = IdeologyComposer.Parse().Cast<IdeologyConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка идеологий...");
        Logger.AddLog($"Ideologies Initialized: {Mod.Ideologies.Count} items, sample: {(Mod.Ideologies.Count > 0 ? Mod.Ideologies.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Map.Provinces = ProvinceComposer.Parse().Cast<ProvinceConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка провинций...");
        Logger.AddLog($"Provinces Initialized: {Mod.Map.Provinces.Count} items, sample: {(Mod.Map.Provinces.Count > 0 ? Mod.Map.Provinces.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.StateCathegories = StateCathegoryComposer.Parse().Cast<StateCathegoryConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка категорий штатов...");
        Logger.AddLog($"StateCathegories Initialized: {Mod.StateCathegories.Count} items, sample: {(Mod.StateCathegories.Count > 0 ? Mod.StateCathegories.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Rules = RuleComposer.Parse().Cast<RuleConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка правил...");
        Logger.AddLog($"Rules Initialized: {Mod.Rules.Count} items, sample: {(Mod.Rules.Count > 0 ? Mod.Rules.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.StaticModifiers = StaticModifierComposer.Parse().Cast<StaticModifierConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка статических модификаторов...");
        Logger.AddLog($"StaticModifiers Initialized: {Mod.StaticModifiers.Count} items, sample: {(Mod.StaticModifiers.Count > 0 ? Mod.StaticModifiers.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.DynamicModifiers = DynamicModifierComposer.Parse().Cast<DynamicModifierConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка динамических модификаторов...");
        Logger.AddLog($"DynamicModifiers Initialized: {Mod.DynamicModifiers.Count} items, sample: {(Mod.DynamicModifiers.Count > 0 ? Mod.DynamicModifiers.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.IdeaSlots = IdeaGroupComposer.Parse().Cast<IdeaGroupConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка слотов идей...");
        Logger.AddLog($"IdeaSlots Initialized: {Mod.IdeaSlots.Count} items, sample: {(Mod.IdeaSlots.Count > 0 ? Mod.IdeaSlots.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Ideas = IdeaComposer.Parse().Cast<IdeaConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка идей...");
        Logger.AddLog($"Ideas Initialized: {Mod.Ideas.Count} items, sample: {(Mod.Ideas.Count > 0 ? Mod.Ideas.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.IdeaTags = IdeaTagComposer.Parse().Cast<IdeaTagConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка тегов идей...");
        Logger.AddLog($"IdeaTags Initialized: {Mod.IdeaTags.Count} items, sample: {GetRandomId(Mod.IdeaTags)} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.TechTreeLedgers = TechnologyComposer.Parse().Cast<TechTreeConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка технологий...");
        Logger.AddLog($"TechTreeLedgers Initialized: {Mod.TechTreeLedgers.Count} items, sample: {(Mod.TechTreeLedgers.Count > 0 ? Mod.TechTreeLedgers.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.CharacterTraits = CharacterTraitComposer.Parse().Cast<CharacterTraitConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка черт персонажей...");
        Logger.AddLog($"CharacterTraits Initialized: {Mod.CharacterTraits.Count} items, sample: {(Mod.CharacterTraits.Count > 0 ? Mod.CharacterTraits.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Characters = CharacterComposer.Parse().Cast<CountryCharacterConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка персонажей...");
        Logger.AddLog($"Characters Initialized: {Mod.Characters.Count} items, sample: {(Mod.Characters.Count > 0 ? Mod.Characters.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.SubUnitGroups = SubUnitGroupComposer.Parse().Cast<SubUnitGroupConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка групп подразделений...");
        Logger.AddLog($"SubUnitGroups Initialized: {Mod.SubUnitGroups.Count} items | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.SubUnits = SubUnitComposer.Parse().Cast<SubUnitConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка подразделений...");
        Logger.AddLog($"SubUnits Initialized: {Mod.SubUnits.Count} items, sample: {(Mod.SubUnits.Count > 0 ? Mod.SubUnits.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.OpinionModifiers = OpinionModifierComposer.Parse().Cast<OpinionModifierConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка модификаторов мнений...");
        Logger.AddLog($"OpinionModifiers Initialized: {Mod.OpinionModifiers.Count} items, sample: {(Mod.OpinionModifiers.Count > 0 ? Mod.OpinionModifiers.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Countries = CountryComposer.Parse().Cast<CountryConfig>().ToList();
        sw.Stop();
        ReportProgress("Загрузка стран...");
        Logger.AddLog($"Countries Initialized: {Mod.Countries.Count} items, sample: {(Mod.Countries.Count > 0 ? Mod.Countries.Random().Id.ToString() : "none")} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);

        sw.Restart();
        Mod.Map = MapComposer.Parse().FirstOrDefault() as MapConfig;
        sw.Stop();
        ReportProgress("Загрузка карты...");
        Logger.AddLog($"Map Initialized: Provinces: {Mod.Map.Provinces.Count}, States: {Mod.Map.States.Count}, StrategicRegions: {Mod.Map.StrategicRegions.Count} | Time: {sw.ElapsedMilliseconds}ms", ConsoleColor.Cyan);
    }
}

