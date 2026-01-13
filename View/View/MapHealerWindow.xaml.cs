using System.Windows;

namespace View
{
    /// <summary>
    /// Логика взаимодействия для MapHealerWindow.xaml
    /// </summary>
    public partial class MapHealerWindow : Window
    {
        public MapHealerWindow()
        {
            InitializeComponent();
        }

        private void DelNoExistingProvButton_Click(object sender, RoutedEventArgs e)
        {
            //DeleteAllNonExistingProvLinks();
        }
        public static void DeleteAllNonExistingProvLinks()
        {
            //    var mapCache = ModDataStorage.Mod.MapCache;
            //    var existingProvs = new HashSet<int>(ModManagerSettings.Instance.Map.Provinces
            //        .Where(p => p.Shape != null)
            //        .Select(p => p.Id));

            //    Logger.AddDbgLog("Starting cleanup of non-existing province links...");


            //    // Process State files
            //    foreach (var kvp in mapCache.StatesCache)
            //    {
            //        string stateFileFullPath = kvp.Key;
            //        StateCachedFile stateFile = kvp.Value;
            //        if (stateFile == null)
            //        {
            //            continue;
            //        }
            //        Bracket stateBracket = stateFile.StateBracket;
            //        if (stateBracket == null)
            //        {
            //            continue;
            //        }
            //        bool statemodified = false;

            //        var provincesBracket = stateBracket.SubBrackets
            //            .FirstOrDefault(b => string.Equals(b.Header, "provinces", StringComparison.OrdinalIgnoreCase));

            //        var historyBracket = stateBracket.SubBrackets
            //            .FirstOrDefault(b => string.Equals(b.Header, "history", StringComparison.OrdinalIgnoreCase));

            //        if (provincesBracket != null)
            //        {
            //            CleanBracketContent(provincesBracket, stateFileFullPath, "provinces");
            //            statemodified = true;
            //        }

            //        if (historyBracket != null)
            //        {
            //            var vpBrackets = historyBracket.SubBrackets
            //                .Where(b => string.Equals(b.Header, "victory_points", StringComparison.OrdinalIgnoreCase))
            //                .ToList();

            //            foreach (var vpBracket in vpBrackets)
            //            {
            //                bool hasBad = vpBracket.Content.Any(line =>
            //                {
            //                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            //                    return parts.Any(part => !int.TryParse(part, out int id) || !existingProvs.Contains(id));
            //                });

            //                if (hasBad)
            //                {
            //                    historyBracket.SubBrackets.Remove(vpBracket);
            //                    Logger.AddDbgLog($"Removed invalid victory_points bracket from state file: {stateFileFullPath}");
            //                    statemodified = true;
            //                }
            //            }
            //        }

            //        if (statemodified)
            //        {
            //            mapCache.MarkStateFileDirty(stateFileFullPath);
            //        }
            //    }

            //    // Process Strategic Region files
            //    foreach (var kvp in mapCache.StrategicRegionsCache)
            //    {
            //        string regionfFileFullPath = kvp.Key;
            //        StrategicRegionCachedFile regionFile = kvp.Value;
            //        Bracket regionBracket = regionFile.RegionBracket;
            //        bool regionmodified = false;

            //        var provincesBracket = regionBracket.SubBrackets
            //            .FirstOrDefault(b => string.Equals(b.Header, "provinces", StringComparison.OrdinalIgnoreCase));

            //        if (provincesBracket != null)
            //        {
            //            CleanBracketContent(provincesBracket, regionfFileFullPath, "strategic region provinces");
            //            regionmodified = true;
            //        }

            //        if (regionmodified)
            //        {
            //            mapCache.MarkStrategicRegionFileDirty(regionfFileFullPath);
            //        }
            //    }


            //    string FileFullPath = ModDataStorage.Mod.MapCache.MapDefinitionCache.FileFullPath;
            //    DefinitionCachedFile defFile = ModDataStorage.Mod.MapCache.MapDefinitionCache;
            //    bool modified = false;

            //    var linesToRemove = new List<string>();
            //    foreach (var line in defFile.DefinitionLines)
            //    {
            //        var parts = line.Split(';');
            //        if (parts.Length > 0 && int.TryParse(parts[0], out int provId) && !existingProvs.Contains(provId))
            //        {
            //            linesToRemove.Add(line);
            //            Logger.AddDbgLog($"Removed non-existing province {provId} from definition file: {FileFullPath}");
            //        }
            //    }

            //    foreach (var line in linesToRemove)
            //    {
            //        defFile.DefinitionLines.Remove(line);
            //        modified = true;
            //    }

            //    if (modified)
            //    {
            //        mapCache.MarkDefinitionFileDirty(FileFullPath);
            //    }


            //    mapCache.SaveAllDirtyFiles();
            //    Logger.AddDbgLog("Cleanup of non-existing province links completed.");
            //}
            //public static void CheckProvincesDefines(ErrorPanel errorPanel)
            //{
            //    var mapCache = ModDataStorage.Mod.MapCache;
            //    if (mapCache == null || mapCache.MapDefinitionCache == null)
            //    {
            //        Logger.AddDbgLog("MapCache or MapDefinitionCache is not initialized. Cannot perform province defines check.");
            //        return;
            //    }

            //    var lines = ModDataStorage.Mod.MapCache.MapDefinitionCache.DefinitionLines;
            //    var FileFullPath = ModDataStorage.Mod.MapCache.MapDefinitionCache.FileFullPath;
            //    if (lines.Count == 0)
            //    {
            //        return;
            //    }

            //    // Collect all parsed provinces with line numbers (1-based)
            //    var idToLine = new Dictionary<int, int>();
            //    var colorToLines = new Dictionary<(int r, int g, int b), List<int>>();
            //    var parsedIds = new SortedSet<int>();
            //    bool hasZero = false;

            //    for (int i = 0; i < lines.Count; i++)
            //    {
            //        string line = lines[i].Trim();
            //        if (string.IsNullOrEmpty(line)) continue;

            //        var parts = line.Split(';');
            //        int lineNum = i + 1;

            //        // Critical 4: Not exactly 8 parts
            //        if (parts.Length != 8)
            //        {
            //            string msg = $"[Critical in prov define] Line {lineNum} does not have exactly 8 parts separated by ';'. Found {parts.Length} parts.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            continue;
            //        }

            //        try
            //        {
            //            int id = int.Parse(parts[0]);
            //            int r = int.Parse(parts[1]);
            //            int g = int.Parse(parts[2]);
            //            int b = int.Parse(parts[3]);
            //            string type = parts[4];
            //            bool isCoastal = bool.Parse(parts[5]);
            //            string terrain = parts[6];
            //            int continent = int.Parse(parts[7]);

            //            // Track IDs
            //            if (idToLine.ContainsKey(id))
            //            {
            //                // Critical 6: Duplicate ID
            //                string msg = $"[Critical in prov define] Duplicate province ID {id} found on lines {idToLine[id]} and {lineNum}.";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //            else
            //            {
            //                idToLine[id] = lineNum;
            //                parsedIds.Add(id);
            //            }

            //            if (id == 0) hasZero = true;

            //            // Track colors
            //            var colorKey = (r, g, b);
            //            if (!colorToLines.ContainsKey(colorKey))
            //            {
            //                colorToLines[colorKey] = new List<int>();
            //            }
            //            colorToLines[colorKey].Add(lineNum);
            //        }
            //        catch (Exception ex)
            //        {
            //            // Critical 5: Invalid formatting or data types
            //            string msg = $"[Critical in prov define] Line {lineNum} has invalid data or formatting: {ex.Message}.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //        }
            //    }

            //    // Critical 2: Missing ID 0
            //    if (!hasZero)
            //    {
            //        string msg = "[Critical in prov define] Province ID 0 is missing (required).";
            //        errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //    }

            //    // Check sequence for gaps and differences
            //    if (parsedIds.Count > 0)
            //    {
            //        int minId = parsedIds.Min;
            //        int maxId = parsedIds.Max;

            //        // Critical 3: Missing intermediate IDs
            //        for (int id = minId; id <= maxId; id++)
            //        {
            //            if (!parsedIds.Contains(id))
            //            {
            //                string msg = $"[Critical in prov define] Missing province ID {id} in sequence from {minId} to {maxId}.";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //        }

            //        // Critical 1: Non-sequential IDs (differences not exactly 1)
            //        var idList = parsedIds.ToList();
            //        for (int i = 0; i < idList.Count - 1; i++)
            //        {
            //            int current = idList[i];
            //            int next = idList[i + 1];
            //            int diff = next - current;

            //            if (diff != 1)
            //            {
            //                string msg = $"[Critical in prov define] Non-sequential province IDs: {current} (line {idToLine[current]}) followed by {next} (line {idToLine[next]}). Expected {current + 1}.";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //        }
            //    }

            //    // Critical 7: Duplicate colors
            //    foreach (var colorEntry in colorToLines.Where(e => e.Value.Count > 1))
            //    {
            //        var (r, g, b) = colorEntry.Key;
            //        string linesStr = string.Join(", ", colorEntry.Value);
            //        string msg = $"[Critical in prov define] Duplicate color RGB({r},{g},{b}) found on lines {linesStr}.";
            //        errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //    }

            //    // Warning 1: No empty line at end
            //    if (!string.IsNullOrEmpty(lines.LastOrDefault()?.Trim()))
            //    {
            //        string msg = "[Warn in prov define] No empty line at the end of the file.";
            //        errorPanel.AddError(ErrorType.Warning, msg, FileFullPath);
            //    }

            //    // Warning 2: Province 0 incorrect format
            //    if (hasZero && lines.Count > 0)
            //    {
            //        string zeroLine = lines.FirstOrDefault(l => l.StartsWith("0;"));
            //        if (zeroLine != null && zeroLine.Trim() != "0;0;0;0;land;false;unknown;0")
            //        {
            //            int zeroLineNum = lines.IndexOf(zeroLine) + 1;
            //            string msg = $"[Warn in prov define] Province ID 0 on line {zeroLineNum} does not match ideal format: '0;0;0;0;land;false;unknown;0'.";
            //            errorPanel.AddError(ErrorType.Warning, msg, FileFullPath);
            //        }
            //    }


            //    Logger.AddDbgLog("Province defines check completed.");
            //}
            //private static void CheckStateDefines(ErrorPanel errorPanel)
            //{
            //    var mapCache = ModDataStorage.Mod.MapCache;
            //    if (mapCache == null || mapCache.StatesCache.Count == 0)
            //    {
            //        Logger.AddDbgLog("MapCache or StatesCache is not initialized. Cannot perform state defines check.");
            //        return;
            //    }

            //    var existingProvs = new HashSet<int>(ModDataStorage.Mod.Map.Provinces
            //        .Where(p => p.Shape != null)
            //        .Select(p => p.Id));
            //    var provinceToFile = new Dictionary<int, string>();
            //    foreach (var kvp in mapCache.StatesCache)
            //    {
            //        string FileFullPath = kvp.Key;
            //        string fileName = System.IO.Path.GetFileName(FileFullPath);
            //        StateCachedFile stateFile = kvp.Value;
            //        Bracket stateBracket = stateFile.StateBracket;

            //        if (stateBracket == null)
            //        {
            //            string msg = $"[Critical in state defines] In file {fileName}: Failed to parse state bracket.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            continue;
            //        }

            //        // Critical 2: Unexpected symbols not part of assignments (content lines that aren't parsed as vars or brackets)
            //        foreach (var contentLine in stateBracket.Content)
            //        {
            //            string trimmed = contentLine.Trim();
            //            if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
            //            {
            //                string msg = $"[Critical in state defines] In file {fileName}: Unexpected content not part of any assignment or bracket: '{trimmed}'.";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //        }

            //        // Note: For Critical 1 - Unexpected assignment symbols without context.
            //        // Since BracketSearcher already parses and assumes valid structure, we check for isolated '=' or unmatched brackets indirectly.
            //        // To detect isolated '=', we can scan the original content for patterns.
            //        string content = stateFile.Content;
            //        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //        for (int i = 0; i < lines.Length; i++)
            //        {
            //            string line = lines[i].Split('#')[0].Trim(); // Ignore comments
            //            if (string.IsNullOrEmpty(line)) continue;

            //            // Check for isolated '='
            //            if (line.Contains('=') && !line.Contains("{"))
            //            {
            //                var parts = line.Split('=');
            //                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            //                {
            //                    string msg = $"[Critical in state defines] In file {fileName}: Invalid assignment '=' without proper name and value on line {i + 1}: '{line}'.";
            //                    errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //                }
            //            }
            //        }

            //        // Check bracket pairing - assuming parser already ensures pairs, but check if EndPosition is valid.
            //        if (stateBracket.EndPosition <= stateBracket.StartPosition)
            //        {
            //            string msg = $"[Critical in state defines] In file {fileName}: Unmatched or improperly paired brackets in state definition.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //        }

            //        // Critical 3: Missing provinces bracket
            //        var provincesBrackets = stateBracket.SubBrackets
            //            .Where(b => string.Equals(b.Header, "provinces", StringComparison.OrdinalIgnoreCase))
            //            .ToList();
            //        if (provincesBrackets.Count == 0)
            //        {
            //            string msg = $"[Critical in state defines] In file {fileName}: Missing 'provinces' bracket in state.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //        }

            //        // Critical 9: Provinces defined multiple times
            //        if (provincesBrackets.Count > 1)
            //        {
            //            string msg = $"[Critical in state defines] In file {fileName}: 'provinces' bracket defined multiple times.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //        }

            //        Bracket provincesBracket = provincesBrackets.FirstOrDefault();

            //        if (provincesBracket != null)
            //        {
            //            // Critical 4: Provinces empty
            //            if (provincesBracket.Content.Count == 0)
            //            {
            //                string msg = $"[Critical in state defines] In file {fileName}: 'provinces' bracket is empty (no content).";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }

            //            // Critical 5: SubVars or SubBrackets in provinces
            //            if (provincesBracket.SubVars.Count > 0 || provincesBracket.SubBrackets.Count > 0)
            //            {
            //                string msg = $"[Critical in state defines] In file {fileName}: 'provinces' bracket contains unexpected SubVars or SubBrackets (should only have province IDs).";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }

            //            // Critical 6: Non-existing provinces in provinces
            //            var provIds = new List<int>();
            //            var seenInFile = new HashSet<int>();
            //            foreach (var line in provincesBracket.Content)
            //            {
            //                var ids = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            //                    .Where(s => int.TryParse(s, out _))
            //                    .Select(int.Parse)
            //                    .ToList();
            //                provIds.AddRange(ids);

            //                foreach (var id in ids)
            //                {
            //                    if (!seenInFile.Add(id))
            //                    {
            //                        string msg = $"[Critical in state defines] In file {fileName}: Duplicate province ID {id} within the same 'provinces' bracket.";
            //                        errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //                    }
            //                }
            //            }

            //            // Critical 6: Non-existing provinces (existing)

            //            // New: Check for global duplicates across files
            //            foreach (var id in provIds.Distinct())
            //            {
            //                if (provinceToFile.TryGetValue(id, out string prevFile))
            //                {
            //                    string msg = $"[Critical in state defines] Province ID {id} duplicated in files {prevFile} and {fileName}.";
            //                    errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //                }
            //                else
            //                {
            //                    provinceToFile[id] = fileName;
            //                }
            //            }

            //            var badIds = provIds.Where(id => !existingProvs.Contains(id)).ToList();
            //            if (badIds.Count > 0)
            //            {
            //                string badIdsStr = string.Join(", ", badIds);
            //                string msg = $"[Critical in state defines] In file {fileName}: Non-existing province IDs in 'provinces': {badIdsStr}.";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //        }

            //        // Critical 7: Missing or empty history
            //        var historyBrackets = stateBracket.SubBrackets
            //            .Where(b => string.Equals(b.Header, "history", StringComparison.OrdinalIgnoreCase))
            //            .ToList();
            //        if (historyBrackets.Count == 0)
            //        {
            //            string msg = $"[Critical in state defines] In file {fileName}: Missing 'history' bracket in state.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //        }
            //        else
            //        {
            //            Bracket historyBracket = historyBrackets.First();
            //            if (historyBracket.SubVars.Count == 0)
            //            {
            //                string msg = $"[Critical in state defines] In file {fileName}: 'history' bracket is empty (no SubVars).";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //        }

            //        // Critical 8: History defined multiple times
            //        if (historyBrackets.Count > 1)
            //        {
            //            string msg = $"[Critical in state defines] In file {fileName}: 'history' bracket defined multiple times.";
            //            errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //        }

            //        // Critical 10-13: Missing required SubVars in state
            //        var requiredVars = new[] { "id", "manpower", "name", "state_category" };
            //        foreach (var varName in requiredVars)
            //        {
            //            if (stateBracket.GetSubVarIndex(varName) == -1)
            //            {
            //                string msg = $"[Critical in state defines] In file {fileName}: Missing required variable '{varName}' in state bracket.";
            //                errorPanel.AddError(ErrorType.Critical, msg, FileFullPath);
            //            }
            //        }

            //        // Warnings
            //        if (historyBrackets.Count > 0)
            //        {
            //            Bracket historyBracket = historyBrackets.First();

            //            // Warn 1: Missing owner in history.SubVars
            //            if (historyBracket.GetSubVarIndex("owner") == -1)
            //            {
            //                string msg = $"[Warn in state defines] In file {fileName}: Missing 'owner' variable in history bracket.";
            //                errorPanel.AddError(ErrorType.Warning, msg, FileFullPath);
            //            }

            //            // Warn 2: Missing add_core_of in history.SubVars
            //            if (historyBracket.GetSubVarIndex("add_core_of") == -1)
            //            {
            //                string msg = $"[Warn in state defines] In file {fileName}: Missing 'add_core_of' variable in history bracket.";
            //                errorPanel.AddError(ErrorType.Warning, msg, FileFullPath);
            //            }

            //            // Warn 3: Missing buildings in history.SubBrackets
            //            if (historyBracket.GetSubBracketIndex("buildings") == -1)
            //            {
            //                string msg = $"[Warn in state defines] In file {fileName}: Missing 'buildings' bracket in history.";
            //                errorPanel.AddError(ErrorType.Warning, msg, FileFullPath);
            //            }
            //        }
            //    }

            //    Logger.AddDbgLog("State defines check completed.");
        }

        private void CheckStateDefines_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CheckRegionDefinesButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CheckProvDefines_Click(object sender, RoutedEventArgs e)
        {
            //    ErrorPanel errorPanel = new ErrorPanel();
            //    CheckProvincesDefines(errorPanel);
            //    var dialog = new Window
            //    {
            //        Title = "Ошибки",
            //        Width = 500,
            //        Height = 325,
            //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //        Owner = this,
            //        Content = errorPanel,
            //        ResizeMode = ResizeMode.NoResize,
            //        WindowStyle = WindowStyle.SingleBorderWindow,
            //    };
            //    dialog.ShowDialog();
            //}

            //private void CheckStateDefines_Click(object sender, RoutedEventArgs e)
            //{
            //    ErrorPanel errorPanel = new ErrorPanel();
            //    CheckStateDefines(errorPanel);
            //    var dialog = new Window
            //    {
            //        Title = "Ошибки",
            //        Width = 500,
            //        Height = 325,
            //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //        Owner = this,
            //        Content = errorPanel,
            //        ResizeMode = ResizeMode.NoResize,
            //        WindowStyle = WindowStyle.SingleBorderWindow,
            //    };
            //    dialog.ShowDialog();
            //}

            //private void CheckRegionDefinesButton_Click(object sender, RoutedEventArgs e)
            //{
            //    ErrorPanel errorPanel = new ErrorPanel();
            //    errorPanel.AddError(ErrorType.Warning, "This feature is not implemented yet.", "MapHealerWindow");
            //    errorPanel.AddError(ErrorType.Critical, "Пенис 24 ПИЗДЕЦЦЦЦ ПРОСТО Я ЕБАЛ ПИЗАД.", "C:\\Users\\Acer\\Documents\\Paradox Interactive\\Hearts of Iron IV\\mod\\SME\\history\\states\\1-France.txt");
            //    var dialog = new Window
            //    {
            //        Title = "Ошибки",
            //        Width = 500,
            //        Height = 325,
            //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //        Owner = this,
            //        Content = errorPanel
            //    };

            //    dialog.ShowDialog();
        }
    }
}
