using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public struct SpriteType
    {
        public string name;                  // name is the name you have given to the asset.
        public string texturefile;           // relative path to texture inside /Hearts of Iron IV/
        public int noofframes;               // number of frames for multi-frame images
        public string effectfile;            // effect definition from /gfx/FX/*.lua
        public bool allwaystransparent;      // image cannot be clicked or interacted with
        public bool legacy_lazy_load;        // lazy loading flag
        public bool transparencecheck;       // if alpha channel is used for click bounding

        // Animation fields
        public string animationmaskfile;
        public string animationtexturefile;
        public float animationrotation;      // default -90 clockwise
        public bool animationlooping;
        public float animationtime;          // duration in seconds
        public float animationdelay;         // delay in seconds
        public string animationblendmode;    // add, multiply, overlay
        public string animationtype;         // scrolling, rotating, pulsing
        public Point animationrotationoffset;
        public float animationtexturescale;
        public int animationframes;
    }

}
