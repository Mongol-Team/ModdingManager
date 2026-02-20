using Application.Debugging;
using Application.Extentions;
using Application.Settings;
using Application.utils.Math;
using Application.utils.Pathes;
using Models.EntityFiles;
using Models.GfxTypes;
using Models.Interfaces;
using Models.Types.ObjectCacheData;
using Models.Types.Utils;
using RawDataWorker.Parsers;
using RawDataWorker.Parsers.Patterns;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using DDF = Data.DataDefaultValues;
namespace Application.Loaders
{
    public static class GfxLoader
    {
        public static List<GfxFile<IGfx>> LoadAll()
        {
            var stopwatch = Stopwatch.StartNew();
            var gfxFiles = new List<GfxFile<IGfx>>();

            string[] possiblePaths =
            {
                GamePathes.InterfacePath,
                ModPathes.InterfacePath
            };

            ConcurrentDictionary<string, IGfx> gfxDictionary = new(); // для отслеживания уникальности по id
            int totalLoaded = 0;

            int maxDegreeOfParallelism = ParallelTaskCounter.CalculateMaxDegreeOfParallelism();
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

            foreach (string path in possiblePaths)
            {
                if (!Directory.Exists(path)) continue;

                string[] files = Directory.GetFiles(path, "*.gfx", SearchOption.AllDirectories);
                Parallel.ForEach(files, parallelOptions, file =>
                {
                    try
                    {
                        var configFile = LoadFromFile(file, path.StartsWith(ModPathes.InterfacePath));

                        if (configFile.Entities.Any())
                        {
                            lock (gfxFiles)
                            {
                                gfxFiles.Add(configFile);
                            }

                            foreach (var gfx in configFile.Entities)
                            {
                                if (gfx == null || gfx.Id == null) continue;

                                string idKey = gfx.Id.ToString().ToLower();
                                bool wasAdded = gfxDictionary.TryAdd(idKey, gfx);
                                if (!wasAdded)
                                {
                                    Logger.AddDbgLog($"Переопределение GFX с ID '{idKey}' из файла: {Path.GetFileName(file)}");
                                    gfxDictionary[idKey] = gfx; // мод переопределяет
                                }
                                else
                                {
                                    Logger.AddDbgLog($"Добавлен новый GFX с ID '{idKey}' из файла: {Path.GetFileName(file)}");
                                }

                                Interlocked.Increment(ref totalLoaded);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.AddDbgLog($"Ошибка загрузки GFX-файла '{file}': {ex.Message}");
                    }
                });
            }

            stopwatch.Stop();
            Logger.AddLog($"Загрузка GFX завершена. Время: {stopwatch.ElapsedMilliseconds} мс. " +
                          $"Файлов: {gfxFiles.Count}, уникальных GFX: {gfxDictionary.Count}, обработано всего: {totalLoaded}");

            return gfxFiles;
        }

        /// <summary>
        /// Парсит один .gfx файл и возвращает его как ConfigFile<IGfx>
        /// </summary>
        public static GfxFile<IGfx> LoadFromFile(string gfxFilePath, bool isOverride)
        {
            var configFile = new GfxFile<IGfx>
            {
                FileFullPath = gfxFilePath,
                IsOverride = isOverride
            };

            HoiFuncFile funcFile;
            try
            {
                var parser = new TxtParser(new TxtPattern());
                funcFile = parser.Parse(gfxFilePath) as HoiFuncFile;

                if (parser.healer?.Errors?.Any() == true)
                {
                    ModDataStorage.TxtErrors.AddRangeSafe(parser.healer.Errors);
                }
            }
            catch (Exception ex)
            {
                Logger.AddDbgLog($"Не удалось распарсить GFX-файл {gfxFilePath}: {ex.Message}");
                return configFile;
            }

            if (funcFile == null || funcFile.Brackets.Count == 0)
            {
                Logger.AddDbgLog($"Файл {gfxFilePath} пуст или не содержит блоков");
                return configFile;
            }

            foreach (Bracket defineBr in funcFile.Brackets)
            {
                if (defineBr.Name == "spriteTypes")
                {
                    foreach (Bracket spriteBr in defineBr.SubBrackets)
                    {
                        IGfx gfx = ParseSingleSpriteGfx(spriteBr, gfxFilePath);
                        if (gfx != null)
                            configFile.Entities.Add(gfx);
                    }
                }
                else if (defineBr.Name == "objectTypes")
                {
                    foreach (Bracket spriteBr in defineBr.SubBrackets)
                    {
                        IGfx gfx = ParseSingleObjGfx(spriteBr);
                        if (gfx != null)
                            configFile.Entities.Add(gfx);
                    }
                }
            }

            return configFile;
        }
        public static IGfx? ParseSingleSpriteGfx(Bracket gfxBracket, string fullPath)
        {
            string brName = gfxBracket.Name.ToLower();
            string dd = gfxBracket.GetVarString("name");
            if (dd == "GFX_idea_LMM_last_time_of_liberia_lion")
            {
                var pat = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                var cnt = BitmapExtensions.LoadResourceRealativePath(pat);
                var fim = "serf";
            }
            switch (brName)
            {
                case "textspritetype":
                    {
                        var res = new TextSpriteType()
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile").Replace("/", "\\"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            Content = BitmapExtensions.LoadResourceRealativePath(gfxBracket.GetVarString("textureFile").Replace("/", "\\")),
                            FileFullPath = fullPath,
                        };
                        return res;
                    }
                case "spritetype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string name = gfxBracket.GetVarString("name");

                        string modPath = Path.Combine(ModPathes.RootPath, textureFile);
                        string gamePath = Path.Combine(GamePathes.RootPath, textureFile);

                        var res = new SpriteType()
                        {
                            Id = new Identifier(name),
                            TexturePath = gfxBracket.GetVarString("textureFile").Replace("/", "\\"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            AllwaysTransparent = gfxBracket.GetVarBool("alwaystransparent"),
                            LegacyLazyLoad = gfxBracket.GetVarBool("legacy_lazy_load"),
                            TransparenceCheck = gfxBracket.GetVarBool("transparencecheck"),
                            FileFullPath = fullPath,
                            Content = File.Exists(modPath) ? BitmapExtensions.LoadResourceFullPath(modPath)
                                   : File.Exists(gamePath) ? BitmapExtensions.LoadResourceFullPath(gamePath) : DDF.NullImageSource

                        };
                        return res;
                    }

                case "frameanimatedspritetype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string modPath = Path.Combine(ModPathes.RootPath, textureFile);
                        string gamePath = Path.Combine(GamePathes.RootPath, textureFile);

                        return new FrameAnimatedSpriteType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            AnimationRateFps = gfxBracket.GetVarInt("animation_rate_fps"),
                            Content = File.Exists(modPath) ? BitmapExtensions.LoadResourceFullPath(modPath)
                                   : File.Exists(gamePath) ? BitmapExtensions.LoadResourceFullPath(gamePath) : DDF.NullImageSource,
                            Looping = gfxBracket.GetVarBool("looping"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            AllwaysTransparent = gfxBracket.GetVarBool("alwaystransparent"),
                            PlayOnShow = gfxBracket.GetVarBool("play_on_show"),
                            FileFullPath = fullPath,
                            PauseOnLoop = gfxBracket.GetVarDouble("pause_on_loop")
                        };
                    }

                case "progressbartype":
                    {
                        string textureFile1 = gfxBracket.GetVarString("textureFile1").Replace("/", "\\");
                        string textureFile2 = gfxBracket.GetVarString("textureFile2").Replace("/", "\\");
                        string modPath1 = Path.Combine(ModPathes.RootPath, textureFile1);
                        string gamePath1 = Path.Combine(GamePathes.RootPath, textureFile1);
                        string modPath2 = Path.Combine(ModPathes.RootPath, textureFile2);
                        string gamePath2 = Path.Combine(GamePathes.RootPath, textureFile2);

                        return new ProgressbarType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile1"),
                            SecondTexturePath = gfxBracket.GetVarString("textureFile2"),
                            Content = File.Exists(modPath1) ? BitmapExtensions.LoadResourceFullPath(modPath1)
                                   : File.Exists(gamePath1) ? BitmapExtensions.LoadResourceFullPath(gamePath1) : DDF.NullImageSource,
                            BgContent = File.Exists(modPath2) ? BitmapExtensions.LoadResourceFullPath(modPath2)
                                     : File.Exists(gamePath2) ? BitmapExtensions.LoadResourceFullPath(gamePath2) : DDF.NullImageSource,
                            Size = new System.Drawing.Point(
                                gfxBracket.GetSubBracketVarInt("size", "x"),
                                gfxBracket.GetSubBracketVarInt("size", "y")),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            Color = gfxBracket.GetArrayColor("color"),
                            FileFullPath = fullPath,
                            SecondColor = gfxBracket.GetArrayColor("colortwo"),
                            IsHorisontal = false,
                            Steps = -1
                        };
                    }

                case "corneredtilespritetype":
                    {
                        string textureFile = gfxBracket.GetVarString("textureFile").Replace("/", "\\");
                        string modPath = Path.Combine(ModPathes.RootPath, textureFile);
                        string gamePath = Path.Combine(GamePathes.RootPath, textureFile);

                        return new CorneredTileSpriteType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile"),
                            NoOfFrames = gfxBracket.GetVarInt("noOfFrames"),
                            EffectFile = gfxBracket.GetVarString("effectFile"),
                            AllwaysTrancparent = gfxBracket.GetVarBool("alwaystransparent"),
                            Content = File.Exists(modPath) ? BitmapExtensions.LoadResourceFullPath(modPath)
                                   : File.Exists(gamePath) ? BitmapExtensions.LoadResourceFullPath(gamePath) : DDF.NullImageSource,
                            TilingCenter = gfxBracket.GetVarBool("tilingCenter"),
                            Looping = gfxBracket.GetVarBool("looping"),
                            AnimationRateSpf = gfxBracket.GetVarInt("animation_rate_spf"),
                            Size = new System.Drawing.Point(
                                gfxBracket.GetSubBracketVarInt("size", "x"),
                                gfxBracket.GetSubBracketVarInt("size", "y")),
                            FileFullPath = fullPath,
                            BorderSize = new System.Drawing.Point(
                                gfxBracket.GetSubBracketVarInt("borderSize", "x"),
                                gfxBracket.GetSubBracketVarInt("borderSize", "y"))
                        };
                    }
                case "piecharttype":
                    {

                        return new PieChartType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            FileFullPath = fullPath,
                            Size = gfxBracket.GetVarInt("size"),
                            Colors = gfxBracket.Arrays.FirstOrDefault(a => a.Name == "colors")?.Values.Cast<Color>().ToList() ?? new List<Color>()
                        };
                    }
                case "linecharttype":
                    {
                        return new LineChartType()
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            Size = new System.Drawing.Point(
                                gfxBracket.GetSubBracketVarInt("size", "x"),
                                gfxBracket.GetSubBracketVarInt("size", "y")),
                            FileFullPath = fullPath,
                            LineWidth = gfxBracket.GetVarDouble("lineWidth"),
                        };
                    }
                case "maskedshieldtype":
                    {
                        string textureFile1 = gfxBracket.GetVarString("textureFile1").Replace("/", "\\");
                        string textureFile2 = gfxBracket.GetVarString("textureFile2").Replace("/", "\\");
                        string modPath1 = Path.Combine(ModPathes.RootPath, textureFile1);
                        string gamePath1 = Path.Combine(GamePathes.RootPath, textureFile1);
                        string modPath2 = Path.Combine(ModPathes.RootPath, textureFile2);
                        string gamePath2 = Path.Combine(GamePathes.RootPath, textureFile2);

                        return new MaskedShieldType
                        {
                            Id = new Identifier(gfxBracket.GetVarString("name")),
                            TexturePath = gfxBracket.GetVarString("textureFile1"),
                            MaskTexturePath = gfxBracket.GetVarString("textureFile2"),
                            MaskContent = File.Exists(modPath2) ? BitmapExtensions.LoadResourceFullPath(modPath2)
                                       : File.Exists(gamePath2) ? BitmapExtensions.LoadResourceFullPath(gamePath2) : DDF.NullImageSource,
                            Content = File.Exists(modPath1) ? BitmapExtensions.LoadResourceFullPath(modPath1)
                                    : File.Exists(gamePath1) ? BitmapExtensions.LoadResourceFullPath(gamePath1) : DDF.NullImageSource,
                            FileFullPath = fullPath,
                            EffectFile = gfxBracket.GetVarString("effectFile")
                        };
                    }
                case "circularprogressbartype":
                    return new CircularProgressBarType
                    {
                        Id = new Identifier(gfxBracket.GetVarString("name")),
                        Content = BitmapExtensions.LoadResourceRealativePath(gfxBracket.GetVarString("textureFile1")),
                        EffectPath = gfxBracket.GetVarString("effectFile"),
                        TexturePath = gfxBracket.GetVarString("textureFile1").Replace("/", "\\"),
                        Size = gfxBracket.GetVarInt("size"),
                        Rotation = gfxBracket.GetVarInt("rotation"),
                        Amount = gfxBracket.GetVarInt("amount"),
                        FileFullPath = fullPath,
                        EffectContent = BitmapExtensions.LoadResourceRealativePath(gfxBracket.GetVarString("effectFile2").Replace("/", "\\")),
                    };
                default:
                    return new SpriteType(DDF.NullImageSource, DDF.Null);
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
                        string modPath = Path.Combine(ModPathes.RootPath, textureFile);
                        string gamePath = Path.Combine(GamePathes.RootPath, textureFile);

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
                    return new SpriteType(DDF.NullImageSource, DDF.Null);
            }

        }
    }
}
