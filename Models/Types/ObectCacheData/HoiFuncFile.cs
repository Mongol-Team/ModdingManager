using Models.Interfaces;
using Models.Types.ObectCacheData;

namespace Models.Types.ObjectCacheData
{
    public class HoiFuncFile : IHoiData
    {
        public List<Bracket> Brackets { get; set; } = new List<Bracket>();
        public List<Var> Vars { get; set; } = new List<Var>();
        public List<HoiArray> Arrays { get; set; } = new List<HoiArray>();
        public string FilePath { get; set; }
    }
}