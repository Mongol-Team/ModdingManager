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
    public class ModdingManagerSettings
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

        public static ModdingManagerSettings Instance { get; private set; }

        public static void Load()
        {
            var json = File.ReadAllText(ProgramPathes.ConfigFilePath);
            Instance = JsonSerializer.Deserialize<ModdingManagerSettings>(json);
        }
    }
}
