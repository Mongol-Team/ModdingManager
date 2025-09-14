using System.Drawing;

namespace ModdingManagerModels.Interfaces
{
    public interface IGfx
    {
        string Name { get; set; }
        string Filetexture { get; set; }
        Bitmap? Content { get; set; }
    }
}
