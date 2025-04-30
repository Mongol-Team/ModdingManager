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
using ModdingManager.classes;
namespace ModdingManager.managers.gfx
{
    public class ImageManager
    {
        public ImageManager() { }


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
                return SixLabors.ImageSharp.Image.Load<Rgba32>(ms);
            }
        }

        public static Image<Rgba32> ResizeStretch(Image<Rgba32> image, int width, int height)
        {
            return image.Clone(x => x.Resize(width, height));
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

                    // Получаем абсолютные значения масштаба
                    double scaleX = imageArg.ScaleX == 0 ? 1 : Math.Abs(imageArg.ScaleX);
                    double scaleY = imageArg.ScaleY == 0 ? 1 : Math.Abs(imageArg.ScaleY);

                    // Рассчитываем размеры после масштабирования
                    double scaledWidth = originalWidth * scaleX;
                    double scaledHeight = originalHeight * scaleY;

                    if (imageArg.IsCompresed)
                    {
                        // Для сжатых изображений просто растягиваем на всю область
                        dc.DrawImage(source, new Rect(0, 0, width, height));
                    }
                    else
                    {
                        // Начальная позиция (центр у левого/верхнего края)
                        double x = -scaledWidth / 2 + imageArg.OffsetX;
                        double y = -scaledHeight / 2 + imageArg.OffsetY;

                        // Создаем группу преобразований
                        var transformGroup = new TransformGroup();

                        // Масштабирование
                        transformGroup.Children.Add(new ScaleTransform(
                            imageArg.ScaleX == 0 ? 1 : imageArg.ScaleX,
                            imageArg.ScaleY == 0 ? 1 : imageArg.ScaleY,
                            x + scaledWidth / 2,
                            y + scaledHeight / 2
                        ));

                        // Применяем преобразования
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
