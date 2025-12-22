# Иерархия зависимостей проектов ModdingManager

## Обзор структуры решения

Решение состоит из 5 проектов, организованных в слоистую архитектуру:

```
┌─────────────────────────────────────────────────────────────┐
│                    View (WPF Application)                    │
│                  net8.0-windows (WinExe)                    │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│              Application (Business Logic)                   │
│                  net8.0-windows (Exe)                       │
└───────┬───────────────────────┬─────────────────────────────┘
        │                       │
        ▼                       ▼
┌──────────────────┐   ┌─────────────────────────────────────┐
│  RawDataWorker   │   │           Models                     │
│   (Parsers)      │   │      (Domain Models)                 │
│   net8.0 (Exe)   │   │      net8.0 (Library)               │
└────────┬─────────┘   └──────────────┬──────────────────────┘
         │                            │
         │                            ▼
         │                    ┌──────────────┐
         │                    │     Data      │
         │                    │  (Resources)  │
         │                    │  net8.0 (Lib) │
         │                    └───────────────┘
         │                            ▲
         └────────────────────────────┘
```

## Детальная иерархия зависимостей

### Уровень 1: Базовые проекты (без зависимостей от других проектов)

#### **Data** (`Data/Data.csproj`)
- **Тип**: Class Library
- **Target Framework**: net8.0
- **Зависимости от проектов**: Нет
- **Внешние пакеты**:
  - System.Drawing.Common (9.0.10)
  - System.Resources.Extensions (9.0.10)
- **Назначение**: Хранение встроенных ресурсов и данных по умолчанию
- **Основные компоненты**:
  - `DataLib.cs` - библиотека данных
  - Встроенные текстовые ресурсы (RulesCoreDefenitions.txt, BaisicUnitGroupsDefenitions.txt)

---

### Уровень 2: Модели данных

#### **Models** (`Models/Models.csproj`)
- **Тип**: Class Library
- **Target Framework**: net8.0
- **Зависимости от проектов**:
  - `Data` (ProjectReference)
- **Внешние пакеты**:
  - SixLabors.Fonts (2.1.3)
  - SixLabors.ImageSharp (3.1.11)
  - System.Drawing.Common (10.0.0-preview.7.25380.108)
- **Назначение**: Доменные модели и конфигурации для модов HOI4
- **Основные компоненты**:
  - Конфигурации: `CountryConfig`, `StateConfig`, `IdeaConfig`, `TechTreeConfig`, `IdeologyConfig`, `CharacterConfig`, и др.
  - Enums: типы для игры (языки, типы персонажей, идеологии)
  - Types: вспомогательные типы (`HoiReference`, `Identifier`, локализация)
  - Interfaces: `IConfig`, `IGfx`, `IHoiData`, `IModifier`

---

### Уровень 3: Парсинг данных

#### **RawDataWorker** (`RawDataWorker/RawDataWorker.csproj`)
- **Тип**: Executable
- **Target Framework**: net8.0
- **Зависимости от проектов**:
  - `Models` (ProjectReference)
- **Внешние пакеты**:
  - HtmlAgilityPack (1.12.4)
- **Назначение**: Парсинг файлов HOI4 в объектные модели
- **Основные компоненты**:
  - `HoiVarsConverter.cs` - конвертация переменных
  - `Parsers/` - парсеры игровых файлов
  - `Regexes.cs`, `Regexes_generated.cs` - регулярные выражения для парсинга

---

### Уровень 4: Бизнес-логика

#### **Application** (`Application/Application.csproj`)
- **Тип**: Executable
- **Target Framework**: net8.0-windows
- **Зависимости от проектов**:
  - `RawDataWorker` (ProjectReference)
  - `Data` (ProjectReference)
  - `Models` (ProjectReference)
- **Внешние пакеты**:
  - BCnEncoder.Net (2.2.1)
  - ColorFontPickerWPF (2.0.1)
  - HarfBuzzSharp (8.3.1.2)
  - Microsoft.Extensions.Logging (9.0.10)
  - NAudio (2.2.1)
  - Newtonsoft.Json (13.0.4)
  - OpenCvSharp4.Extensions (4.11.0.20250507)
  - OpenCvSharp4.Windows (4.11.0.20250507)
  - Pfim (0.11.3)
  - RoyT.TrueType (0.2.0)
  - SixLabors.Fonts (2.1.3)
  - SixLabors.ImageSharp (3.1.11)
  - SixLabors.ImageSharp.Drawing (2.1.7)
  - Syntellect.Typography.OpenFont.Net6 (1.0.0)
  - TeximpNet (1.4.3)
  - TgaLib (1.0.2)
  - TTCFileSplitter (1.0.1)
- **Назначение**: Основная бизнес-логика приложения
- **Основные компоненты**:
  - `Composers/` - парсинг игровых файлов в модели (CountryComposer, CharacterComposer, IdeaComposer, и др.)
  - `Handlers/` - обработка действий пользователя (CountryHandler, CharacterHandler, StateWorkerHandler, и др.)
  - `Managers/` - управление конфигами и графикой (DDS, изображения)
  - `utils/Pathes/` - пути к директориям игры и мода
  - `ModDataStorage.cs` - центральное хранилище данных мода
  - `Loaders/` - загрузка графики
  - `Settings/ModManagerSettings.cs` - настройки приложения
  - `extentions/` - расширения для различных типов

---

### Уровень 5: Пользовательский интерфейс

#### **View** (`View/View.csproj`)
- **Тип**: WPF Application (WinExe)
- **Target Framework**: net8.0-windows
- **Зависимости от проектов**:
  - `Application` (ProjectReference)
  - `RawDataWorker` (ProjectReference)
  - `Models` (ProjectReference)
- **Внешние пакеты**: (те же, что и Application)
- **Назначение**: WPF интерфейс пользователя
- **Основные компоненты**:
  - `View/` - окна приложения (MainWindow, TechTreeCreator, CountryCreator, и др.)
  - `Presenters/` - презентеры (MVP паттерн)
  - `controls/` - кастомные WPF контролы
  - `Intefaces/` - интерфейсы представлений
  - `Properties/Resources.resx` - ресурсы приложения

---

## Граф зависимостей

```
Data (базовый)
  │
  ├─→ Models
  │     │
  │     ├─→ RawDataWorker
  │     │     │
  │     │     └─→ Application
  │     │           │
  │     │           └─→ View
  │     │
  │     └─→ Application
  │           │
  │           └─→ View
  │
  └─→ Application
        │
        └─→ View
```

## Таблица зависимостей

| Проект | Зависит от | Уровень |
|--------|------------|---------|
| **Data** | - | 1 (Базовый) |
| **Models** | Data | 2 |
| **RawDataWorker** | Models | 3 |
| **Application** | RawDataWorker, Data, Models | 4 |
| **View** | Application, RawDataWorker, Models | 5 (UI) |

## Описание слоев

### Слой данных (Data)
Самый нижний слой, содержащий встроенные ресурсы и данные по умолчанию. Не зависит от других проектов решения.

### Слой моделей (Models)
Содержит все доменные модели и конфигурации. Зависит только от Data для доступа к ресурсам.

### Слой парсинга (RawDataWorker)
Отвечает за парсинг файлов формата HOI4 в объектные модели. Использует Models для типов данных.

### Слой бизнес-логики (Application)
Содержит основную логику приложения:
- Композиция данных мода из файлов игры
- Обработка действий пользователя
- Управление конфигурациями
- Работа с графикой и ресурсами

### Слой представления (View)
WPF приложение, предоставляющее пользовательский интерфейс. Использует Application для бизнес-логики и Models для работы с данными.

## Примечания

- Все проекты используют .NET 8.0
- View и Application используют `net8.0-windows` для поддержки WPF
- Остальные проекты используют `net8.0` (кроссплатформенные)
- Application и View имеют одинаковый набор внешних пакетов для работы с графикой
- Проект View является точкой входа приложения (WinExe)

