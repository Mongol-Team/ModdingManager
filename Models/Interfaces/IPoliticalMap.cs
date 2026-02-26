using Models.Types.Utils;
using System.Drawing;

namespace Models.Interfaces
{
    /// <summary>
    /// Политическая карта - корень всей структуры
    /// </summary>
    public interface IPoliticalMap
    {
        /// <summary>
        /// Базовые сущности карты (обычно провинции)
        /// Это все листья дерева в плоском виде
        /// </summary>
        IEnumerable<IBasicMapEntity> Basic { get; }

        /// <summary>
        /// Изображение карты для парсинга в shapes
        /// </summary>
        Bitmap MapImage { get; set; }

        /// <summary>
        /// Получить все слои карты
        /// Каждый слой - это коллекция IMapEntity одного типа/уровня
        /// Например: ("States", [state1, state2, ...]), ("Countries", [country1, country2, ...])
        /// </summary>
        /// <returns>Пары (Название слоя, Коллекция IMapEntity)</returns>
        IEnumerable<(string LayerName, List<IMapEntity> Entities)> GetLayers();
    }
}
