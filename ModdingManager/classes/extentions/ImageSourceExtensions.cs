using System.IO;
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
}
