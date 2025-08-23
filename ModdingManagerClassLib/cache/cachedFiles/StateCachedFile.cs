using ModdingManagerClassLib;
using ModdingManagerClassLib.Debugging;
using ModdingManagerModels.Types.ObectCacheData;

namespace ModdingManager.classes.cache.cachedFiles
{
    public class StateCachedFile : CachedFile
    {
        public Bracket StateBracket { get; private set; }

        public StateCachedFile(string filePath) : base(filePath)
        {
            Content = File.ReadAllText(filePath);
            ParseContent();
        }
        public StateCachedFile(string filePath, string content = null, bool isDirty = false) : base(filePath, content, isDirty)
        {
            FilePath = filePath;
            Content = content ?? File.ReadAllText(filePath);
            IsDirty = isDirty;
            LastWriteTime = File.GetLastWriteTime(filePath);
        }
        private void ParseContent()
        {
            try
            {
                var searcher = new BracketSearcher { CurrentString = Content.ToCharArray() };
                StateBracket = searcher.FindBracketsByName("state").FirstOrDefault();
                UpdateIndexTime();
            }
            catch (Exception ex)
            {
                Logger.AddLog($"⚠️ Failed to parse state file {FilePath}: {ex.Message}");
            }
        }
        public override void SaveToFile()
        {
            if (IsDirty)
            {
                var content = StateBracket.ToString();
                File.WriteAllText(FilePath, content);
                LastWriteTime = File.GetLastWriteTime(FilePath);
                IsDirty = false;
            }
        }
        public override void RefreshContent()
        {
            base.RefreshContent();
            ParseContent();
        }
    }
}
