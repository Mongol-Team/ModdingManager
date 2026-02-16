using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EntityFiles
{
    public interface IFile
    {
        string FileFullPath { get; set; }
        string FileName { get; }
        bool IsOverride { get; set; }
        bool IsCore { get; set; }
        public bool Rename(string newFileName);
    }
}
