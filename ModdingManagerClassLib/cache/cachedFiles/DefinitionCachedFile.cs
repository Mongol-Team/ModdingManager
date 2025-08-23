using ModdingManagerClassLib;
using ModdingManagerModels;
using System.Drawing;

namespace ModdingManager.classes.cache.cachedFiles
{
    public class DefinitionCachedFile : CachedFile
    {
        public List<string> DefinitionLines { get; private set; } = new List<string>();

        public DefinitionCachedFile(string filePath) : base(filePath)
        {
            Content = File.ReadAllText(filePath);
            ParseContent();
        }
        public DefinitionCachedFile(string filePath, string content = null, bool isDirty = false) : base(filePath, content, isDirty)
        {
            FilePath = filePath;
            Content = content ?? File.ReadAllText(filePath);
            IsDirty = isDirty;
            LastWriteTime = File.GetLastWriteTime(filePath);
            ParseContent();
        }
        private void ParseContent()
        {
            DefinitionLines = Content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            UpdateIndexTime();
        }

        public override void RefreshContent()
        {
            base.RefreshContent();
            ParseContent();
        }

        public override void SaveToFile()
        {
            if (IsDirty)
            {
                Content = string.Join(Environment.NewLine, DefinitionLines);
                base.SaveToFile();
            }
        }

        /// <summary>
        /// Updates province definition in the cache. Logs errors if required properties are missing.
        /// </summary>
        /// <param name="province">Province configuration to update</param>
        public void SetProvinceDefinition(ProvinceConfig province)
        {
            // Validate required properties
            if (province.Id <= 0)
            {
                Debugger.Instance.LogMessage($"⚠️ Failed to set province definition: Invalid ID {province.Id}");
                return;
            }

            if (province.Color == null)
            {
                Debugger.Instance.LogMessage($"⚠️ Failed to set province definition for {province.Id}: Color is null");
                return;
            }

            if (string.IsNullOrEmpty(province.Type))
            {
                Debugger.Instance.LogMessage($"⚠️ Failed to set province definition for {province.Id}: Type is null or empty");
                return;
            }

            if (string.IsNullOrEmpty(province.Terrain))
            {
                Debugger.Instance.LogMessage($"⚠️ Failed to set province definition for {province.Id}: Terrain is null or empty");
                return;
            }

            // Build definition line
            string newLine = $"{province.Id};{province.Color.R};{province.Color.G};{province.Color.B};" +
                           $"{province.Type};{province.IsCoastal.ToString().ToLower()};{province.Terrain};{province.ContinentId}";

            // Find and replace existing line
            bool found = false;
            for (int i = 0; i < DefinitionLines.Count; i++)
            {
                var parts = DefinitionLines[i].Split(';');
                if (parts.Length > 0 && int.TryParse(parts[0], out int id) && id == province.Id)
                {
                    DefinitionLines[i] = newLine;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debugger.Instance.LogMessage($"⚠️ Province definition not found for ID {province.Id}, use AddProvinceDefinition instead");
                return;
            }

            MarkDirty();
            Debugger.Instance.LogMessage($"✓ Updated province definition for ID {province.Id}");
        }

        /// <summary>
        /// Retrieves province definition by ID. Returns null if not found.
        /// </summary>
        /// <param name="id">Province ID to search for</param>
        /// <returns>ProvinceConfig or null if not found</returns>
        public ProvinceConfig GetProvinceDefinition(int id)
        {
            foreach (var line in DefinitionLines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 8 && int.TryParse(parts[0], out int provinceId) && provinceId == id)
                {
                    try
                    {
                        return new ProvinceConfig
                        {
                            Id = provinceId,
                            Color = Color.FromArgb(
                                int.Parse(parts[1]),
                                int.Parse(parts[2]),
                                int.Parse(parts[3])),
                            Type = parts[4],
                            IsCoastal = bool.Parse(parts[5]),
                            Terrain = parts[6],
                            ContinentId = int.Parse(parts[7])
                        };
                    }
                    catch (Exception ex)
                    {
                        Debugger.Instance.LogMessage($"⚠️ Failed to parse province definition for ID {id}: {ex.Message}");
                        return null;
                    }
                }
            }

            Debugger.Instance.LogMessage($"⚠️ Province definition not found for ID {id}");
            return null;
        }

        /// <summary>
        /// Adds new province definition to the cache
        /// </summary>
        /// <param name="province">Province configuration to add</param>
        public void AddProvinceDefinition(ProvinceConfig province)
        {
            // Validate required properties
            if (province.Id <= 0)
            {
                Debugger.Instance.LogMessage($"⚠️ Failed to add province definition: Invalid ID {province.Id}");
                return;
            }

            // Check if already exists
            foreach (var line in DefinitionLines)
            {
                var parts = line.Split(';');
                if (parts.Length > 0 && int.TryParse(parts[0], out int existingId) && existingId == province.Id)
                {
                    Debugger.Instance.LogMessage($"⚠️ Province definition already exists for ID {province.Id}, use SetProvinceDefinition instead");
                    return;
                }
            }

            // Build and add new line
            string newLine = $"{province.Id};{province.Color.R};{province.Color.G};{province.Color.B};" +
                           $"{province.Type};{province.IsCoastal.ToString().ToLower()};{province.Terrain};{province.ContinentId}";

            DefinitionLines.Add(newLine);
            MarkDirty();
            Debugger.Instance.LogMessage($"✓ Added new province definition for ID {province.Id}");
        }
    }
}
