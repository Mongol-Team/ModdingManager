using Models.Interfaces;

namespace Models.Args
{
    /// <summary>
    /// Аргументы события двойного клика по сущности
    /// </summary>
    public class EntityDoubleClickEventArg
    {
        /// <summary>
        /// Сущность, по которой кликнули
        /// </summary>
        public object Entity { get; set; }

        /// <summary>
        /// Название слоя
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Базовая сущность (IBasicMapEntity), если клик был по ней
        /// </summary>
        public IBasicMapEntity BasicEntity { get; set; }

        /// <summary>
        /// Составная сущность (IMapEntity), если клик был по ней
        /// </summary>
        public IMapEntity MapEntity { get; set; }
    }

    // Добавить в Models.Args или в существующий файл с аргументами событий
    public class EntityMoveEventArg
    {
        public int BasicEntityId { get; set; }
        public IMapEntity SourceParent { get; set; } // null если источник - неприсвоенная сущность
        public IMapEntity TargetParent { get; set; }
        public string LayerName { get; set; }
        public object MovedChild { get; set; }
        public bool IsUnassignedSource { get; set; } // true если перемещаем неприсвоенную сущность
        public bool AllowMove { get; set; } = true; // По умолчанию разрешаем перемещение
    }

    /// <summary>
    /// Аргументы события одиночного клика
    /// </summary>
    public class EntityClickEventArg
    {
        /// <summary>
        /// Сущность, по которой кликнули
        /// </summary>
        public object Entity { get; set; }

        /// <summary>
        /// Название слоя
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Базовая сущность (IBasicMapEntity), если клик был по ней
        /// </summary>
        public IBasicMapEntity BasicEntity { get; set; }

        /// <summary>
        /// Составная сущность (IMapEntity), если клик был по ней
        /// </summary>
        public IMapEntity MapEntity { get; set; }

        /// <summary>
        /// Координаты клика (X)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Координаты клика (Y)
        /// </summary>
        public double Y { get; set; }
    }
}
