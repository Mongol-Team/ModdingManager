using System.Drawing;

public struct CorneredTileSpriteType
{
    public string name;               // "GFX_<name>"
    public string texturefile;        // "<path>"
    public int noofframes;            // <int>

    public Point size;                // { x y }
    public Point bordersize;          // { x y }

    public string effectfile;         // "<path>"

    public bool allwaystransparent;   // <bool>
    public bool tilingcenter;         // <bool>

    public bool looping;              // <bool>
    public int animation_rate_spf;    // <int>
}
