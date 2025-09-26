using CommunityToolkit.HighPerformance.Helpers;
using ModdingManagerModels.Types.ObectCacheData;
using System.Windows.Documents;

namespace ModdingManagerClassLib.Extentions
{
    public static class ArrExtensions
    {
        public static System.Drawing.Color AsColor(this HoiArray arr)
        {
            if (arr.Values.Count == 3)
            {
                return System.Drawing.Color.FromArgb((int)arr.Values[0], (int)arr.Values[1], (int)arr.Values[2]);
            }
            else if(arr.Values.Count == 4)
            {
                return System.Drawing.Color.FromArgb((int)arr.Values[3], (int)arr.Values[0], (int)arr.Values[1], (int)arr.Values[2]);
            }
            throw new InvalidCastException("Array does not contain 3 elements");
        }

    }
}
