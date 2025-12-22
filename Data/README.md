# Структура проекта Data

Проект содержит все ресурсы, встроенные данные и константы по умолчанию для приложения.

## Структура директорий

```
Data/
├── Resources/              # Все ресурсы приложения
│   ├── Images/            # Изображения
│   │   ├── Interface/     # Изображения интерфейса (для ResX)
│   │   └── Controls/      # Изображения элементов управления (для ResX)
│   ├── Icons/             # Иконки приложения
│   └── Audio/             # Аудио файлы
│
├── Embedded/              # Встроенные ресурсы (встраиваются в сборку)
│   └── Text/              # Текстовые файлы для парсинга
│       ├── RulesCoreDefenitions.txt
│       └── BaisicUnitGroupsDefenitions.txt
│
├── Defaults/              # Классы с константами и значениями по умолчанию
│   ├── DataDefaultValues.cs      # Значения по умолчанию (Null, NaN, изображения)
│   └── ClassStaticValues.cs      # Статические значения для классов
│
└── Properties/            # Ресурсы ResX
    ├── Resources.resx            # Все ресурсы (изображения, иконки)
    └── Resources.Designer.cs     # Автогенерируемый код доступа к ресурсам
```

## Использование

### Ресурсы ResX

Доступ через `Data.Properties.Resources.*`:
```csharp
using Data.Properties;

var image = Resources.null_item_image;
var icon = Resources.healer;
```

### Встроенные ресурсы

Доступ через `DataLib.*`:
```csharp
using Data;

var rules = DataLib.RulesCoreDefenitions;
var unitGroups = DataLib.BaisicUnitGroupsDefenitions;
```

### Значения по умолчанию

```csharp
using Data;

var nullImage = DataDefaultValues.NullImageSource;
var nullString = DataDefaultValues.Null;
```

### Статические значения

```csharp
using Data;

var postfix = ClassStaticValues.CountryLeaderTraitPostfix;
```

## Типы ресурсов

1. **ResX Resources** (`Resources/`) - ресурсы, доступные через Properties.Resources
2. **Embedded Resources** (`Embedded/`) - текстовые файлы, встроенные в DLL
3. **Runtime Data** (`Resources/Audio/`) - файлы данных, доступные во время выполнения

