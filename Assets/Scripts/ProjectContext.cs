using System.Collections.Generic;
using UnityEngine;

public class ProjectContext : MonoBehaviour
{
    [SerializeField] private CarShopConfig _carShopConfig;
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private AdManager _adManager;
    [SerializeField] private List<CarConfig> _defaultCars;

    private static ProjectContext _instance;
    public static ProjectContext Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ProjectContext>();

            return _instance;
        }
    }

    public PlayerData Player { get; private set; }
    public CarShopConfig CarShopConfig => _carShopConfig;
    public SceneLoader SceneLoader => _sceneLoader;
    public List<CarConfig> DefaultCars => _defaultCars;
    public NetworkManager NetworkManager => _networkManager;
    public AdManager AdManager => _adManager;

    public void Initialize(PlayerData player)
    {
        Player = player;
        _carShopConfig.InitializeConfigs();
        NetworkManager.InitializeAndConnect(player);

        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}
