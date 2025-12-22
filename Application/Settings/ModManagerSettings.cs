using ModdingManager.classes.utils;
using ModdingManagerClassLib.Utils.Pathes;
using ModdingManagerModels.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModdingManagerClassLib.Settings
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
            var json = File.ReadAllText(ProgramPathes.ConfigFilePath);
            Instance = JsonSerializer.Deserialize<ModManagerSettings>(json);
        }
    }
}
