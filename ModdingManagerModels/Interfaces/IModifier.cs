using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModdingManagerModels.Interfaces
{
    public interface IModifier
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
