using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager.configs
{
    public class CountryCharacterConfig
    {
        public CountryCharacterConfig() { }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Ideology { get; set; } = "";
        public List<string> Types { get; set; } = new List<string>();
        public string Tag { get; set; } = "";
        public List<string> Traits { get; set; } = new List<string>();
        public int Skill { get; set; } = 1;
        public int Attack { get; set; } = 1;
        public int Defense { get; set; } = 1;
        public int Supply { get; set; } = 1;
        public int Speed { get; set; } = 1;
        public string AdvisorSlot { get; set; } = "";
        public int AdvisorCost { get; set; } = 100;
        public string AiWillDo { get; set; } = "20";
        public string Expire { get; set; } = "";
        public Image SmallImage { get; set; }
        public Image BigImage { get; set; }
    }
}
