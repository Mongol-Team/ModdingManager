using Models.Args;
using Models.Enums;
using Models.Types.Utils;
using System.Drawing;

namespace Models.Interfaces
{
    /// <summary>
    /// Базовая сущность карты (листья дерева) - например Province
    /// Содержит Shape для отображения
    /// </summary>
    public interface IBasicMapEntity
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        Identifier Id { get; set; }

        /// <summary>
        /// Форма сущности для отображения на карте (ShapeArg)
        /// </summary>
        ProvinceShapeArg Shape { get; set; }

        /// <summary>
        /// Цвет отображения (может быть переопределён родительской сущностью)
        /// </summary>
        Color? Color { get; set; }
    }
}
