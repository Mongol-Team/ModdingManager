using Models.Configs;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Interfaces
{
    /// <summary>
    /// Составная сущность карты - например State, Country
    /// Может содержать как IBasicMapEntity, так и другие IMapEntity (вложенность)
    /// </summary>
    public interface IMapEntity
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        Identifier Id { get; set; }

        /// <summary>
        /// Цвет отображения для всех вложенных сущностей
        /// </summary>
        Color? Color { get; set; }

        /// <summary>
        /// Получить все базовые сущности (IBasicMapEntity) этой сущности
        /// Рекурсивно проходит по всем вложенным IMapEntity и собирает их Basic
        /// </summary>
        /// <returns>Плоский список всех IBasicMapEntity</returns>
        IEnumerable<IBasicMapEntity> GetAllBasicEntities();

        /// <summary>
        /// Получить прямые дочерние сущности
        /// Может возвращать как IBasicMapEntity, так и IMapEntity
        /// </summary>
        /// <returns>Список дочерних сущностей</returns>
        IEnumerable<object> GetChildren();

        /// <summary>
        /// Добавить дочернюю сущность
        /// </summary>
        void AddChild(object child);

        /// <summary>
        /// Удалить дочернюю сущность
        /// </summary>
        void RemoveChild(object child);
    }
}
