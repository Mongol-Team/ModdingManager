using ModdingManagerModels.Types.ObectCacheData;

namespace ModdingManagerModels.Types.ObjectCacheData
{
    public class Bracket
    {
        public List<Var> SubVars { get; set; } = new List<Var>();
        public List<Bracket> SubBrackets { get; set; } = new List<Bracket>();
        public List<HoiArray> Arrays { get; set; } = new List<HoiArray>();
        public string Name { get; set; } = string.Empty;

    }
}
