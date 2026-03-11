using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Enums.Gui
{
    /// <summary>
    /// Кнопка мыши для drag-scroll
    /// </summary>
    [Flags]
    public enum GuiDragScrollButtonType
    {
        None = 0,
        Left = 1,
        Middle = 2,
        Right = 4
    }
}
