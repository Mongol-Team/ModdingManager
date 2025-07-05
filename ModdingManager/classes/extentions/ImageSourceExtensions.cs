using System.IO;
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
