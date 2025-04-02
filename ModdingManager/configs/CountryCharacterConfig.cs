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
        public CountryCharacterConfig(
            string id,
            string name,
            string description,
            List<string> types,
            string tag,
            List<string> traits,
            int skill,
            int attack,
            int defense,
            int supply,
            int speed,
            string advisorSlot,
            int advisorCost,
            string aiWillDo,
            string expire)
        {
            Id = id;
            Name = name;
            Description = description;
            Types = types ?? new List<string>();
            Tag = tag;
            Traits = traits ?? new List<string>();
            Skill = skill;
            Attack = attack;
            Defense = defense;
            Supply = supply;
            Speed = speed;
            AdvisorSlot = advisorSlot;
            AdvisorCost = advisorCost;
            AiWillDo = aiWillDo;
            Expire = expire;
        }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
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
        public string AiWillDo { get; set; } = "";
        public string Expire { get; set; } = "";
        //[JsonIgnore]
        public string BigIconPath { get; set; } = "";
        //[JsonIgnore]
        public string SmallIconPath { get; set; } = "";
    }
}
