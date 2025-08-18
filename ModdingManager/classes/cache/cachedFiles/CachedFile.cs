using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.classes.cache.cachedFiles
{
    public abstract class CachedFile
    {
        public string FilePath { get; set; }
        public string Content { get; set; }
        public bool IsDirty { get;  set; }
        public DateTime LastWriteTime { get; set; }
        public bool NeedsRefresh => File.GetLastWriteTime(FilePath) > LastWriteTime;

        protected DateTime _lastIndexUpdate = DateTime.MinValue;
        public bool NeedsReindex => File.GetLastWriteTime(FilePath) > _lastIndexUpdate;

        public CachedFile(string filePath, string content = null, bool isDirty = false)
        {
            FilePath = filePath;
            Content = content ?? File.ReadAllText(filePath);
            IsDirty = isDirty;
            LastWriteTime = File.GetLastWriteTime(filePath);
        }

        public virtual void RefreshContent()
        {
            Content = File.ReadAllText(FilePath);
            LastWriteTime = File.GetLastWriteTime(FilePath);
            IsDirty = false;
        }

        public virtual void SaveToFile()
        {
            if (IsDirty)
            {
                File.WriteAllText(FilePath, Content);
                LastWriteTime = File.GetLastWriteTime(FilePath);
                IsDirty = false;
            }
        }

        public void MarkDirty() => IsDirty = true;
        public void UpdateIndexTime() => _lastIndexUpdate = DateTime.Now;
    }
}
