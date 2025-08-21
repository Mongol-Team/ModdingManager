using ModdingManagerModels.Types;

namespace ModdingManager.classes.cache.cachedFiles
{
    public class StrategicRegionCachedFile : CachedFile
    {
        public Bracket RegionBracket { get; private set; }

        public StrategicRegionCachedFile(string filePath) : base(filePath)
        {

            Content = File.ReadAllText(filePath);
            ParseContent();
        }
        public StrategicRegionCachedFile(string filePath, string content = null, bool isDirty = false) : base(filePath, content, isDirty)
        {
            FilePath = filePath;
            Content = content ?? File.ReadAllText(filePath);
            IsDirty = isDirty;
            LastWriteTime = File.GetLastWriteTime(filePath);
        }
        private void ParseContent()
        {
            var searcher = new BracketSearcher { CurrentString = Content.ToCharArray() };
            RegionBracket = searcher.FindBracketsByName("strategic_region").FirstOrDefault();
            UpdateIndexTime();
        }
        public override void SaveToFile()
        {
            if (IsDirty)
            {
                var content = RegionBracket.ToString();
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
