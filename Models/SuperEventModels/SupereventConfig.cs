using Models.Attributes;
using Models.Configs;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using static Models.SuperEventModels.SuperEventGuiElements;

namespace Models.SuperEventModels
{
    [ConfigCreator(ConfigCreatorType.GenericGuiCreator)]
    public class SupereventConfig : IConfig
    {
        public IGfx Gfx { get; set; }
        public Identifier Id { get; set; }
        public ConfigLocalisation Localisation { get; set; }
        public string SoundPath { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }

        public GuiDocument Gui { get; set; }

        // необязательная мапа: имя спрайта → абсолютный путь к исходному изображению
        public Dictionary<string, string> SpriteSources { get; set; } = new();

        // если хочешь прокинуть тексты вариантов:
        public Dictionary<char, string> OptionTexts { get; set; } = new();
    }

}
