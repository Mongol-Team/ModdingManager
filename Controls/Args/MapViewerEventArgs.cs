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

    /// <summary>
    /// Аргументы события перемещения сущности
    /// </summary>
    public class EntityMoveEventArg
    {
        /// <summary>
        /// ID перемещаемой базовой сущности
        /// </summary>
        public int BasicEntityId { get; set; }

        /// <summary>
        /// Исходная родительская сущность
        /// </summary>
        public IMapEntity SourceParent { get; set; }

        /// <summary>
        /// Целевая родительская сущность
        /// </summary>
        public IMapEntity TargetParent { get; set; }

        /// <summary>
        /// Название слоя
        /// </summary>
        public string LayerName { get; set; }
        /// <summary>
        /// Перемещаемая дочерняя сущность (может быть IBasicMapEntity или IMapEntity)
        /// Это непосредственный ребёнок SourceParent
        /// </summary>
        public object MovedChild { get; set; }
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
