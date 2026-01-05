using System;

namespace Application.Settings
{
    public class RecentProject
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string LastOpened { get; set; }

        public RecentProject()
        {
            Path = string.Empty;
            Name = string.Empty;
            LastOpened = string.Empty;
        }

        public RecentProject(string path, string name)
        {
            Path = path ?? string.Empty;
            Name = name ?? string.Empty;
            LastOpened = GetLastOpenedString(path);
        }

        private string GetLastOpenedString(string path)
        {
            try
            {
                if (System.IO.Directory.Exists(path))
                {
                    var lastWrite = System.IO.Directory.GetLastWriteTime(path);
                    var now = DateTime.Now;
                    var diff = now - lastWrite;

                    if (diff.TotalDays < 1)
                        return "Today";
                    if (diff.TotalDays < 2)
                        return "Yesterday";
                    if (diff.TotalDays < 30)
                        return $"{Math.Floor(diff.TotalDays)} days ago";
                    
                    return lastWrite.ToString("dd.MM.yyyy");
                }
            }
            catch
            {
            }
            return "";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

