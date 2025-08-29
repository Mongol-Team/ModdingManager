using ModdingManagerModels.Interfaces;
using ModdingManagerModels.Types.ObectCacheData;

namespace ModdingManagerModels.Types.ObjectCacheData
{
    public class HoiFuncFile : IHoiData
    {
        public List<Bracket> Brackets { get; set; } = new List<Bracket>();
        public List<Var> Vars { get; set; } = new List<Var>();
        public List<HoiArray> Arrays { get; set; } = new List<HoiArray>();
        public string FilePath { get; set; }
    }
}