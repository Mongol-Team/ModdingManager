using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;
namespace ModdingManager.configs
{
    public class SupereventConfig
    {
        public SupereventConfig() { }
        public string Id { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public System.Windows.Controls.Canvas EventConstructor { get; set; }
        public string SoundPath { get; set; }

    }
}
