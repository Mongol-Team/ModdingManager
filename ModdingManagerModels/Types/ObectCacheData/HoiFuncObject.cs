using ModdingManagerModels.Interfaces;

namespace ModdingManagerModels.Types.ObectCacheData
{
    public class HoiFunkFile : IHoiData
    {
        public List<Bracket> Brackets { get; set; } = new List<Bracket>();
        public List<Var> Vars { get; set; } = new List<Var>();
        public string FilePath { get; set; }
    }
}