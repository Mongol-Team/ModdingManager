using Models.Types.ObectCacheData;
using System.Text;

namespace Models.Types.ObjectCacheData
{
    public class Bracket
    {
        public List<Var> SubVars { get; set; } = new List<Var>();
        public List<Bracket> SubBrackets { get; set; } = new List<Bracket>();
        public List<HoiArray> Arrays { get; set; } = new List<HoiArray>();
        public string Name { get; set; } = string.Empty;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Name} = {{");
            foreach (Var var in this.SubVars)
            {
                sb.AppendLine($"\t{var.Name} = {var.Value}");
            }
            foreach (Bracket br in this.SubBrackets)
            {
                sb.AppendLine($"\t{br.ToString().Replace("\n", "\n\t")}");
            }
            foreach (HoiArray arr in this.Arrays)
            {
                sb.AppendLine($"\t{arr.Name} = {{");
                foreach (var item in arr.Values)
                {
                    sb.AppendLine($"\t\t{item.ToString()}");
                }
                sb.AppendLine("\t}");
            }
            return sb.AppendLine("}").ToString();
        }
    }
}
