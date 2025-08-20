using ModdingManager.classes.handlers;
using ModdingManager.classes.managers.@base;
using ModdingManager.classes.views;
using ModdingManager.configs;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class CountryPresenter
{
    private readonly ICountryView _view;
    private readonly CountryHandler _handler;
    private CountryConfig _currentConfig = new CountryConfig();

    public CountryPresenter(ICountryView view)
    {
        _view = view;
        _handler = new CountryHandler();

        _view.ApplyClicked += OnApplyClicked;
        _view.LoadConfigClicked += ConfigManager.OnLoadConfigClickedEvent;
        _view.SaveConfigClicked += ConfigManager.OnSaveConfigClickedEvent;
    }

    private void OnApplyClicked(object sender, RoutedEventArgs e)
    {
        _currentConfig = new CountryConfig
        {
            Tag = _view.Tag,
            Name = _view.Name,
            Capital = _view.Capital,
            GraphicalCulture = _view.GraphicalCulture,
            Color = _view.Color,
            Technologies = _view.Technologies,
            Convoys = _view.Convoys,
            OOB = _view.OOB,
            CountryFileName = _view.CountryFileName,
            Stab = _view.Stab,
            WarSup = _view.WarSup,
            ResearchSlots = _view.ResearchSlots,
            RulingParty = _view.RulingParty,
            LastElection = _view.LastElection,
            ElectionFrequency = _view.ElectionFrequency,
            ElectionsAllowed = _view.ElectionsAllowed,
            Ideas = _view.Ideas,
            Characters = _view.Characters,
            StateCores = _view.States,
            PartyPopularities = _view.PartyPopularities,
            CountryFlags = _view.CountryFlags,
        };
        _handler.CurrentConfig = _currentConfig;
        try
        {
            _handler.CreateCountryHistoryFile();
            _handler.UpdateStateOwnership();
            _handler.CreateCommonCountriesFile();
            _handler.CreateCountryFlags();
            _handler.AddCountryTag();
            _handler.CreateLocalizationFiles();

            _view.ShowMessage("Страна успешно создана!");
        }
        catch (Exception ex)
        {
            _view.ShowError($"Ошибка: {ex.Message}");
        }
    }
}