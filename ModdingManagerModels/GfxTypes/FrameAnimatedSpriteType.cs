namespace ModdingManagerModels.GfxTypes
{
    public struct FrameAnimatedSpriteType
    {
        public string Name;                 // "GFX_<name>"
        public string TextureFile;          // "<path>"
        public int NoOfFrames;              // <int>
        public string EffectFile;           // "<path>"

        public int AnimationRateFps;        // <int>
        public bool Looping;                // <bool>
        public bool PlayOnShow;             // <bool>
        public float PauseOnLoop;           // <float>

        public bool AllWaysTransparent;     // <bool>
    }
}
