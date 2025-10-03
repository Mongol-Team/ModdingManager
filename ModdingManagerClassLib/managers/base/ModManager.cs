using ModdingManager.classes.utils;
using ModdingManagerClassLib.Composers;
using ModdingManagerClassLib.Debugging;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.Loaders;
using ModdingManagerClassLib.utils;
using ModdingManagerModels;
using ModdingManagerModels.Enums;
using SixLabors.ImageSharp;
using System.Text.Json;
using System.Text.RegularExpressions;

using System.Windows;

namespace ModdingManager.managers.@base
{
    public class ModManager
    {
        public static string ModDirectory { get; 
            set; } = "";
        public static bool IsDebugRuning
        {
            get;
            set;
        } = false;
        public static string GameDirectory { get; set; } = "";
        public static string CurrentCountryTag { get; set; } = "ZOV";
        public static Language CurrentLanguage = Language.russian;

        public static ModConfig Mod = new();
        public static LocalisationRegistry Localisation;
        public ModManager()
        {
            LoadInstance();
        }
        public static void LoadInstance()
        {
            string relativePath = System.IO.Path.Combine("..", "..", "..", "data", "dir.json");
            string fullPath = System.IO.Path.GetFullPath(relativePath, AppDomain.CurrentDomain.BaseDirectory);
            Logger.LoggingLevel = 3;
            Logger.IsDebug = IsDebugRuning;
            try
            {
                string json = File.ReadAllText(fullPath);
                var path = JsonSerializer.Deserialize<PathConfig>(json);

                ModDirectory = path.ModPath;
                GameDirectory = path.GamePath;
                
                ComposeMod();
            }
            catch (Exception ex)
            {
                Logger.AddLog($"[MAIN WPF] On load exception: {ex.Message}{ex.StackTrace}");
            }
        }
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

            Mod.Ideas = IdeaComposer.Parse().Cast<IdeaConfig>().ToList();
            Logger.AddLog($"Ideas Intalized:{Mod.Ideas.Count}, some rng obj:{Mod.Ideas.Random().Id.ToString()}");
            //Mod.Regiments = RegimentComposer.Parse().Cast<RegimentConfig>().ToList();


            ////Mod.OpinionModifiers = OpinionModifierComposer.Parse().Cast<OpinionModifierConfig>().ToList();

            ////Mod.TechTreeLedgers = TechTreeComposer.Parse().Cast<TechTreeConfig>().ToList();
            ////Mod.Characters = CountryCharacterComposer.Parse().Cast<CountryCharacterConfig>().ToList();


            //Mod.Countries = CountryComposer.Parse().Cast<CountryConfig>().ToList();
            //Mod.Map = MapComposer.Parse().FirstOrDefault() as MapConfig;
        }
        public static List<string> LoadCountryFileNames()
        {
            string countriesDir = Path.Combine(ModManager.ModDirectory, "common", "country_tags");

            if (!System.IO.Directory.Exists(countriesDir))
            {
                System.Windows.MessageBox.Show("Папка 'countries' не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var filePaths = System.IO.Directory.GetFiles(countriesDir);
            var fileNamesLines = filePaths.Select(path => Path.GetFileName(path)).ToList();
            return fileNamesLines;
            
        }
        
        public static System.Drawing.Color GenerateColorFromId(int id)
        {
            byte r = (byte)((id * 53) % 255);
            byte g = (byte)((id * 97) % 255);
            byte b = (byte)((id * 151) % 255);
            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }
}