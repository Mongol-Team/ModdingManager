using Data;
using Models.Attributes;
using Models.Interfaces;
using Models.Types.Utils;
using System.Drawing;

namespace Models.GfxTypes
{
    [ConfigCreator(ConfigCreatorType.GenericCreator)]
    public class SpriteType : IGfx
    {
        public Identifier Id { get; set; }
        public string TexturePath { get; set; }          // relative path to texture inside /Hearts of Iron IV/
        public int NoOfFrames { get; set; }               // number of frames for multi-frame images
        public string EffectFile { get; set; }            // effect definition from /gfx/FX/*.lua
        public bool AllwaysTransparent { get; set; }     // image cannot be clicked or interacted with
        public bool LegacyLazyLoad { get; set; }        // lazy loading flag
        public bool TransparenceCheck { get; set; }       // if alpha channel is used for click bounding
        public Bitmap Content { get; set; }
        public string FileFullPath { get; set; }
        public SpriteType() { }
        public SpriteType(Bitmap content, string name)
        {
            Content = content;
            TexturePath = DataDefaultValues.Null;
            Id = new Identifier(name);
            NoOfFrames = -1;
            EffectFile = DataDefaultValues.Null;
            AllwaysTransparent = false;
            LegacyLazyLoad = false;
            TransparenceCheck = false;
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }

}
