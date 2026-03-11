using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Enums.Gui
{
    /// <summary>
    /// Определяет точку привязки элемента относительно его контейнера или экрана
    /// </summary>
    public enum GuiOrientationType
    {
        Center,
        CenterUp,
        CenterDown,
        CenterLeft,
        CenterRight,
        UpperLeft,
        LowerLeft,
        UpperRight,
        LowerRight
    }
}
