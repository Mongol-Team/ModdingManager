using ModdingManager.classes.cache.cachedFiles;
using ModdingManager.managers.@base;
using ModdingManagerModels.Types.ObjectCacheData;

namespace ModdingManager.classes.cache.data
{
    public class MapCache : IDisposable
    {
        public Dictionary<string, StateCachedFile> StatesCache = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, StrategicRegionCachedFile> StrategicRegionsCache = new(StringComparer.OrdinalIgnoreCase);
        public DefinitionCachedFile MapDefinitionCache;

        public Dictionary<int, (string fileKey, Bracket stateBracket)> _provinceIndex;
        public HashSet<string> _dirtyFiles = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<int, (string fileKey, Bracket stateBracket)> ProvinceIndex =>
            _provinceIndex ??= BuildProvinceIndex();

        public MapCache()
        {
            LoadAllCaches();
        }

        public void Dispose()
        {
            SaveAllDirtyFiles();
            ClearAllCaches();
        }

        #region State Files Management
        public IReadOnlyDictionary<string, StateCachedFile> GetStateFiles() => StatesCache;

        public StateCachedFile GetStateFile(string filePath)
        {
            if (!StatesCache.TryGetValue(filePath, out var cachedFile))
            {
                cachedFile = new StateCachedFile(filePath);
                StatesCache[filePath] = cachedFile;
            }
            return cachedFile;
        }

        public bool AddStateFile(string filePath)
        {
            if (StatesCache.ContainsKey(filePath)) return false;

            StatesCache[filePath] = new StateCachedFile(filePath);
            return true;
        }

        public bool RemoveStateFile(string filePath)
        {
            if (!StatesCache.ContainsKey(filePath)) return false;

            StatesCache.Remove(filePath);
            _provinceIndex = null; // Reset index as it's now invalid
            return true;
        }

        public void MarkFileDirty(string filePath)
        {
            if (StatesCache.ContainsKey(filePath) ||
                StrategicRegionsCache.ContainsKey(filePath) ||
                MapDefinitionCache.FilePath == filePath)
            {
                _dirtyFiles.Add(filePath);
            }
        }
        #endregion

        #region Strategic Regions Management
        public IReadOnlyDictionary<string, StrategicRegionCachedFile> GetStrategicRegionFiles() => StrategicRegionsCache;

        public StrategicRegionCachedFile GetStrategicRegionFile(string filePath)
        {
            if (!StrategicRegionsCache.TryGetValue(filePath, out var cachedFile))
            {
                cachedFile = new StrategicRegionCachedFile(filePath);
                StrategicRegionsCache[filePath] = cachedFile;
            }
            return cachedFile;
        }

        public bool AddStrategicRegionFile(string filePath)
        {
            if (StrategicRegionsCache.ContainsKey(filePath)) return false;

            StrategicRegionsCache[filePath] = new StrategicRegionCachedFile(filePath);
            return true;
        }

        public bool RemoveStrategicRegionFile(string filePath)
        {
            return StrategicRegionsCache.Remove(filePath);
        }

        //public bool TryFindStrategicRegionForProvince(int provinceId, out StrategicRegionCachedFile regionFile)
        //{
        //    regionFile = StrategicRegionsCache.Values.FirstOrDefault(f =>
        //        f.RegionBracket.SubBrackets
        //            .FirstOrDefault(b => b.Header == "provinces")?
        //            .Content.Any(line => line.Contains(provinceId.ToString())) ?? false);

        //    return regionFile != null;
        //}
        #endregion

        #region Definition Files Management


        #endregion

        #region Province Index Management
        public Dictionary<int, (string fileKey, Bracket stateBracket)> BuildProvinceIndex()
        {
            var index = new Dictionary<int, (string, Bracket)>();

            foreach (var stateFile in StatesCache)
            {
                var provincesBracket = stateFile.Value.StateBracket.SubBrackets
                    .FirstOrDefault(b => b.Header.Equals("provinces", StringComparison.OrdinalIgnoreCase));

                if (provincesBracket == null) continue;

                foreach (var provinceId in ParseProvinceIds(provincesBracket.Content))
                {
                    index[provinceId] = (stateFile.Key, stateFile.Value.StateBracket);
                }
            }

            _provinceIndex = index;
            return index;
        }

        private IEnumerable<int> ParseProvinceIds(IEnumerable<string> contentLines)
        {
            return contentLines
                .SelectMany(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(s => int.TryParse(s, out int id) ? id : -1)
                .Where(id => id > 0);
        }

        public bool TryGetProvinceState(int provinceId, out (string fileKey, Bracket stateBracket) stateInfo)
        {
            if (_provinceIndex == null) BuildProvinceIndex();
            return _provinceIndex.TryGetValue(provinceId, out stateInfo);
        }
        #endregion

        #region Cache Management
        private void LoadAllCaches()
        {
            LoadStateFiles();
            LoadStrategicRegionsFiles();
            LoadDefinitionFile();
        }

        private void LoadStateFiles()
        {
            LoadCacheFiles(
                Path.Combine("history", "states"),
                new[] { ".txt" },
                StatesCache,
                path => new StateCachedFile(path));
        }

        private void LoadStrategicRegionsFiles()
        {
            LoadCacheFiles(
                Path.Combine("map", "strategicregions"),
                new[] { ".txt" },
                StrategicRegionsCache,
                path => new StrategicRegionCachedFile(path));
        }

        private void LoadDefinitionFile()
        {
            string modFile = Path.Combine(ModManager.ModDirectory, "map", "definition.csv");
            string gameFile = Path.Combine(ModManager.GameDirectory, "map", "definition.csv");

            if (File.Exists(modFile))
            {
                MapDefinitionCache = new DefinitionCachedFile(modFile, File.ReadAllText(modFile), false);
            }
            else if (File.Exists(gameFile))
            {
                MapDefinitionCache = new DefinitionCachedFile(gameFile, File.ReadAllText(gameFile), false);
            }
        }


        private void LoadCacheFiles<T>(
            string relativePath,
            string[] allowedExtensions,
            Dictionary<string, T> cache,
            Func<string, T> fileFactory) where T : CachedFile
        {
            string modPath = Path.Combine(ModManager.ModDirectory, relativePath);
            string gamePath = Path.Combine(ModManager.GameDirectory, relativePath);

            if (Directory.Exists(modPath))
            {
                foreach (var file in Directory.GetFiles(modPath)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)))
                {
                    cache[file] = fileFactory(file);
                }
            }

            if (Directory.Exists(gamePath) && cache.Count == 0)
            {
                foreach (var file in Directory.GetFiles(gamePath)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)))
                {
                    cache[file] = fileFactory(file);
                }
            }
        }

        public void SaveAllDirtyFiles()
        {
            foreach (var filePath in _dirtyFiles)
            {
                if (StatesCache.TryGetValue(filePath, out var stateFile) && stateFile.IsDirty)
                {
                    stateFile.SaveToFile();
                }
                else if (StrategicRegionsCache.TryGetValue(filePath, out var regionFile) && regionFile.IsDirty)
                {
                    regionFile.SaveToFile();
                }
                else if (MapDefinitionCache.FilePath == filePath && MapDefinitionCache.IsDirty)
                {
                    MapDefinitionCache.SaveToFile();
                }
            }
            _dirtyFiles.Clear();
        }

        private void ClearAllCaches()
        {
            StatesCache.Clear();
            StrategicRegionsCache.Clear();
            MapDefinitionCache = null;
            _provinceIndex?.Clear();
        }
        #endregion
        #region Dirty Files Management

        // State Files
        public void MarkStateFileDirty(string filePath)
        {
            if (StatesCache.ContainsKey(filePath))
            {
                _dirtyFiles.Add(filePath);
                StatesCache[filePath].MarkDirty();
            }
        }

        public void SaveDirtyStateFiles()
        {
            foreach (var filePath in _dirtyFiles.Where(fp => StatesCache.ContainsKey(fp)).ToList())
            {
                if (StatesCache[filePath].IsDirty)
                {
                    StatesCache[filePath].SaveToFile();
                    _dirtyFiles.Remove(filePath);
                }
            }
        }

        // Strategic Region Files
        public void MarkStrategicRegionFileDirty(string filePath)
        {
            if (StrategicRegionsCache.ContainsKey(filePath))
            {
                _dirtyFiles.Add(filePath);
                StrategicRegionsCache[filePath].MarkDirty();
            }
        }

        public void SaveDirtyStrategicRegionFiles()
        {
            foreach (var filePath in _dirtyFiles.Where(fp => StrategicRegionsCache.ContainsKey(fp)).ToList())
            {
                if (StrategicRegionsCache[filePath].IsDirty)
                {
                    StrategicRegionsCache[filePath].SaveToFile();
                    _dirtyFiles.Remove(filePath);
                }
            }
        }

        // Definition Files
        public void MarkDefinitionFileDirty(string filePath)
        {
            if (MapDefinitionCache.FilePath == filePath)
            {
                _dirtyFiles.Add(filePath);
                MapDefinitionCache.MarkDirty();
            }
        }

        #endregion
    }
}