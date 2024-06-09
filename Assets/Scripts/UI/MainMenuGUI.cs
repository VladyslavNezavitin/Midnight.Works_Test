using UnityEngine;
using UnityEngine.UI;

public class MainMenuGUI : GameGUI
{
    [SerializeField] private GameObject _prevButtonGO;
    [SerializeField] private GameObject _nextButtonGO;
    [SerializeField] private Button _playButton;

    private MainMenu _mainMenu;
    private PlayerData _player;

    public void Initialize(PlayerData player, MainMenu mainMenu)
    {
        _mainMenu = mainMenu;
        _player = player;
    }

    public void Enter()
    {
        UpdateCarSelectionButtonsVisibility(_player.PurchasedCars.IndexOfSelected);
    }



    public void OnPrevCarButtonPressed()
    {
        int index = _player.PurchasedCars.IndexOfSelected;

        if (index - 1 >= 0)
            SelectAndUpdateButtons(index - 1);
    }

    public void OnNextCarButtonPressed()
    {
        int index = _player.PurchasedCars.IndexOfSelected;

        if (index + 1 < _player.PurchasedCars.Cars.Count)
            SelectAndUpdateButtons(index + 1);
    }

    private void SelectAndUpdateButtons(int index)
    {
        CarConfig config = _player.PurchasedCars.Cars[index].config;
        _player.SelectCar(config);
        _mainMenu.LoadCarPreview(_player.PurchasedCars.DataOfSelected);

        UpdateCarSelectionButtonsVisibility(index);
    }

    private void UpdateCarSelectionButtonsVisibility(int currentIndex)
    {
        bool prevActive = currentIndex > 0;
        bool nextActive = currentIndex < _player.PurchasedCars.Cars.Count - 1;

        _prevButtonGO.SetActive(prevActive);
        _nextButtonGO.SetActive(nextActive);
    }

    public void OnPlayButtonPressed() => _mainMenu.OnPlayButtonPressed();
    public void OnGarageButtonPressed() => _mainMenu.OnGarageButtonPressed();
    public void OnOptionsButtonPressed() => _mainMenu.OnOptionsButtonPressed();
    public void OnQuitButtonPressed() => _mainMenu.OnQuitButtonPressed();
    
}
