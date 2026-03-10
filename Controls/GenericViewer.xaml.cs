using Application;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Controls;
using UserControl = System.Windows.Controls.UserControl;
using Models.Configs.HoiConfigs;

namespace Controls
{
    public partial class GenericViewer : UserControl
    {
        private Type _configType;
        private object _configInstance;
        private PropertyInfo _listProperty;
        /// <summary>
        /// Вызывается при смене отображаемой сущности через <see cref="ShowEntity"/>.
        /// Аргумент — новый объект (может быть null).
        /// </summary>
        public event Action<object> EntityChanged;

        // ─── Свойство ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Текущий отображаемый объект.
        /// </summary>
        public object CurrentEntity => _configInstance;

        // ─── Метод ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Меняет отображаемую сущность без пересоздания контрола.
        /// Вызывает <see cref="EntityChanged"/> после смены.
        /// </summary>
        /// <param name="entity">Новый объект. Если null — очищает viewer.</param>
        public void ShowEntity(object entity)
        {
            _configInstance = entity;

            if (entity != null)
            {
                _configType = entity.GetType();
                _listProperty = FindListPropertyInModConfig(_configType);
                ConfigViewer.BuildingContent = entity;
            }
            else
            {
                ConfigViewer.BuildingContent = null;
            }

            EntityChanged?.Invoke(entity);
        }
        public GenericViewer(Type configType, object configInstance)
        {
            InitializeComponent();
            
            _configType = configType ?? throw new ArgumentNullException(nameof(configType));
            _configInstance = configInstance ?? throw new ArgumentNullException(nameof(configInstance));
            
            _listProperty = FindListPropertyInModConfig(_configType);
            
            if (_configInstance != null)
            {
                ConfigViewer.BuildingContent = _configInstance;
            }
        }

        private PropertyInfo FindListPropertyInModConfig(Type configType)
        {
            var modConfigType = typeof(HoiModConfig);
            var properties = modConfigType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                {
                    var itemType = prop.PropertyType.GetGenericArguments()[0];
                    if (itemType == configType)
                    {
                        return prop;
                    }
                }
            }
            
            return null;
        }

        public void Save()
        {
            try
            {
                if (_listProperty == null)
                {
                    CustomMessageBox.Show(
                        $"Не найдено соответствующее свойство в ModConfig для типа {_configType.Name}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var list = _listProperty.GetValue(ModDataStorage.Mod) as System.Collections.IList;
                if (list == null)
                {
                    var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(_configType);
                    list = (System.Collections.IList)Activator.CreateInstance(listType);
                    _listProperty.SetValue(ModDataStorage.Mod, list);
                }

                var index = -1;
                for (int i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(list[i], _configInstance))
                    {
                        index = i;
                        break;
                    }
                }
                
                if (index >= 0)
                {
                    list[index] = _configInstance;
                }
                else
                {
                    list.Add(_configInstance);
                }

                CustomMessageBox.Show(
                    $"{GetConfigDisplayName(_configType)} успешно сохранен",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(
                    $"Ошибка при сохранении: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string GetConfigDisplayName(Type configType)
        {
            var name = configType.Name;
            if (name.EndsWith("Config"))
            {
                name = name.Substring(0, name.Length - 6);
            }
            return name;
        }
    }
}

