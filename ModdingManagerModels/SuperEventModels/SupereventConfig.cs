using static ModdingManagerModels.SuperEventModels.SuperEventGuiElements;

namespace ModdingManagerModels.SuperEventModels
{
    public class SupereventConfig : IConfig
    {
        public string Id { get; set; }
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
