using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels.Interfaces
{
    public interface IGfx
    {
        Identifier Id { get; set; }
        string TexturePath { get; set; }
        Bitmap? Content { get; set; }
    }
}
