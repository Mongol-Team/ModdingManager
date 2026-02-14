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
        var steps = new List<(Action action, string locKey)>();
        steps.Add((() => { Mod = new ModConfig(); Localisation = new LocalisationRegistry(); }, "Progress.InitLocalisation"));
        steps.Add((() => { Mod.Gfxes = GfxLoader.LoadAll().ToObservableCollection(); }, "Progress.LoadingGraphics"));
        steps.Add((() => { Mod.Resources = ResourceComposer.Parse().ToObservableCollection(); }, "Progress.LoadingResources"));
        steps.Add((() => { Mod.ModifierDefinitions = ModifierDefComposer.Parse().ToObservableCollection(); }, "Progress.LoadingModifierDefs"));
        steps.Add((() => { Mod.Buildings = BuildingComposer.Parse().ToObservableCollection(); }, "Progress.LoadingBuildings"));
        steps.Add((() => { Mod.Ideologies = IdeologyComposer.Parse().ToObservableCollection(); }, "Progress.LoadingIdeologies"));
        steps.Add((() => { Mod.Map.Provinces = ProvinceComposer.Parse().ToList(); }, "Progress.LoadingProvinces"));
        steps.Add((() => { Mod.StateCathegories = StateCathegoryComposer.Parse().ToObservableCollection(); }, "Progress.LoadingStateCategories"));
        steps.Add((() => { Mod.Rules = RuleComposer.Parse().ToObservableCollection(); }, "Progress.LoadingRules"));
        steps.Add((() => { Mod.SubUnits = SubUnitComposer.Parse().ToObservableCollection(); }, "Progress.LoadingSubUnits"));
        steps.Add((() => { Mod.Ideas = IdeaComposer.Parse().ToObservableCollection(); }, "Progress.LoadingIdeas"));
        steps.Add((() => { Mod.IdeaSlots = IdeaGroupComposer.Parse().ToObservableCollection(); }, "Progress.LoadingIdeaSlots"));
        steps.Add((() => { Mod.StaticModifiers = StaticModifierComposer.Parse().ToObservableCollection(); }, "Progress.LoadingStaticModifiers"));
        steps.Add((() => { Mod.DynamicModifiers = DynamicModifierComposer.Parse().ToObservableCollection(); }, "Progress.LoadingDynamicModifiers"));
        steps.Add((() => { Mod.IdeaTags = IdeaTagComposer.Parse().ToObservableCollection(); }, "Progress.LoadingIdeaTags"));
        steps.Add((() => { Mod.TechTreeLedgers = TechnologyComposer.Parse().ToObservableCollection(); }, "Progress.LoadingTechnologies"));
        steps.Add((() => { Mod.CharacterTraits = CharacterTraitComposer.Parse().ToObservableCollection(); }, "Progress.LoadingCharacterTraits"));
        steps.Add((() => { Mod.Characters = CharacterComposer.Parse().ToObservableCollection(); }, "Progress.LoadingCharacters"));
        
        steps.Add((() => { Mod.OpinionModifiers = OpinionModifierComposer.Parse().ToObservableCollection(); }, "Progress.LoadingOpinionModifiers"));
        steps.Add((() => { Mod.Equipments = EquipmentComposer.Parse().ToObservableCollection(); }, "Progress.LoadingEquipments"));
        steps.Add((() => { Mod.TechCategories = TechCategoryComposer.Parse().ToObservableCollection(); }, "Progress.LoadingTechCategories"));
        steps.Add((() => { Mod.TechTreeItems = TechTreeItemComposer.Parse().ToObservableCollection(); }, "Progress.LoadingTechTreeDefinitions"));
        steps.Add((() => { Mod.Countries = CountryComposer.Parse().ToObservableCollection(); }, "Progress.LoadingCountries"));
        steps.Add((() => { Mod.Map = MapComposer.Parse() as MapConfig; }, "Progress.LoadingMap"));
        steps.Add((() => { OverrideManager.HandleOverride(); }, "Progress.HandlingOverrides"));

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
            Mod.Ideologies.Count > 0 ? Mod.Ideologies.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.ModifierDefsInitialized",
            Mod.ModifierDefinitions.Count,
            Mod.ModifierDefinitions.Count > 0 ? Mod.ModifierDefinitions.FileEntitiesToList()?.Random().Id : "none",
            Mod.ModifierDefinitions.Any(m => !m.IsCore),
            customMods.Count > 0 ? customMods.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.GfxInitialized",
            Mod.Gfxes.Count,
            Mod.Gfxes.Count > 0 ? Mod.Gfxes.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.LocalisationInitialized",
            Localisation.OtherLocalisation.Data.Count,
            Localisation.VictoryPointsLocalisation.Data.Count));

        Logger.AddLog(StaticLocalisation.GetString("Log.ProvincesInitialized",
            Mod.Map.Provinces.Count,
            Mod.Map.Provinces.Count > 0 ? Mod.Map.Provinces.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.StateCategoriesInitialized",
            Mod.StateCathegories.Count,
            Mod.StateCathegories.Count > 0 ? Mod.StateCathegories.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.RulesInitialized",
            Mod.Rules.Count,
            Mod.Rules.Count > 0 ? Mod.Rules.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.StaticModifiersInitialized",
            Mod.StaticModifiers.Count,
            Mod.StaticModifiers.Count > 0 ? Mod.StaticModifiers.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.DynamicModifiersInitialized",
            Mod.DynamicModifiers.Count,
            Mod.DynamicModifiers.Count > 0 ? Mod.DynamicModifiers.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.IdeaSlotsInitialized",
            Mod.IdeaSlots.Count,
            Mod.IdeaSlots.Count > 0 ? Mod.IdeaSlots.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.IdeaTagsInitialized",
            Mod.IdeaTags.Count,
            Mod.IdeaTags.Count > 0 ? Mod.IdeaTags.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.TechTreeLedgersInitialized",
            Mod.TechTreeLedgers.Count,
            Mod.TechTreeLedgers.Count > 0 ? Mod.TechTreeLedgers.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.CharacterTraitsInitialized",
            Mod.CharacterTraits.Count,
            Mod.CharacterTraits.Count > 0 ? Mod.CharacterTraits.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.CharactersInitialized",
            Mod.Characters.Count,
            Mod.Characters.Count > 0 ? Mod.Characters.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.SubUnitsInitialized",
            Mod.SubUnits.Count,
            Mod.SubUnits.Count > 0 ? Mod.SubUnits.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.OpinionModifiersInitialized",
            Mod.OpinionModifiers.Count,
            Mod.OpinionModifiers.Count > 0 ? Mod.OpinionModifiers.FileEntitiesToList()?.Random().Id : "none"));

        Logger.AddLog(StaticLocalisation.GetString("Log.CountriesInitialized",
            Mod.Countries.Count,
            Mod.Countries.Count > 0 ? Mod.Countries.FileEntitiesToList()?.Random().Id : "none"));
    }
}

