using Models.Types.Utils;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Models.Interfaces
{
    public interface IGfx
    {
        Identifier Id { get; set; }
        public Bitmap Content { get; set; }
        public string FileFullPath { get; set; }
    }
}
