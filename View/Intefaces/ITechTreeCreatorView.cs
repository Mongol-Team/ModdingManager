using ViewControls;
using Models.Enums;
using Models.Interfaces;
using Models.Types.LocalizationData;
using Models.Types.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Models.Configs;

namespace ViewInterfaces
{
    public interface ITechTreeCreatorView
    {
        // Основные события
        event EventHandler AddItemRequested;
        event EventHandler UpdateItemRequested;
        event EventHandler AddChildConnectionRequested;
        event EventHandler AddMutualConnectionRequested;
        event EventHandler SaveTreeRequested;
        event EventHandler LoadTreeRequested;
        event EventHandler GenerateTreeRequested;
        event EventHandler DoneRequested;
        event EventHandler DebugRequested;

        // События для работы с элементами
        event EventHandler<TechTreeItemConfig> GridElementEditRequested;
        event EventHandler<TechTreeItemConfig> SetItemToControlsRequested;
        event EventHandler<TechTreeItemConfig> UpdateItemToControlsRequested;
        event EventHandler ClearControlsRequested;

        // Свойства для доступа к данным UI
        TechTreeConfig TechTreeConfig { get; set; }
        TechnologyGrid TechnologyGrid { get; set; }

        // Свойства для элементов управления
        Identifier TechId { get; set; }
        ConfigLocalisation Localisation { get; set; }
        int TechModifCost { get; set; }
        int TechCost { get; set; }
        int StartYear { get; set; }
        string TreeName { get; set; }
        TechTreeOrientationType TreeOrientation { get; set; }
        TechTreeLedgerType TreeLedger { get; set; }

        Dictionary<BuildingConfig, object> EnableBuildings { get; set; }
        List<EquipmentConfig> EnableEquipments { get; set; }
        List<SubUnitConfig> EnableUnits { get; set; }


        string Allowed { get; set; }
        string AllowBranch { get; set; }
        Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        string Effects { get; set; }
        string AiWillDo { get; set; }

        Dictionary<TechTreeItemConfig, int> Dependencies { get; set; }

        List<TechCategoryConfig> Categories { get; set; }

        Image SmallTechImage { get; set; }
        Image BigTechImage { get; set; }
        bool IsBigImage { get; }

        // Методы для работы с UI
        void ShowMessage(string message, string title);
        void ShowError(string message, string title);
        void UpdateUIFromConfig(TechTreeConfig config);
        void ClearForm();
        void RefreshTechTreeView();
    }

}