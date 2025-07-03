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
namespace ModdingManager.classes.gfx
{
    public class ImageManager
    {
        public ImageManager() { }

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
        public static Image<Rgba32> ConvertToImageSharp(System.Drawing.Image systemDrawingImage)
        {
            if (systemDrawingImage == null)
            {
                var emptyFlag = new Image<Rgba32>(82, 52);
                emptyFlag.Mutate(x => x.BackgroundColor(new Rgba32(255, 0, 255, 255)));
                return emptyFlag;
            }

            using (var ms = new MemoryStream())
            {
                systemDrawingImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                return SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(ms);
            }
        }

        public static ImageSource GetCombinedTechImage(ImageSource overlayimg, ImageSource backgroundimg, int type)
        {
            double renderWidth = 0;
            double renderHeight = 0;
            switch (type)
            {
                case 1:
                    renderWidth = 183;
                    renderHeight = 84;
                    break;
                case 2:
                    renderWidth = 62;
                    renderHeight = 62;
                    break;
            }

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                if (backgroundimg != null)
                {
                    dc.DrawImage(backgroundimg, new Rect(0, 0, renderWidth, renderHeight));
                }

                if (overlayimg != null)
                {
                    dc.DrawImage(overlayimg, new Rect(0, 0, renderWidth, renderHeight));
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);

            return bmp;
        }

        public static System.Windows.Controls.Image GetImageFromBorder(Border border)
        {
            if (border.Child is Canvas canvas)
            {
                return canvas.Children.OfType<System.Windows.Controls.Image>().FirstOrDefault();
            }
            return null;
        }

        public static Bitmap ConvertImageSourceToBitmap(ImageSource imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                using (var memoryStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);

                    using (var tempBitmap = new Bitmap(memoryStream))
                    {
                        return new Bitmap(tempBitmap);
                    }
                }
            }
            throw new ArgumentException("Невозможно преобразовать ImageSource в Bitmap — неподдерживаемый формат.");
        }


        public static BitmapSource CreateIndependentBitmapCopy(BitmapSource source)
        {
            if (source == null)
                return null;
            return new WriteableBitmap(source);
        }


        public static System.Drawing.Image ConvertToDrawingImage(BitmapSource bitmapSource)
        {
            if (bitmapSource == null)
                return null;

            using (var outStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder(); // <-- используем PNG, не BMP
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(outStream);
                outStream.Position = 0;

                return System.Drawing.Image.FromStream(outStream, true, true);
            }
        }
        public static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        public static SixLabors.ImageSharp.Image<Rgba32> ResizeStretch(Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image, int width, int height)
        {
            return image.Clone(x => x.Resize(width, height));
        }
        public static BitmapSource ConvertToBitmapSource(System.Drawing.Image image)
        {
            if (image == null)
                return null;

            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapSource RenderUIElementToBitmap(UIElement element, int width, int height)
        {
            var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(System.Windows.Media.Brushes.Transparent, null, new Rect(0, 0, width, height));

                var vb = new VisualBrush(element);
                ctx.DrawRectangle(vb, null, new Rect(0, 0, width, height));
            }

            renderBitmap.Render(dv);
            return renderBitmap;
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
