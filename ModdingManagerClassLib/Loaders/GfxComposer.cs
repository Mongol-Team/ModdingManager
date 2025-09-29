using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.utils.Pathes;
using ModdingManagerDataManager.Parsers;
using ModdingManagerDataManager.Parsers.Patterns;
using ModdingManagerModels.GfxTypes;
using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObjectCacheData;
using ModdingManagerModels.Types.Utils;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

namespace ModdingManagerClassLib.Loaders
{
    public static class GfxLoader
    {
        public static List<IGfx> LoadAll()
        {
            string[] possiblePaths = {
                ModPathes.InterfacePath,
                GamePathes.InterfacePath,
            };
            List<IGfx> result = new();
            foreach (string path in possiblePaths)
            {

                if (System.IO.Directory.Exists(path))
                {
                    var files = System.IO.Directory.GetFiles(path, "*.gfx", System.IO.SearchOption.AllDirectories);
                    
                    foreach (var file in files)
                    {
                        var gfxs = LoadFromFile(file);
                        if (gfxs.Count > 0)
                            result.AddRange(gfxs);
                    }
                    if (result.Count > 0) break;
                }
            }
            return result;
        }
        public static List<IGfx> LoadFromFile(string gfxFilePath)
        {
            HoiFuncFile parser = new TxtParser(new TxtPattern()).Parse(gfxFilePath) as HoiFuncFile;
            List<IGfx> result = new();
            if (parser == null) return result;
            if (parser.Brackets.Count == 0) return result;
            foreach (Bracket defineBr in parser.Brackets)
            {
                if (defineBr.Name == "spriteTypes")
                {
                    foreach (Bracket spriteBr in defineBr.SubBrackets)
                    {
                        IGfx gfx = ParseSingleSpriteGfx(spriteBr);
                        
                        result.AddSafe(gfx);
                    }
                }
                else if (defineBr.Name == "objectTypes")
                {
                    foreach (Bracket spriteBr in defineBr.SubBrackets)
                    {
                        IGfx gfx = ParseSingleObjGfx(spriteBr);

                        result.AddSafe(gfx);
                    }
                }
            }
            return result;
        }
        public static IGfx? ParseSingleSpriteGfx(Bracket gfxBracket)
        {
            string brName = gfxBracket.Name.ToLower();
            switch (brName)
            {
                case "textspritetype":
                    {
                        return new TextSpriteType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            Content = BitmapExtensions.LoadResourceRealativePath(gfxBracket.GetVarString("textureFile"))
                        };
                    }
                case "spritetype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string modPath = Path.Combine(ModPathes.InterfacePath, textureFile);
                        string gamePath = Path.Combine(GamePathes.InterfacePath, textureFile);

                        return new SpriteType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            AllwaysTransparent = gfxBracket.GetVarBool("alwaystransparent"),
                            LegacyLazyLoad = gfxBracket.GetVarBool("legacy_lazy_load"),
                            TransparenceCheck = gfxBracket.GetVarBool("transparencecheck"),
                            Content = File.Exists(modPath) ? BitmapExtensions.LoadResourceFullPath(modPath)
                                   : File.Exists(gamePath) ? BitmapExtensions.LoadResourceFullPath(gamePath) : null
                        };
                    }

                case "frameanimatedspritetype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string modPath = Path.Combine(ModPathes.InterfacePath, textureFile);
                        string gamePath = Path.Combine(GamePathes.InterfacePath, textureFile);

                        return new FrameAnimatedSpriteType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            AnimationRateFps = gfxBracket.GetVarInt("animation_rate_fps"),
                            Content = File.Exists(modPath) ? BitmapExtensions.LoadResourceFullPath(modPath)
                                   : File.Exists(gamePath) ? BitmapExtensions.LoadResourceFullPath(gamePath) : null,
                            Looping = gfxBracket.GetVarBool("looping"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            AllwaysTransparent = gfxBracket.GetVarBool("alwaystransparent"),
                            PlayOnShow = gfxBracket.GetVarBool("play_on_show"),
                            PauseOnLoop = gfxBracket.GetVarDouble("pause_on_loop")
                        };
                    }

                case "progressbartype":
                    {
                        string textureFile1 = gfxBracket.GetVarString("textureFile1").Replace("/", "\\");
                        string textureFile2 = gfxBracket.GetVarString("textureFile2").Replace("/", "\\");
                        string modPath1 = Path.Combine(ModPathes.InterfacePath, textureFile1);
                        string gamePath1 = Path.Combine(GamePathes.InterfacePath, textureFile1);
                        string modPath2 = Path.Combine(ModPathes.InterfacePath, textureFile2);
                        string gamePath2 = Path.Combine(GamePathes.InterfacePath, textureFile2);

                        return new ProgressbarType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile1"),
                            SecondTexturePath = gfxBracket.GetVarString("textureFile2"),
                            Content = File.Exists(modPath1) ? BitmapExtensions.LoadResourceFullPath(modPath1)
                                   : File.Exists(gamePath1) ? BitmapExtensions.LoadResourceFullPath(gamePath1) : null,
                            BgContent = File.Exists(modPath2) ? BitmapExtensions.LoadResourceFullPath(modPath2)
                                     : File.Exists(gamePath2) ? BitmapExtensions.LoadResourceFullPath(gamePath2) : null,
                            Size = new Point(
                                gfxBracket.GetSubBracketVarInt("size", "x"),
                                gfxBracket.GetSubBracketVarInt("size", "y")),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            Color = gfxBracket.GetArrayColor("color"),
                            SecondColor = gfxBracket.GetArrayColor("colortwo"),
                            IsHorisontal = false,
                            Steps = -1
                        };
                    }

                case "corneredtilespritetype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string modPath = Path.Combine(ModPathes.InterfacePath, textureFile);
                        string gamePath = Path.Combine(GamePathes.InterfacePath, textureFile);

                        return new CorneredTileSpriteType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            AllwaysTrancparent = gfxBracket.GetVarBool("alwaystransparent"),
                            Content = File.Exists(modPath) ? BitmapExtensions.LoadResourceFullPath(modPath)
                                   : File.Exists(gamePath) ? BitmapExtensions.LoadResourceFullPath(gamePath) : null,
                            TilingCenter = gfxBracket.GetVarBool("tilingCenter"),
                            Looping = gfxBracket.GetVarBool("looping"),
                            AnimationRateSpf = gfxBracket.GetVarInt("animation_rate_spf"),
                            Size = new Point(
                                gfxBracket.GetSubBracketVarInt("size", "x"),
                                gfxBracket.GetSubBracketVarInt("size", "y")),
                            BorderSize = new Point(
                                gfxBracket.GetSubBracketVarInt("borderSize", "x"),
                                gfxBracket.GetSubBracketVarInt("borderSize", "y"))
                        };
                    }
                case "piecharttype":
                    {

                        return new PieChartType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            Size = gfxBracket.GetVarInt("size"),
                            Colors = gfxBracket.Arrays.FirstOrDefault(a => a.Name == "colors")?.Values.Cast<Color>().ToList() ?? new List<Color>()
                        };
                    }
                case "linecharttype":
                    {
                        return new LineChartType()
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            Size = new Point(
                                gfxBracket.GetSubBracketVarInt("size", "x"),
                                gfxBracket.GetSubBracketVarInt("size", "y")),
                            LineWidth = gfxBracket.GetVarDouble("lineWidth"),
                        };
                    }
                case "maskedshieldtype":
                    {
                        string textureFile1 = gfxBracket.GetVarString("textureFile1").Replace("/", "\\");
                        string textureFile2 = gfxBracket.GetVarString("textureFile2").Replace("/", "\\");
                        string modPath1 = Path.Combine(ModPathes.InterfacePath, textureFile1);
                        string gamePath1 = Path.Combine(GamePathes.InterfacePath, textureFile1);
                        string modPath2 = Path.Combine(ModPathes.InterfacePath, textureFile2);
                        string gamePath2 = Path.Combine(GamePathes.InterfacePath, textureFile2);

                        return new MaskedShieldType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile1"),
                            MaskTexturePath = gfxBracket.GetVarString("textureFile2"),
                            MaskContent = File.Exists(modPath2) ? BitmapExtensions.LoadResourceFullPath(modPath2)
                                       : File.Exists(gamePath2) ? BitmapExtensions.LoadResourceFullPath(gamePath2) : null,
                            Content = File.Exists(modPath1) ? BitmapExtensions.LoadResourceFullPath(modPath1)
                                    : File.Exists(gamePath1) ? BitmapExtensions.LoadResourceFullPath(gamePath1) : null,
                            EffectFile = gfxBracket.GetVarString("effectFile")
                        };
                    }
                case "circularprogressbartype":
                    return new CircularProgressBarType
                    {
                        Id = new Identifier(gfxBracket.GetVarString("name")),
                        Content = BitmapExtensions.LoadResourceRealativePath(gfxBracket.GetVarString("textureFile1")),
                        EffectPath = gfxBracket.GetVarString("effectFile"),
                        TexturePath = gfxBracket.GetVarString("textureFile1"),
                        Size = gfxBracket.GetVarInt("size"),
                        Rotation = gfxBracket.GetVarInt("rotation"),
                        Amount = gfxBracket.GetVarInt("amount"),
                        EffectContent = BitmapExtensions.LoadResourceRealativePath(gfxBracket.GetVarString("effectFile2")),
                    };
                default:
                    return null;
            }

        }
        public static IGfx? ParseSingleObjGfx(Bracket gfxBracket)
        {
            string brName = gfxBracket.Name.ToLower();
            switch (brName)
            {
                case "arrowtype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string modPath = Path.Combine(ModPathes.InterfacePath, textureFile);
                        string gamePath = Path.Combine(GamePathes.InterfacePath, textureFile);

                        var resarrow = new ArrowType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("texture"),
                            EffectPath = gfxBracket.GetVarString("effect"),
                            SpecularPath = gfxBracket.GetVarString("specular"),
                            NormalPath = gfxBracket.GetVarString("normal"),
                           
                            
                        };
                        resarrow.Content = BitmapExtensions.LoadResourceRealativePath(resarrow.TexturePath);
                        resarrow.NormalContent = BitmapExtensions.LoadResourceRealativePath(resarrow.NormalPath);
                        resarrow.SpecularContent = BitmapExtensions.LoadResourceRealativePath(resarrow.SpecularPath);
                        return resarrow;
                    }


                default:
                    return null;
            }

        }
    }
}
