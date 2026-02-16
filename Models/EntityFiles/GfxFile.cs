using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EntityFiles
{
    public class GfxFile<T> : IFile where T : IGfx
    {
        public List<T> Entities { get; set; } = new List<T>();
        public string FileFullPath { get; set; }
        public bool IsOverride { get; set; }
        public bool IsCore { get; set; }
        public string FileName => Path.GetFileName(FileFullPath);
        public GfxFile() { }

        public bool Rename(string newFileName)
        {
            if (string.IsNullOrWhiteSpace(FileFullPath) ||
                string.IsNullOrWhiteSpace(newFileName))
            {
                return false;
            }

            try
            {
                string? directory = Path.GetDirectoryName(FileFullPath);
                if (directory == null)
                    return false;

                string newFullPath = Path.Combine(directory, newFileName);

                FileFullPath = newFullPath;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
