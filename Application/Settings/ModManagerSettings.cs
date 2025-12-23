using ModdingManager.classes.utils;
using Application.Utils.Pathes;
using Application.Utils;
using Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Settings
{
    public class ModManagerSettings
    {
        [JsonInclude]
        public string ModDirectory { get; private set; }
        [JsonInclude]
        public bool IsDebugRunning { get; private set; }
        [JsonInclude]
        public int MaxPercentForParallelUsage { get; private set; }
        [JsonInclude]
        public string GameDirectory { get; private set; }
        [JsonInclude]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Language CurrentLanguage { get; private set; }
        [JsonInclude]
        public List<string> RecentProjects { get; private set; }

        public static ModManagerSettings Instance { get; private set; }

        public static void Load()
        {
            var oldJsonPath = Path.Combine(AppContext.BaseDirectory, "Program.json");
            
            if (File.Exists(oldJsonPath))
            {
                MigrateFromJson(oldJsonPath);
            }
            
            MigrateDirJson();
            
            if (File.Exists(ProgramPathes.ConfigFilePath))
            {
                var config = ConfigFileParser.ParseConfigFile(ProgramPathes.ConfigFilePath);
                var settings = new ModManagerSettings();
                
                typeof(ModManagerSettings).GetProperty(nameof(ModDirectory))?.SetValue(settings, 
                    config.TryGetValue("ModDirectory", out var modDir) ? modDir : string.Empty);
                typeof(ModManagerSettings).GetProperty(nameof(GameDirectory))?.SetValue(settings, 
                    config.TryGetValue("GameDirectory", out var gameDir) ? gameDir : string.Empty);
                typeof(ModManagerSettings).GetProperty(nameof(IsDebugRunning))?.SetValue(settings, 
                    config.TryGetValue("IsDebugRunning", out var debug) && bool.TryParse(debug, out var debugVal) ? debugVal : false);
                typeof(ModManagerSettings).GetProperty(nameof(MaxPercentForParallelUsage))?.SetValue(settings, 
                    config.TryGetValue("MaxPercentForParallelUsage", out var maxPercent) && int.TryParse(maxPercent, out var maxVal) ? maxVal : 50);
                typeof(ModManagerSettings).GetProperty(nameof(CurrentLanguage))?.SetValue(settings, 
                    config.TryGetValue("CurrentLanguage", out var lang) && Enum.TryParse<Language>(lang, true, out var langVal) ? langVal : Language.english);
                typeof(ModManagerSettings).GetProperty(nameof(RecentProjects))?.SetValue(settings, 
                    config.TryGetValue("RecentProjects", out var projects) ? ConfigFileParser.ParseList(projects) : new List<string>());
                
                Instance = settings;
            }
            else
            {
                var settings = new ModManagerSettings();
                typeof(ModManagerSettings).GetProperty(nameof(ModDirectory))?.SetValue(settings, string.Empty);
                typeof(ModManagerSettings).GetProperty(nameof(GameDirectory))?.SetValue(settings, string.Empty);
                typeof(ModManagerSettings).GetProperty(nameof(IsDebugRunning))?.SetValue(settings, false);
                typeof(ModManagerSettings).GetProperty(nameof(MaxPercentForParallelUsage))?.SetValue(settings, 50);
                typeof(ModManagerSettings).GetProperty(nameof(CurrentLanguage))?.SetValue(settings, Language.english);
                typeof(ModManagerSettings).GetProperty(nameof(RecentProjects))?.SetValue(settings, new List<string>());
                Instance = settings;
            }
        }
        
        private static void MigrateFromJson(string jsonPath)
        {
            try
            {
                var json = File.ReadAllText(jsonPath);
                var oldSettings = JsonSerializer.Deserialize<ModManagerSettings>(json);
                
                if (oldSettings != null)
                {
                    var config = new Dictionary<string, string>
                    {
                        ["ModDirectory"] = oldSettings.ModDirectory ?? string.Empty,
                        ["GameDirectory"] = oldSettings.GameDirectory ?? string.Empty,
                        ["IsDebugRunning"] = oldSettings.IsDebugRunning.ToString(),
                        ["MaxPercentForParallelUsage"] = oldSettings.MaxPercentForParallelUsage.ToString(),
                        ["CurrentLanguage"] = oldSettings.CurrentLanguage.ToString(),
                        ["RecentProjects"] = ConfigFileParser.SerializeList(oldSettings.RecentProjects ?? new List<string>())
                    };
                    
                    ConfigFileParser.WriteConfigFile(ProgramPathes.ConfigFilePath, config);
                    
                    File.Move(jsonPath, jsonPath + ".backup");
                }
            }
            catch
            {
            }
        }
        
        public static void MigrateDirJson()
        {
            var oldDirJsonPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Application", "data", "dir.json");
            oldDirJsonPath = Path.GetFullPath(oldDirJsonPath);
            
            if (File.Exists(oldDirJsonPath))
            {
                try
                {
                    var json = File.ReadAllText(oldDirJsonPath);
                    var dirData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (dirData != null)
                    {
                        var config = new Dictionary<string, string>();
                        if (dirData.TryGetValue("GamePath", out var gamePath))
                            config["GamePath"] = gamePath;
                        if (dirData.TryGetValue("ModPath", out var modPath))
                            config["ModPath"] = modPath;
                        
                        ConfigFileParser.WriteConfigFile(ProgramPathes.DirConfigPath, config);
                        File.Move(oldDirJsonPath, oldDirJsonPath + ".backup");
                    }
                }
                catch
                {
                }
            }
        }

        public static void Save(string modDirectory, string gameDirectory)
        {
            var settings = new ModManagerSettings();
            typeof(ModManagerSettings).GetProperty(nameof(ModDirectory))?.SetValue(settings, modDirectory);
            typeof(ModManagerSettings).GetProperty(nameof(GameDirectory))?.SetValue(settings, gameDirectory);
            
            if (Instance != null)
            {
                typeof(ModManagerSettings).GetProperty(nameof(IsDebugRunning))?.SetValue(settings, Instance.IsDebugRunning);
                typeof(ModManagerSettings).GetProperty(nameof(MaxPercentForParallelUsage))?.SetValue(settings, Instance.MaxPercentForParallelUsage);
                typeof(ModManagerSettings).GetProperty(nameof(CurrentLanguage))?.SetValue(settings, Instance.CurrentLanguage);
                typeof(ModManagerSettings).GetProperty(nameof(RecentProjects))?.SetValue(settings, Instance.RecentProjects ?? new List<string>());
            }
            else
            {
                typeof(ModManagerSettings).GetProperty(nameof(IsDebugRunning))?.SetValue(settings, false);
                typeof(ModManagerSettings).GetProperty(nameof(MaxPercentForParallelUsage))?.SetValue(settings, 50);
                typeof(ModManagerSettings).GetProperty(nameof(CurrentLanguage))?.SetValue(settings, Language.english);
                typeof(ModManagerSettings).GetProperty(nameof(RecentProjects))?.SetValue(settings, new List<string>());
            }

            var config = new Dictionary<string, string>
            {
                ["ModDirectory"] = settings.ModDirectory ?? string.Empty,
                ["GameDirectory"] = settings.GameDirectory ?? string.Empty,
                ["IsDebugRunning"] = settings.IsDebugRunning.ToString(),
                ["MaxPercentForParallelUsage"] = settings.MaxPercentForParallelUsage.ToString(),
                ["CurrentLanguage"] = settings.CurrentLanguage.ToString(),
                ["RecentProjects"] = ConfigFileParser.SerializeList(settings.RecentProjects ?? new List<string>())
            };
            
            ConfigFileParser.WriteConfigFile(ProgramPathes.ConfigFilePath, config);
            Instance = settings;
        }

        public static void AddRecentProject(string projectPath)
        {
            if (Instance == null) Load();
            
            var recentProjects = Instance.RecentProjects ?? new List<string>();
            if (recentProjects.Contains(projectPath))
            {
                recentProjects.Remove(projectPath);
            }
            recentProjects.Insert(0, projectPath);
            
            if (recentProjects.Count > 10)
            {
                recentProjects.RemoveAt(recentProjects.Count - 1);
            }
            
            typeof(ModManagerSettings).GetProperty(nameof(RecentProjects))?.SetValue(Instance, recentProjects);
            Save(Instance.ModDirectory, Instance.GameDirectory);
        }

        public static void SaveGameDirectory(string gameDirectory)
        {
            if (Instance == null) Load();
            Save(Instance?.ModDirectory ?? string.Empty, gameDirectory);
        }
    }
}
