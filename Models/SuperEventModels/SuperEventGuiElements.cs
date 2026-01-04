using Models.Types;

namespace Models.SuperEventModels
{
    public class SuperEventGuiElements
    {

        public sealed class GuiDocument
        {
            public List<ContainerWindow> Containers { get; set; } = new();
        }

        /// <summary>
        /// containerWindowType = { ... }
        /// </summary>
        public sealed class ContainerWindow
        {
            public string Name { get; set; } = string.Empty;
            public Size2D Size { get; set; } = new();
            public Point2D Position { get; set; } = new();

            // В оригинале строки (например, "center"). Оставляем string для гибкости.
            public string? Orientation { get; set; }   // center/left/right/...
            public string? Origo { get; set; }         // center/top_left/...
            public bool? Clipping { get; set; }      // clipping = no/yes  -> bool?
            public string? ShowSound { get; set; }     // show_sound = ...

            // Дочерние элементы
            public List<IconControl> Icons { get; set; } = new();
            public List<TextBoxControl> Texts { get; set; } = new();
            public List<ButtonControl> Buttons { get; set; } = new();
        }

        /// <summary>
        /// iconType = { ... }
        /// </summary>
        public sealed class IconControl
        {
            public string Name { get; set; } = string.Empty;
            public string SpriteType { get; set; } = string.Empty;
            public Point2D Position { get; set; } = new();
            public string? Orientation { get; set; }   // center/...
            public bool? AlwaysTransparent { get; set; } // alwaystransparent = yes/no
        }

        /// <summary>
        /// instantTextBoxType = { ... }
        /// </summary>
        public sealed class TextBoxControl
        {
            public string Name { get; set; } = string.Empty;
            public Point2D Position { get; set; } = new();

            public FontSignature Font { get; set; }
            public Point2D? BorderSize { get; set; } // { x=0 y=0 } — как в примере
            public string Text { get; set; } = string.Empty;
            public int? MaxWidth { get; set; }
            public int? MaxHeight { get; set; }
            public bool? FixedSize { get; set; }      // fixedsize = yes/no

            public string? Orientation { get; set; }
            public string? Format { get; set; }       // format = centre/left/right
        }

        /// <summary>
        /// buttonType = { ... }
        /// </summary>
        public sealed class ButtonControl
        {
            public string Name { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public string? Shortcut { get; set; }         // "ESCAPE"
            public Point2D Position { get; set; } = new();
            public string? QuadTextureSprite { get; set; } // quadTextureSprite = "..."
            public FontSignature Font { get; set; }
            public string? Orientation { get; set; }
        }

        /// <summary>
        /// Вектор (x,y) для позиций и borderSize.
        /// </summary>
        public struct Point2D
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point2D(int x, int y) { X = x; Y = y; }
            public override readonly string ToString() => $"{{x={X} y={Y}}}";
        }

        /// <summary>
        /// Размер { width, height }.
        /// </summary>
        public struct Size2D
        {
            public int Width { get; set; }
            public int Height { get; set; }

            public Size2D(int width, int height) { Width = width; Height = height; }
            public override readonly string ToString() => $"{{width={Width} height={Height}}}";
        }
    }

}

