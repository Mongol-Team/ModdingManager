using Models.Configs.HoiConfigs;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.extentions
{
    public static class ModExtensios
    {
        public static IGui FindGuiType(this HoiModConfig mod, string id) 
        {
            return mod.GuiFiles.SelectMany(g => g.Entities).FirstOrDefault(g => id == g.Id);
        }
    }
}
