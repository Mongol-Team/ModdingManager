using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeximpNet;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Drawing.Imaging;
using ModdingManager.classes.args;
using System.Text.RegularExpressions;
using ModdingManager.managers.utils;
using ModdingManager.classes.extentions;
namespace ModdingManager.classes.gfx
{
    public static class ImageManager
    {
        public static Bitmap LoadAndCropRightSideOfIcon(string filePath)
        {
            static Bitmap CropLeftHalf(Bitmap original)
            {
                int halfWidth = original.Width / 2;
                var rect = new System.Drawing.Rectangle(0, 0, halfWidth, original.Height);
                return original.Clone(rect, original.PixelFormat);
            }

            try
            {
                using var fullBitmap = DDSManager.LoadDDSAsBitmap(filePath);
                return CropLeftHalf(fullBitmap);
            }
            catch
            {
                return null;
            }
        }

        public static System.Drawing.Image FindUnitIcon(string unitName)
        {
            var directoriesToSearch = new[]
            {
                ModManager.GameDirectory,
                ModManager.Directory
            };

            foreach (var dir in directoriesToSearch)
            {
                if (!Directory.Exists(dir)) continue;

                var gfxFiles = Directory.GetFiles(dir, "*.gfx", SearchOption.AllDirectories);
                foreach (var file in gfxFiles)
                {
                    var content = File.ReadAllText(file);
                    var spriteBlocks = Regex.Matches(content, @"spriteType\s*=\s*\{([^\}]+)\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                    foreach (Match block in spriteBlocks)
                    {
                        var blockContent = block.Groups[1].Value;

                        var nameMatch = Regex.Match(blockContent, $@"name\s*=\s*""GFX_unit_{Regex.Escape(unitName)}_icon_medium""", RegexOptions.IgnoreCase);
                        if (!nameMatch.Success) continue;

                        var textureMatch = Regex.Match(blockContent, @"textureFile\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);
                        if (!textureMatch.Success) continue;

                        var texturePath = textureMatch.Groups[1].Value.Replace('/', Path.DirectorySeparatorChar);
                        var fullTexturePath = Path.Combine(dir, texturePath);

                        if (File.Exists(fullTexturePath))
                        {
                            try
                            {
                                return ImageManager.LoadAndCropRightSideOfIcon(fullTexturePath);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }

            return Properties.Resources.null_item_image;
        }

        
        public static ImageSource GetCombinedImages(List<ImageSourceArg> images, int width, int height)
        {
            if (images == null || images.Count == 0)
            {
                return new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            }

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                foreach (var imageArg in images)
                {
                    if (imageArg?.Source == null) continue;

                    var source = imageArg.Source;
                    double originalWidth = source.Width;
                    double originalHeight = source.Height;

                    double scaleX = imageArg.ScaleX == 0 ? 1 : Math.Abs(imageArg.ScaleX);
                    double scaleY = imageArg.ScaleY == 0 ? 1 : Math.Abs(imageArg.ScaleY);

                    double scaledWidth = originalWidth * scaleX;
                    double scaledHeight = originalHeight * scaleY;

                    if (imageArg.IsCompresed)
                    {
                        dc.DrawImage(source, new Rect(0, 0, width, height));
                    }
                    else
                    {
                        double x = -scaledWidth / 2 + imageArg.OffsetX;
                        double y = -scaledHeight / 2 + imageArg.OffsetY;

                        var transformGroup = new TransformGroup();

                        transformGroup.Children.Add(new ScaleTransform(
                            imageArg.ScaleX == 0 ? 1 : imageArg.ScaleX,
                            imageArg.ScaleY == 0 ? 1 : imageArg.ScaleY,
                            x + scaledWidth / 2,
                            y + scaledHeight / 2
                        ));

                        dc.PushTransform(transformGroup);
                        dc.DrawImage(source, new Rect(x, y, scaledWidth, scaledHeight));
                        dc.Pop();
                    }
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);

            return bmp;
        }

    }
}
