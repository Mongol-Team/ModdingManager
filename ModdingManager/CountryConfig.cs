using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManager
{
    public class CountryConfig
    {

        public string Tag { get; set; }
        public string Name { get; set; }
        public int Capital { get; set; }
        public string GraphicalCulture { get; set; }
        public string Color { get; set; }
        public List<string> Technologies { get; set; } = new List<string>();
        public int Convoys { get; set; }
        public string OOB { get; set; }
        public int Stab {  get; set; }
        public int WarSup {  get; set; }
        public int ResearchSlots { get; set; }
        public string RulingParty { get; set; }
        public string LastElection { get; set; }
        public int ElectionFrequency { get; set; }
        public bool ElectionsAllowed { get; set; }
        public int NeutralityPopularity { get; set; }
        public int FascismPopularity { get; set; }
        public int CommunismPopularity { get; set; }
        public int DemocraticPopularity { get; set; }
        public List<string> Ideas { get; set; } = new List<string>();
        public List<string> Characters { get; set; } = new List<string>();
        public Dictionary<int, bool> States { get; set; } = new Dictionary<int, bool>();
    }
}
