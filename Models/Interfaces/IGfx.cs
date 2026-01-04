using Models.Types.Utils;
using System.Drawing;

namespace Models.Interfaces
{
    public interface IGfx
    {
        Identifier Id { get; set; }
        public Bitmap Content { get; set; }
    }
}
