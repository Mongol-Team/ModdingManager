using ModdingManager.Controls;
using ModdingManager.Intefaces;
using ModdingManagerClassLib.Extentions;
using ModdingManagerClassLib.handlers;
using ModdingManagerModels;
using System;
using System.Linq;
using System.Windows;

namespace ModdingManager.Presenters
{
    public class TechTreeWorkerPresenter
    {
        private readonly ITechTreeCreatorView _view;
        private readonly TechTreeHandler _techTreeHandler = new TechTreeHandler();
        private readonly TechnologyGrid _techGrid = new TechnologyGrid();
        public TechTreeWorkerPresenter(ITechTreeCreatorView view)
        {
            _view = view;
            _techGrid = view.TechnologyGrid;
            _techTreeHandler = new TechTreeHandler();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _view.AddItemRequested += OnAddItemRequested;
            _view.UpdateItemRequested += OnUpdateItemRequested;
            _view.AddChildConnectionRequested += OnAddChildConnectionRequested;
            _view.AddMutualConnectionRequested += OnAddMutualConnectionRequested;
            _view.SaveTreeRequested += OnSaveTreeRequested;
            _view.LoadTreeRequested += OnLoadTreeRequested;
            _view.DoneRequested += OnDoneRequested;
            _view.DebugRequested += OnDebugRequested;
            _view.GridElementEditRequested += OnGridElementEditRequested;
            _view.ClearControlsRequested += OnClearControlsRequested;
            _techGrid.ItemEdited += OnGridElementEditRequested;
        }

        private void OnAddItemRequested(object sender, EventArgs e)
        {
            try
            {
                TechTreeItemConfig item = CreateItemFromControls();
                _techGrid.AddItem(item, 0,0);
                _view.ShowMessage("Элемент успешно добавлен!", "Успех");
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при добавлении элемента: {ex.Message}", "Ошибка");
            }
        }

        private void OnUpdateItemRequested(object sender, EventArgs e)
        {
            try
            {
                var markedIds = _techGrid.GetMarkedIds();
                if (markedIds.Count != 1)
                {
                    _view.ShowError("Выделите ровно один элемент для обновления.", "Ошибка");
                    return;
                }

                var id = markedIds.First();
                var item = _view.TechTreeConfig.Items.FirstOrDefault(x => x.Id.AsString() == id);
                if (item == null)
                {
                    _view.ShowError("Не удалось найти элемент в конфигурации.", "Ошибка");
                    return;
                }

                UpdateItemFromControls(item);
                _techGrid.EditItem(item);
                _view.ShowMessage("Элемент успешно обновлён!", "Успех");
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при обновлении элемента: {ex.Message}", "Ошибка");
            }
        }

        private void OnAddChildConnectionRequested(object sender, EventArgs e)
        {
            try
            {
                _techGrid.AddChildConection();
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при добавлении связи: {ex.Message}", "Ошибка");
            }
        }

        private void OnAddMutualConnectionRequested(object sender, EventArgs e)
        {
            try
            {
                _techGrid.AddMutualConection();
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при добавлении взаимной связи: {ex.Message}", "Ошибка");
            }
        }

        private void OnSaveTreeRequested(object sender, EventArgs e)
        {
            try
            {
                // Реализация сохранения древа
                _view.ShowMessage("Древо успешно сохранено!", "Успех");
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при сохранении древа: {ex.Message}", "Ошибка");
            }
        }

        private void OnLoadTreeRequested(object sender, EventArgs e)
        {
            try
            {
                // Реализация загрузки древа
                _view.ShowMessage("Древо успешно загружено!", "Успех");
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при загрузке древа: {ex.Message}", "Ошибка");
            }
        }

        private void OnDoneRequested(object sender, EventArgs e)
        {
            try
            {
                var config = _view.TechTreeConfig;
                config.Id = new(_view.TreeName);
                config.Orientation = _view.TreeOrientation;
                config.Ledger = _view.TreeLedger;

                _techGrid.RefreshView();
                _view.ShowMessage("Древо успешно сгенерировано!", "Успех");
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при генерации древа: {ex.Message}", "Ошибка");
            }
        }

        private void OnDebugRequested(object sender, EventArgs e)
        {
            // Реализация отладки
        }

        private void OnGridElementEditRequested(object sender, TechTreeItemConfig item)
        {
            try
            {
                FillControlsWithItem(item);
            }
            catch (Exception ex)
            {
                _view.ShowError($"Ошибка при заполнении контролов: {ex.Message}", "Ошибка");
            }
        }

        private void OnClearControlsRequested(object sender, EventArgs e)
        {
            _view.ClearForm();
        }
        #region Helper Methods
        private void UpdateItemFromControls(TechTreeItemConfig item)
        {
            item.Id = new(_view.TechId);
            item.Name = _view.TechName;
            item.Description = _view.TechDescription;
            item.ModifCost = _view.TechModifCost;
            item.Cost = _view.TechCost;
            item.StartYear = _view.StartYear;
            item.Enables = _view.Enables;
            item.Allowed = _view.Allowed;
            item.Effects = _view.Effects;
            item.Modifiers = _view.Modifiers;
            item.AiWillDo = _view.AiWillDo;
            item.Dependencies = _view.Dependencies;
            item.Categories = _view.Categories;
            item.Image = _view.IsBigImage ? _view.BigTechImage.ToBitmap() : _view.SmallTechImage.ToBitmap();
        }
        private TechTreeItemConfig CreateItemFromControls()
        {
            TechTreeItemConfig item = new TechTreeItemConfig
            {
                Id = new(_view.TechId),
                Name = _view.TechName,
                Description = _view.TechDescription,
                ModifCost = _view.TechModifCost,
                Cost = _view.TechCost,
                StartYear = _view.StartYear,
                Enables = _view.Enables,
                Allowed = _view.Allowed,
                Effects = _view.Effects,
                Modifiers = _view.Modifiers,
                AiWillDo = _view.AiWillDo,
                Dependencies = _view.Dependencies,
                Categories = _view.Categories,
                Image = _view.IsBigImage ? _view.BigTechImage.ToBitmap() : _view.SmallTechImage.ToBitmap()
            };
            return item;
        }
        private void FillControlsWithItem(TechTreeItemConfig item)
        {
            _view.TechId = item.Id.AsString();
            _view.TechName = item.Name;
            _view.TechDescription = item.Description;
            _view.TechModifCost = item.ModifCost;
            _view.TechCost = item.Cost;
            _view.StartYear = item.StartYear;
            _view.Enables = item.Enables;
            _view.Allowed = item.Allowed;
            _view.Effects = item.Effects;
            _view.Modifiers = item.Modifiers;
            _view.AiWillDo = item.AiWillDo;
            _view.Dependencies = item.Dependencies;
            _view.Categories = item.Categories;
            if (item.IsBig && item.Image != null)
            {
                _view.BigTechImage = item.Image;
            }
            else if (item.Image != null)
            {
                _view.SmallTechImage = item.Image;
            }
        }
        #endregion
    }
}