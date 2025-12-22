using ModdingManager.classes.utils;
using Application.Utils.Pathes;
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

        public static ModManagerSettings Instance { get; private set; }

        public static void Load()
        {
            if (File.Exists(ProgramPathes.ConfigFilePath))
            {
                var json = File.ReadAllText(ProgramPathes.ConfigFilePath);
                Instance = JsonSerializer.Deserialize<ModManagerSettings>(json);
            }
            else
            {
                var settings = new ModManagerSettings();
                typeof(ModManagerSettings).GetProperty(nameof(ModDirectory))?.SetValue(settings, string.Empty);
                typeof(ModManagerSettings).GetProperty(nameof(GameDirectory))?.SetValue(settings, string.Empty);
                typeof(ModManagerSettings).GetProperty(nameof(IsDebugRunning))?.SetValue(settings, false);
                typeof(ModManagerSettings).GetProperty(nameof(MaxPercentForParallelUsage))?.SetValue(settings, 50);
                typeof(ModManagerSettings).GetProperty(nameof(CurrentLanguage))?.SetValue(settings, Language.english);
                Instance = settings;
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
            }
            else
            {
                typeof(ModManagerSettings).GetProperty(nameof(IsDebugRunning))?.SetValue(settings, false);
                typeof(ModManagerSettings).GetProperty(nameof(MaxPercentForParallelUsage))?.SetValue(settings, 50);
                typeof(ModManagerSettings).GetProperty(nameof(CurrentLanguage))?.SetValue(settings, Language.english);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(ProgramPathes.ConfigFilePath, json);
            Instance = settings;
        }
    }
}
