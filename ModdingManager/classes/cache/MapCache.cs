using ModdingManager.classes.utils.types;
using ModdingManager.managers.@base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModdingManager.classes.cache
{
    public class MapCache : IDisposable
    {
        private Dictionary<string, CachedFile> _statesCache = new();
        private Dictionary<string, CachedFile> _countriesCache = new();
        private Dictionary<string, CachedFile> _strategicRegionsCache = new();
        private Dictionary<string, CachedFile> _mapDefinitionCache = new();
        private Dictionary<int, (string fileKey, Bracket stateBracket)> _provinceIndex;
        public Dictionary<int, (string fileKey, Bracket stateBracket)> ProvinceIndex => _provinceIndex;
        private HashSet<string> _dirtyStateFiles = new HashSet<string>();
        public MapCache()
        {
            LoadAllCaches();
            BuildProvinceIndex();
        }

        public void Dispose()
        {
            SaveAllDirtyFiles();
            ClearAllCaches();
        }

        #region Public Access Methods
        public string GetStateFileContent(string filePath)
        {
            return GetFileContent(filePath, _statesCache);
        }

        public void UpdateStateFile(string filePath, string content)
        {
            UpdateFile(filePath, content, _statesCache);
        }
        public void MarkStateFileDirty(string fileKey)
        {
            if (_statesCache.ContainsKey(fileKey))
            {
                _dirtyStateFiles.Add(fileKey);
            }
        }

        public void SaveDirtyStateFiles()
        {
            foreach (var fileKey in _dirtyStateFiles)
            {
                if (_statesCache.TryGetValue(fileKey, out var cachedFile))
                {
                    // Получаем актуальное содержимое из StateBrackets
                    var contentBuilder = new StringBuilder();
                    foreach (var bracket in cachedFile.StateBrackets)
                    {
                        contentBuilder.AppendLine(bracket.ToString());
                    }

                    // Обновляем содержимое файла
                    cachedFile.Content = contentBuilder.ToString();
                    cachedFile.IsDirty = true;
                    cachedFile.SaveToFile();
                }
            }
            _dirtyStateFiles.Clear();
        }
        public string GetDefinitionFileContent(string filePath)
        {
            if (_mapDefinitionCache.TryGetValue(filePath, out var cachedFile))
            {
                return cachedFile.Content;
            }

            cachedFile = new CachedFile(filePath);
            _mapDefinitionCache[filePath] = cachedFile;
            return cachedFile.Content;
        }

        public void UpdateDefinitionFile(string filePath, string content)
        {
            if (_mapDefinitionCache.TryGetValue(filePath, out var cachedFile))
            {
                cachedFile.Content = content;
                cachedFile.IsDirty = true;
            }
            else
            {
                _mapDefinitionCache[filePath] = new CachedFile(filePath, content, true);
            }
        }

        public void BuildProvinceIndex()
        {
            _provinceIndex = new Dictionary<int, (string, Bracket)>();
            foreach (var stateFile in _statesCache)
            {
                string content = stateFile.Value.Content;
                var searcher = new BracketSearcher { CurrentString = content.ToCharArray() };
                var stateBrackets = searcher.FindBracketsByName("state");

                // Сохраняем распарсенные брекеты для последующего использования
                stateFile.Value.StateBrackets = stateBrackets;

                foreach (var stateBracket in stateBrackets)
                {
                    var provincesBracket = stateBracket.SubBrackets.FirstOrDefault(b => b.Header == "provinces");
                    if (provincesBracket == null) continue;

                    var provinceIds = provincesBracket.Content
                        .SelectMany(line => line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(s => int.TryParse(s, out int id) ? id : -1)
                        .Where(id => id > 0);

                    foreach (var id in provinceIds)
                    {
                        _provinceIndex[id] = (stateFile.Key, stateBracket);
                    }
                }
            }
        }

        public IReadOnlyDictionary<string, CachedFile> GetStateFiles() => _statesCache;

        public void SaveAllChanges()
        {
            SaveChanges(_statesCache);
            SaveChanges(_mapDefinitionCache);
        }

        private void SaveChanges(Dictionary<string, CachedFile> cache)
        {
            foreach (var item in cache.Values.Where(x => x.IsDirty))
            {
                item.SaveToFile();
            }
        }

 
        #endregion

        #region Cache Management
        private void LoadAllCaches()
        {
            LoadCache(Path.Combine("history", "states"), new[] { ".txt" }, _statesCache);
            LoadCache(Path.Combine("history", "countries"), new[] { ".txt" }, _countriesCache);
            LoadCache(Path.Combine("map", "strategicregions"), new[] { ".txt" }, _strategicRegionsCache);
            LoadCache("map", new[] { ".txt", ".csv" }, _mapDefinitionCache);
        }

        private void LoadCache(string relativePath, string[] allowedExtensions, Dictionary<string, CachedFile> cache)
        {
            string modPath = Path.Combine(ModManager.Directory, relativePath);
            string gamePath = Path.Combine(ModManager.GameDirectory, relativePath);

            // Загрузка из мода
            if (Directory.Exists(modPath))
            {
                foreach (var file in Directory.GetFiles(modPath)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)))
                {
                    cache[file] = new CachedFile(file);
                }
            }

            // Загрузка из игры (если в моде нет файлов)
            if (Directory.Exists(gamePath) && cache.Count == 0)
            {
                foreach (var file in Directory.GetFiles(gamePath)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)))
                {
                    cache[file] = new CachedFile(file);
                }
            }
        }
        public void MarkStateFilesDirty()
        {
            foreach (var file in _statesCache.Values)
            {
                file.IsDirty = true;
            }
        }

        // Новый метод для проверки необходимости обновления
        public bool CheckStateFileNeedsRefresh(string filePath)
        {
            if (_statesCache.TryGetValue(filePath, out var cachedFile))
            {
                return cachedFile.NeedsRefresh;
            }
            return true;
        }
        public void AddDefinitionFile(string filePath)
        {
            if (!_mapDefinitionCache.ContainsKey(filePath))
            {
                _mapDefinitionCache[filePath] = new CachedFile(filePath);
            }
        }
        private string GetFileContent(string filePath, Dictionary<string, CachedFile> cache)
        {
            if (!cache.TryGetValue(filePath, out var cachedFile))
            {
                cachedFile = new CachedFile(filePath);
                cache[filePath] = cachedFile;
            }

            if (cachedFile.NeedsRefresh)
            {
                cachedFile.RefreshContent();
            }

            return cachedFile.Content;
        }

        private void UpdateFile(string filePath, string content, Dictionary<string, CachedFile> cache)
        {
            if (cache.TryGetValue(filePath, out var cachedFile))
            {
                cachedFile.Content = content;
                cachedFile.IsDirty = true;
            }
            else
            {
                cache[filePath] = new CachedFile(filePath, content, true);
            }
        }

        public void SaveAllDirtyFiles()
        {
            SaveDirtyFiles(_statesCache);
            SaveDirtyFiles(_countriesCache);
            SaveDirtyFiles(_strategicRegionsCache);
            SaveDirtyFiles(_mapDefinitionCache);
        }

        private void SaveDirtyFiles(Dictionary<string, CachedFile> cache)
        {
            foreach (var item in cache.Where(x => x.Value.IsDirty))
            {
                item.Value.SaveToFile();
            }
        }

        private void ClearAllCaches()
        {
            _statesCache.Clear();
            _countriesCache.Clear();
            _strategicRegionsCache.Clear();
            _mapDefinitionCache.Clear();
        }
        // Изменить модификатор доступа вложенного класса CachedFile на public
        public class CachedFile
        {
            public string FilePath { get; }
            public string Content { get; set; }
            public bool IsDirty { get; set; }
            public DateTime LastWriteTime { get; private set; }
            public bool NeedsRefresh => File.GetLastWriteTime(FilePath) > LastWriteTime;
            private DateTime _lastIndexUpdate = DateTime.MinValue;
            public bool NeedsReindex => File.GetLastWriteTime(FilePath) > _lastIndexUpdate;
            public List<Bracket> StateBrackets { get; set; } = new List<Bracket>();
            public void UpdateIndex()
            {
                _lastIndexUpdate = File.GetLastWriteTime(FilePath);
            }
            public CachedFile(string filePath, string content = null, bool isDirty = false)
            {
                FilePath = filePath;
                Content = content ?? File.ReadAllText(filePath);
                IsDirty = isDirty;
                LastWriteTime = File.GetLastWriteTime(filePath);
            }

            public void RefreshContent()
            {
                Content = File.ReadAllText(FilePath);
                LastWriteTime = File.GetLastWriteTime(FilePath);
                IsDirty = false;
            }

            public void SaveToFile()
            {
                if (IsDirty)
                {
                    File.WriteAllText(FilePath, Content);
                    LastWriteTime = File.GetLastWriteTime(FilePath);
                    IsDirty = false;
                }
            }
        }
        #endregion
    }
}