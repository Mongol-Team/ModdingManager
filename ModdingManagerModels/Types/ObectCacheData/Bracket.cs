namespace ModdingManagerModels.Types.ObectCacheData
{
    public class Bracket
    {
        public List<Var> SubVars { get; set; } = new List<Var>();
        public List<Bracket> SubBrackets { get; set; } = new List<Bracket>();
        public string Name { get; set; } = string.Empty;

    }
}
