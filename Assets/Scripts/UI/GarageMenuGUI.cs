using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class GarageMenuGUI : GameGUI
{
    [Header("Preafabs")]
    [SerializeField] private MenuItemView _shopRouletteItemPrefab;
    [SerializeField] private MenuItemView _tuningMenuItemPrefab;

    [Space, Header("Menu containers")]
    [SerializeField] private Transform _tuningMenuContentParent;
    [SerializeField] private Transform _shopRouletteContentParent;

    [Space, Header("Dynamic elements")]
    [SerializeField] private CanvasGroup _confirmCanvasGroup;
    [SerializeField] private CanvasGroup _tuningCanvasGroup;
    [SerializeField] private TextMeshProUGUI _confirmationText;

    private MainMenu _mainMenu;
    private PlayerData _player;
    private CarCollection _availableCars;

    private MenuSection CurrentMenuSection { get; set; }
    private SelectedItem CurrentItem { get; set; }
    private List<MenuItemView> _activeTuningMenuViews;
    private List<MenuItemView> _activeShopRouletteViews;

    #region Initialization And Control

    public void Initialize(CarCollection availableCars, PlayerData player, MainMenu mainMenu)
    {
        _availableCars = availableCars;
        _player = player;
        _mainMenu = mainMenu;
        _activeShopRouletteViews = new List<MenuItemView>();
        _activeTuningMenuViews = new List<MenuItemView>();
    }

    public void Load()
    {
        SetCurrentlySelectedMenuItem(_player.PurchasedCars.Selected);

        _player.PurchasedCars.TryGetData(_player.PurchasedCars.Selected, out var selected);
        SelectCar(selected);

        LoadCarShopRoulette();
    }

    public void OnBackButtonPressed()
    {
        RevertPreviewed();

        switch (CurrentMenuSection)
        {
            case MenuSection.Cars:
                ExitGUI();
                break;
            case MenuSection.Entities: 
                LoadCarShopRoulette();
                break;
        }
    }

    private void SelectCar(CarData car)
    {
        // if purchased - load customized car data
        if (!_player.PurchasedCars.TryGetData(car.config, out var selectionData))
            selectionData = car;

        _mainMenu.LoadCarPreview(selectionData);
        _availableCars.TrySelectCar(selectionData.config);

        UpdateConfirmationButton();
        UpdateTuningMenuVisibility();
        LoadTuningMenu(selectionData.config);
        SetCurrentlySelectedMenuItem(selectionData.config);
    }

    private void SetCurrentlySelectedMenuItem(IShopItem item)
    {
        CarConfig car = null;
        CarEntity entity = null;

        bool isPurchased = false;
        bool isAlreadyApplied = false;
        MenuSection section = MenuSection.Cars;

        if ((car = item as CarConfig) != null)
        {
            isPurchased = _player.PurchasedCars.Contains(car);
            isAlreadyApplied = car == _player.PurchasedCars.Selected;
        }
        else if ((entity = item as CarEntity) != null)
        {
            if (_player.PurchasedCars.TryGetData(_availableCars.Selected, out var data))
            {
                isPurchased = data.availableEntities.Contains(entity);
                isAlreadyApplied = data.selectedEntities.Contains(entity);
                section = MenuSection.Entities;
            }
        }

        CurrentItem = new SelectedItem
        {
            item = item,
            section = section,
            isPurchased = isPurchased,
            isAlreadyApplied = isAlreadyApplied
        };
    }

    #endregion

    #region Tuning Menu

    private void UpdateTuningMenuVisibility()
    {
        bool enabled = _player.PurchasedCars.Contains(_availableCars.Selected);

        _tuningCanvasGroup.alpha = enabled ? 1f : 0f;
        _tuningCanvasGroup.interactable = enabled;
    }

    private void SetSelectedTuningMenuItemView(MenuItemView view)
    {
        foreach (var v in _activeTuningMenuViews)
            v.SetSelected(false);

        view.SetSelected(true);
    }

    private void LoadTuningMenu(CarConfig targetCar)
    {
        _activeTuningMenuViews.Clear();

        foreach (Transform child in _tuningMenuContentParent)
            Destroy(child.gameObject);

        if (targetCar == null || !_player.PurchasedCars.Contains(targetCar))
            return;

        TunableGroup[] groups = targetCar.Tunables.Groups;
        
        for (int i = 0; i < groups.Length; i++)
        {
            var view = InstantiateMenuItem(_tuningMenuItemPrefab, _tuningMenuContentParent,
                null, groups[i].name, groups[i].type, OnTuningButtonPressed);

            _activeTuningMenuViews.Add(view);
        }
    }

    private void OnTuningButtonPressed(CarEntityType selectedEntityType, MenuItemView view)
    {
        RevertPreviewed();

        IShopItem[] items = _availableCars.Selected.Tunables[selectedEntityType].entities.ToArray();
        IShopItem selected = null;

        if (_player.PurchasedCars.TryGetData(_availableCars.Selected, out var data))
            selected = data.selectedEntities.First(e => e.Type == selectedEntityType);

        LoadShopRoulette(items, selected);
        CurrentMenuSection = MenuSection.Entities;

        SetCurrentlySelectedMenuItem(selected);
        SetSelectedTuningMenuItemView(view);
    }

    #endregion

    #region Shop Roulette

    private void LoadCarShopRoulette()
    {
        CurrentMenuSection = MenuSection.Cars;
        LoadShopRoulette(_availableCars.Cars.Select((c) => c.config).ToArray(), _player.PurchasedCars.Selected);
    }
    
    private void LoadShopRoulette(IShopItem[] items, IShopItem selected)
    {
        _activeShopRouletteViews.Clear();

        foreach (Transform child in _shopRouletteContentParent)
            Destroy(child.gameObject);

        foreach (var item in items)
        {
            Sprite icon = item.Icon;
            string name = item.Name;

            var view = InstantiateMenuItem(_shopRouletteItemPrefab, _shopRouletteContentParent,
                icon, name, item, OnRouletteItemButtonPressed);

            _activeShopRouletteViews.Add(view);

            if (item == selected && selected != null)
            {
                SetSelectedShopRouletteItemView(view);
                SetCurrentlySelectedMenuItem(selected);
            }
        }
    }

    private void OnRouletteItemButtonPressed(IShopItem item, MenuItemView menuItemView)
    {
        SetCurrentlySelectedMenuItem(item);
        SetSelectedShopRouletteItemView(menuItemView);
        UpdateConfirmationButton();

        RevertPreviewed();

        switch (CurrentItem.section)
        {
            case MenuSection.Cars:
                SelectCar(new CarData(item as CarConfig));
                break;
            case MenuSection.Entities:
                PreviewEntity(item as CarEntity);
                break;
        }
    }

    private void SetSelectedShopRouletteItemView(MenuItemView view)
    {
        foreach (var v in _activeShopRouletteViews)
            v.SetSelected(false);

        view.SetSelected(true);
    }

    #endregion

    #region Confirmation

    public void OnConfirmButtonPressed()
    {
        switch (CurrentItem.section)
        {
            case MenuSection.Cars:
                ConfirmCar(CurrentItem.item as CarConfig, CurrentItem.isPurchased);
                break;
            case MenuSection.Entities:
                ConfirmEntity(CurrentItem.item as CarEntity, CurrentItem.isPurchased);
                break;
        }

        SetCurrentlySelectedMenuItem(CurrentItem.item);
        UpdateConfirmationButton();
        UpdateTuningMenuVisibility();
        LoadTuningMenu(_availableCars.Selected);
    }

    private void UpdateConfirmationButton()
    {
        if (CurrentItem.isAlreadyApplied)
        {
            _confirmationText.text = "Selected";
            _confirmCanvasGroup.interactable = false;
        }
        else
        {
            string confirmationText = CurrentItem.isPurchased ? "Select" : $"Buy (${CurrentItem.item.Price})";

            _confirmationText.text = confirmationText;
            _confirmCanvasGroup.interactable = true;
        }
    }

    private void ConfirmCar(CarConfig car, bool isPurchased)
    {
        if (!isPurchased)
        {
            if (!_player.TryPurchaseCar(car))
            {
                // show in-app shop menu
                return;
            }
        }

        _player.SelectCar(car);
        _availableCars.TrySelectCar(car);
    }

    private void ConfirmEntity(CarEntity entity, bool isPurchased)
    {
        if (!isPurchased)
        {
            if (!_player.TryPurchaseCarEntity(_availableCars.Selected, entity))
            {
                // show in-app shop menu
                return;
            }
        }

        _player.SelectCarEntity(_availableCars.Selected, entity);
        ApplyPreviewedEntities();
    }

    #endregion

    #region Helper Methods
    private MenuItemView InstantiateMenuItem<T>(MenuItemView prefab, Transform parent,
        Sprite icon, string name, T onClickArgument, Action<T, MenuItemView> onClick)
    {
        var menuItemView = Instantiate(prefab, parent);

        if (icon != null) 
            menuItemView.SetSprite(icon);

        menuItemView.SetNameText(name);
        menuItemView.Button.onClick.AddListener(() => onClick(onClickArgument, menuItemView));

        return menuItemView;
    }

    private void PreviewEntity(CarEntity entity) => _mainMenu.ActiveCarConfigurator.PreviewEntity(entity);
    private void ApplyPreviewedEntities() => _mainMenu.ActiveCarConfigurator.ApplyPreviewed();
    private void RevertPreviewed() => _mainMenu.ActiveCarConfigurator.RevertPreviewed();

    #endregion

    #region Helper Types

    private struct SelectedItem
    {
        public IShopItem item;
        public MenuSection section;
        public bool isPurchased;
        public bool isAlreadyApplied;
    }

    private enum MenuSection
    {
        Cars,
        Entities
    }

    #endregion
}
