using ModdingManagerModels.Types.ObectCacheData;

namespace ModdingManagerClassLib.Extentions
{
    public static class ArrExtensions
    {
        public static System.Drawing.Color AsColor(this HoiArray arr)
        {
            return System.Drawing.Color.FromArgb((int)arr.Values[0], (int)arr.Values[1], (int)arr.Values[2]);
        }
    }
}
