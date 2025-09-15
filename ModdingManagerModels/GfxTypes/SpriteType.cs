using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.Utils;
using System.Drawing;

namespace ModdingManagerModels.GfxTypes
{
    public class SpriteType : IConfig, IGfx
    {
        public Identifier Id { get; set; }                // name is the name you have given to the asset.
        public string TexturePath { get; set; }          // relative path to texture inside /Hearts of Iron IV/
        public int NoOfFrames { get; set; }               // number of frames for multi-frame images
        public string EffectFile { get; set; }            // effect definition from /gfx/FX/*.lua
        public bool AllwaysTransparent { get; set; }     // image cannot be clicked or interacted with
        public bool LegacyLazyLoad { get; set; }        // lazy loading flag
        public bool TransparenceCheck { get; set; }       // if alpha channel is used for click bounding
        public Bitmap Content { get; set; }
        //// Animation fields
        ///чо ето за хуйня
        //public string Animationmaskfile;
        //public string animationtexturefile;
        //public float animationrotation;      // default -90 clockwise
        //public bool animationlooping;
        //public float animationtime;          // duration in seconds
        //public float animationdelay;         // delay in seconds
        //public string animationblendmode;    // add, multiply, overlay
        //public string animationtype;         // scrolling, rotating, pulsing
        //public Point animationrotationoffset;
        //public float animationtexturescale;
        //public int animationframes;
    }

}
