using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class ImageSourceExtensions
{
    public static BitmapSource ToBitmapSource(this ImageSource imageSource)
    {
        if (imageSource is BitmapSource bitmapSource)
        {
            return bitmapSource;
        }

        var renderTarget = new RenderTargetBitmap(
            (int)imageSource.Width, (int)imageSource.Height, 96, 96, PixelFormats.Pbgra32);

        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            dc.DrawImage(imageSource, new System.Windows.Rect(0, 0, imageSource.Width, imageSource.Height));
        }

        renderTarget.Render(dv);
        return renderTarget;
    }
    public static ImageSource GetCombinedTechImage(this ImageSource overlayimg, ImageSource backgroundimg, int type)
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
    public static DrawingImage ToDrawingImage(this ImageSource source)
    {
        if (source is DrawingImage drawingImage)
        {
            return drawingImage;
        }

        var imageDrawing = new ImageDrawing
        {
            ImageSource = source,
            Rect = new Rect(0, 0, source.Width, source.Height)
        };

        var drawingGroup = new DrawingGroup();
        drawingGroup.Children.Add(imageDrawing);

        var result = new DrawingImage(drawingGroup);
        result.Freeze(); // для повышения производительности

        return result;
    }
    public static System.Drawing.Image ToDrawingDotImage(this ImageSource imageSource)
    {
        return imageSource.ToBitmap(); // если ToBitmap() возвращает System.Drawing.Bitmap
    }
    public static ImageSource ToImageSource(this byte[] imageData, int dpi = 96)
    {
        if (imageData == null || imageData.Length == 0)
            return null;

        BitmapImage bitmap;
        using (var stream = new MemoryStream(imageData))
        {
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
        }

        // Если уже 96 — возвращаем без рескейла
        if (Math.Abs(bitmap.DpiX - dpi) < 0.1 && Math.Abs(bitmap.DpiY - dpi) < 0.1)
            return bitmap;

        // Вычисляем коэффициент масштабирования
        double scaleX = dpi / bitmap.DpiX;
        double scaleY = dpi / bitmap.DpiY;

        // Применяем трансформацию
        var transformed = new TransformedBitmap(bitmap, new ScaleTransform(scaleX, scaleY));
        transformed.Freeze();

        // Конвертируем в RenderTargetBitmap с новым DPI
        int newWidth = (int)(bitmap.PixelWidth * scaleX);
        int newHeight = (int)(bitmap.PixelHeight * scaleY);

        var target = new RenderTargetBitmap(newWidth, newHeight, dpi, dpi, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawImage(transformed, new Rect(0, 0, newWidth, newHeight));
        }
        target.Render(visual);
        target.Freeze();

        return target;
    }


    // Вспомогательный метод для извлечения пикселей
    private static byte[] GetPixels(this BitmapSource source)
    {
        int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
        var pixels = new byte[stride * source.PixelHeight];
        source.CopyPixels(pixels, stride, 0);
        return pixels;
    }

    public static Bitmap ToBitmap(this ImageSource imageSource)
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
}
