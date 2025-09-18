using ModdingManager.Controls;
using ModdingManagerModels;
using ModdingManagerModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

namespace ModdingManager.Intefaces
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
        string TechId { get; set; }
        string TechName { get; set; }
        string TechDescription { get; set; }
        int TechModifCost { get; set; }
        int TechCost { get; set; }
        int StartYear { get; set; }
        string TreeName { get; set; }
        string TreeOrientation { get; set; }
        string TreeLedger { get; set; }
        List<string> Enables { get; set; }
        List<string> Allowed { get; set; }
        List<string> Effects { get; set; }
        Dictionary<ModifierDefinitionConfig, object> Modifiers { get; set; }
        string AiWillDo { get; set; }
        List<string> Dependencies { get; set; }
        string Categories { get; set; }
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