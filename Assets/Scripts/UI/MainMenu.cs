using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("PlayerData Data Display")]
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _moneyCountText;
    [SerializeField] private TextMeshProUGUI _goldCountText;

    [Space, Header("Car Data Display")]
    [SerializeField] private Transform _carPreviewPoint;
    [SerializeField] private TextMeshProUGUI _carNameText;

    [Space, Header("Modules")]
    [SerializeField] private GameGUI _headerGUI;
    [SerializeField] private MainMenuGUI _mainMenuGUI;
    [SerializeField] private GarageMenuGUI _garageMenuGUI;
    [SerializeField] private OptionsMenuGUI _optionsMenuGUI;
    [SerializeField] private GameSelectionGUI _gameSelectionGUI;

    private PlayerData _player;
    public CarConfigurator ActiveCarConfigurator { get; private set; }

    #region MonoBehaviour

    private void Start() => 
        Initialize(ProjectContext.Instance.Player, ProjectContext.Instance.CarShopConfig);

    private void OnEnable()
    {
        _garageMenuGUI.ExitRequested += GarageGUI_OnExitRequested;
        _optionsMenuGUI.ExitRequested += OptionsGUI_OnExitRequested;
        _gameSelectionGUI.ExitRequested += GameGUI_OnExitRequested;
    }

    private void OnDisable()
    {
        _garageMenuGUI.ExitRequested -= GarageGUI_OnExitRequested;
        _optionsMenuGUI.ExitRequested -= OptionsGUI_OnExitRequested;
        _gameSelectionGUI.ExitRequested -= GameGUI_OnExitRequested;
    }

    private void OnDestroy()
    {
        _player.UsernameChanged -= Player_OnUsernameChanged;
        _player.MoneyAmountChanged -= Player_OnMoneyAmountChanged;
        _player.GoldAmountChanged -= Player_OnGoldAmountChanged;
    }

    #endregion

    #region Initialization

    private void Initialize(PlayerData player, CarShopConfig shopConfig)
    {
        _player = player;

        _player.UsernameChanged += Player_OnUsernameChanged;
        _player.MoneyAmountChanged += Player_OnMoneyAmountChanged;
        _player.GoldAmountChanged += Player_OnGoldAmountChanged;

        InitializeModules(player, shopConfig);

        ShowOnlyOneGUI(_mainMenuGUI);
        _mainMenuGUI.Enter();

        player.PurchasedCars.TryGetData(player.PurchasedCars.Selected, out var data);
        LoadCarPreview(data);
    }

    private void InitializeModules(PlayerData player, CarShopConfig shopConfig)
    {
        // init header
        Player_OnUsernameChanged(_player.Username);
        Player_OnMoneyAmountChanged(_player.MoneyAmount);
        Player_OnGoldAmountChanged(_player.GoldAmount);

        _garageMenuGUI.Initialize(new CarCollection(shopConfig.AvailableCars), player, this);
        _mainMenuGUI.Initialize(player, this);
        _optionsMenuGUI.Initialize(player);
    }

    #endregion

    #region Button Callbacks


    public void OnGarageButtonPressed()
    {
        _garageMenuGUI.Load();
        ShowOnlyOneGUI(_garageMenuGUI);
    }

    public void OnPlayButtonPressed()
    {
        ShowOnlyOneGUI(_gameSelectionGUI, true);
        ProjectContext.Instance.NetworkManager.UpdatePlayerData(ProjectContext.Instance.Player);
    }

    public void OnOptionsButtonPressed() => ShowOnlyOneGUI(_optionsMenuGUI);
    public void OnQuitButtonPressed() => Application.Quit();


    #endregion

    #region Event Callbacks

    private void GarageGUI_OnExitRequested()
    {
        ShowOnlyOneGUI(_mainMenuGUI);
        _mainMenuGUI.Enter();

        _player.PurchasedCars.TryGetData(_player.PurchasedCars.Selected, out var data);
        LoadCarPreview(data);

        SaveSystem.Save();
    }

    private void OptionsGUI_OnExitRequested() => ShowOnlyOneGUI(_mainMenuGUI);
    private void GameGUI_OnExitRequested() => ShowOnlyOneGUI(_mainMenuGUI);

    private void Player_OnUsernameChanged(string username) => _usernameText.text = _player.Username;
    private void Player_OnMoneyAmountChanged(float amount) => _moneyCountText.text = _player.MoneyAmount.ToString();
    private void Player_OnGoldAmountChanged(float amount) => _goldCountText.text = _player.GoldAmount.ToString();

    #endregion


    private void ShowOnlyOneGUI(GameGUI gui, bool hideHeader = false)
    {
        if (hideHeader) _headerGUI.Hide();
        else _headerGUI.Show();

        _mainMenuGUI.Hide();
        _garageMenuGUI.Hide();
        _optionsMenuGUI.Hide();
        _gameSelectionGUI.Hide();

        gui.Show();
    }

    public void LoadCarPreview(CarData carData)
    {
        foreach (Transform child in _carPreviewPoint)
            Destroy(child.gameObject);

        var configurator = Instantiate(carData.config.ConfiguratorPrefab, _carPreviewPoint);

        configurator.transform.localPosition = Vector3.zero;
        configurator.transform.localRotation = Quaternion.identity;
        configurator.gameObject.name = carData.config.Name;
        configurator.Initialize(carData);

        ActiveCarConfigurator = configurator;

        _carNameText.text = carData.config.Name;
    }
}