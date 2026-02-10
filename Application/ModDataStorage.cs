using Application.Composers;
using Application.Debugging;
using Application.Extensions;
using Application.Extentions;
using Application.Loaders;
using Application.utils;
using Data;
using Models.Configs;
using Models.Interfaces;
using System.Diagnostics;

namespace Application;

public static class ModDataStorage
{
    public static ModConfig Mod = new();
    public static LocalisationRegistry Localisation = new();
    public static List<IError> CsvErrors = new();
    public static List<IError> TxtErrors = new();

    public static void ComposeMod(Action<int, int, string>? progressCallback = null)
    {
        var steps = new List<(Action action, string locKey)>
        {
            (() => { Mod = new ModConfig(); Localisation = new LocalisationRegistry(); }, "Progress.InitLocalisation"),
            (() => { Mod.Gfxes      = GfxLoader.LoadAll().ToObservableCollection();          }, "Progress.LoadingGraphics"),
            (() => { Mod.Resources  = ResourceComposer.Parse().Cast<ResourceConfig>().ToObservableCollection(); }, "Progress.LoadingResources"),
            (() => { Mod.ModifierDefinitions = ModifierDefComposer.Parse().Cast<ModifierDefinitionConfig>().ToObservableCollection(); }, "Progress.LoadingModifierDefs"),
            (() => { Mod.Ideologies = IdeologyComposer.Parse().Cast<IdeologyConfig>().ToObservableCollection(); }, "Progress.LoadingIdeologies"),
            (() => { Mod.Map.Provinces = ProvinceComposer.Parse().Cast<ProvinceConfig>().ToList(); }, "Progress.LoadingProvinces"),
            (() => { Mod.StateCathegories = StateCathegoryComposer.Parse().Cast<StateCathegoryConfig>().ToObservableCollection(); }, "Progress.LoadingStateCategories"),
            (() => { Mod.Rules = RuleComposer.Parse().Cast<RuleConfig>().ToObservableCollection(); }, "Progress.LoadingRules"),
            (() => { Mod.StaticModifiers = StaticModifierComposer.Parse().Cast<StaticModifierConfig>().ToObservableCollection(); }, "Progress.LoadingStaticModifiers"),
            (() => { Mod.DynamicModifiers = DynamicModifierComposer.Parse().Cast<DynamicModifierConfig>().ToObservableCollection(); }, "Progress.LoadingDynamicModifiers"),
          
            (() => { Mod.IdeaSlots = IdeaGroupComposer.Parse().Cast<IdeaGroupConfig>().ToObservableCollection(); }, "Progress.LoadingIdeaSlots"),
            (() => { Mod.IdeaTags  = IdeaTagComposer.Parse().Cast<IdeaTagConfig>().ToObservableCollection();  }, "Progress.LoadingIdeaTags"),
            (() => { Mod.TechTreeLedgers = TechnologyComposer.Parse().Cast<TechTreeConfig>().ToObservableCollection(); }, "Progress.LoadingTechnologies"),
            (() => { Mod.CharacterTraits = CharacterTraitComposer.Parse().Cast<CharacterTraitConfig>().ToObservableCollection(); }, "Progress.LoadingCharacterTraits"),
            (() => { Mod.Characters = CharacterComposer.Parse().Cast<CountryCharacterConfig>().ToObservableCollection(); }, "Progress.LoadingCharacters"),
            (() => { Mod.SubUnitGroups = SubUnitGroupComposer.Parse().Cast<SubUnitGroupConfig>().ToObservableCollection(); }, "Progress.LoadingSubUnitGroups"),
            (() => { Mod.SubUnits  = SubUnitComposer.Parse().Cast<SubUnitConfig>().ToObservableCollection();  }, "Progress.LoadingSubUnits"),
            (() => { Mod.OpinionModifiers = OpinionModifierComposer.Parse().Cast<OpinionModifierConfig>().ToObservableCollection(); }, "Progress.LoadingOpinionModifiers"),
            (() => { Mod.Buildings = BuildingComposer.Parse().Cast<BuildingConfig>().ToObservableCollection(); }, "Progress.LoadingBuildings"),
            (() => { Mod.Equipments = EquipmentComposer.Parse().Cast<EquipmentConfig>().ToObservableCollection(); }, "Progress.LoadingEquipments"),
            (() => { Mod.TechCategories = TechCategoryComposer.Parse().Cast<TechCategoryConfig>().ToObservableCollection(); }, "Progress.LoadingTechCategories"),
            (() => { Mod.TechTreeItems = TechTreeItemComposer.Parse().Cast<TechTreeItemConfig>().ToObservableCollection(); }, "Progress.LoadingTechTreeDefinitions"),
            (() => { Mod.Countries = CountryComposer.Parse().Cast<CountryConfig>().ToObservableCollection(); }, "Progress.LoadingCountries"),
            (() => { Mod.Map = MapComposer.Parse().FirstOrDefault() as MapConfig; }, "Progress.LoadingMap"),
            (() => { OverrideManager.HandleOverride(); }, "Progress.HandlingOverrides"),
        };

        int totalSteps = steps.Count;
        int currentStep = 0;

        foreach (var step in steps)
        {
            step.action();
            currentStep++;
            string message = StaticLocalisation.GetString(step.locKey);
            progressCallback?.Invoke(currentStep, totalSteps, message);
        }

        // ────────────────────────────────────────────────
        // Логирование через локализацию
        // ────────────────────────────────────────────────

        var customMods = Mod.ModifierDefinitions.Where(m => !m.IsCore).ToObservableCollection();

        Logger.AddLog(StaticLocalisation.GetString("Log.IdeologiesInitialized",
            Mod.Ideologies.Count,
            Mod.Ideologies.Count > 0 ? Mod.Ideologies.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.ModifierDefsInitialized",
            Mod.ModifierDefinitions.Count,
            Mod.ModifierDefinitions.Count > 0 ? Mod.ModifierDefinitions.Random().Id : "none",
            Mod.ModifierDefinitions.Any(m => !m.IsCore),
            customMods.Count > 0 ? customMods.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.GfxInitialized",
            Mod.Gfxes.Count,
            Mod.Gfxes.Count > 0 ? Mod.Gfxes.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.LocalisationInitialized",
            Localisation.OtherLocalisation.Data.Count,
            Localisation.VictoryPointsLocalisation.Data.Count));

        Logger.AddLog(StaticLocalisation.GetString("Log.ProvincesInitialized",
            Mod.Map.Provinces.Count,
            Mod.Map.Provinces.Count > 0 ? Mod.Map.Provinces.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.StateCategoriesInitialized",
            Mod.StateCathegories.Count,
            Mod.StateCathegories.Count > 0 ? Mod.StateCathegories.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.RulesInitialized",
            Mod.Rules.Count,
            Mod.Rules.Count > 0 ? Mod.Rules.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.StaticModifiersInitialized",
            Mod.StaticModifiers.Count,
            Mod.StaticModifiers.Count > 0 ? Mod.StaticModifiers.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.DynamicModifiersInitialized",
            Mod.DynamicModifiers.Count,
            Mod.DynamicModifiers.Count > 0 ? Mod.DynamicModifiers.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.IdeaSlotsInitialized",
            Mod.IdeaSlots.Count,
            Mod.IdeaSlots.Count > 0 ? Mod.IdeaSlots.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.IdeaTagsInitialized",
            Mod.IdeaTags.Count,
            Mod.IdeaTags.Count > 0 ? Mod.IdeaTags.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.TechTreeLedgersInitialized",
            Mod.TechTreeLedgers.Count,
            Mod.TechTreeLedgers.Count > 0 ? Mod.TechTreeLedgers.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.CharacterTraitsInitialized",
            Mod.CharacterTraits.Count,
            Mod.CharacterTraits.Count > 0 ? Mod.CharacterTraits.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.CharactersInitialized",
            Mod.Characters.Count,
            Mod.Characters.Count > 0 ? Mod.Characters.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.SubUnitsInitialized",
            Mod.SubUnits.Count,
            Mod.SubUnits.Count > 0 ? Mod.SubUnits.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.OpinionModifiersInitialized",
            Mod.OpinionModifiers.Count,
            Mod.OpinionModifiers.Count > 0 ? Mod.OpinionModifiers.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.CountriesInitialized",
            Mod.Countries.Count,
            Mod.Countries.Count > 0 ? Mod.Countries.Random().Id : "none"));
    }
}

