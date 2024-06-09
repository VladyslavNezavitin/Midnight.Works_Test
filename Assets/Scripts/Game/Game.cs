using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Game : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    public event Action<PlayerInfo[]> PlayerInfoListUpdated;
    public event Action<GameState> GameStateChanged;

    [Header("Modules")]
    [SerializeField] private SpawnpointCollection _spawnpoints;
    [SerializeField] private GameUIManager _uiManager;

    [Space, Header("Gameplay")]
    [SerializeField] private int _playersRequiredToStart;
    [SerializeField] private int _countdownToStartTime;
    [SerializeField] private int _gameplayTime;

    private Dictionary<Player, PlayerInfo> _infoMap;
    private float _currentTimer;

    private GameState _currentState;
    public GameState CurrentState
    {
        get => _currentState;
        private set
        {
            _currentState = value;
            GameStateChanged?.Invoke(value);

            switch (value)
            {
                case GameState.CountdownToStart: _currentTimer = _countdownToStartTime; break;
                case GameState.Playing: BeginGameplay(); break;
                case GameState.GameOver: GameOver(); break;
            }
        }
    }

    public float CurrentTimer => _currentTimer;
    public PlayerInfo[] Players => _infoMap.Values.ToArray();
    public int PlayersRequiredToStart => _playersRequiredToStart;

    #region MonoBehaviour

    private void Awake()
    {
        _infoMap = new Dictionary<Player, PlayerInfo>();
    }

    private async void Start()
    {
        await ProjectContext.Instance.NetworkManager.UntilConnectedToRoom();

        if (PhotonNetwork.IsMasterClient && 
            PhotonNetwork.CurrentRoom.PlayerCount >= _playersRequiredToStart &&
            CurrentState == GameState.WaitingForPlayers)
        {
            CurrentState = GameState.CountdownToStart;
        }

        Player[] playersInRoom = PhotonNetwork.PlayerListOthers;

        if (playersInRoom.Length > 0)
        {
            await _spawnpoints.UntilSync();

            PlayerStats[] playersStats = FindObjectsOfType<PlayerStats>();

            for (int i = 0; i < playersInRoom.Length; i++)
            {
                _infoMap.Add(playersInRoom[i], new PlayerInfo()
                {
                    controller = playersStats[i].GetComponent<CarController>(),
                    stats = playersStats[i],
                    nickName = playersInRoom[i].NickName
                });
            }

            InitializeLocalPlayer();
        }
        else
        {
            InitializeLocalPlayer();
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (_currentState == GameState.CountdownToStart)
        {
            if (_currentTimer < 0)
                CurrentState = GameState.Playing;

            if (PhotonNetwork.CurrentRoom.PlayerCount < _playersRequiredToStart)
                CurrentState = GameState.WaitingForPlayers;

            _currentTimer -= Time.deltaTime;
        }

        if (_currentState == GameState.Playing)
        {
            if (_currentTimer < 0)
                CurrentState = GameState.GameOver;

            _currentTimer -= Time.deltaTime;
        }
    }

    #endregion

    #region PUN Callbacks

    public void OnEvent(EventData photonEvent)
    {
        if ((PUNEvent)photonEvent.Code == PUNEvent.NewPlayerInitialized)
        {
            int viewID = (int)photonEvent.CustomData;
            PhotonView view = PhotonView.Find(viewID);
            Player player = view.Owner;

            _infoMap.Add(player, new PlayerInfo()
            {
                nickName = player.NickName,
                controller = view.GetComponent<CarController>(),
                stats = view.GetComponent<PlayerStats>()
            });

            PlayerInfoListUpdated?.Invoke(_infoMap.Values.ToArray());
        }
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= _playersRequiredToStart &&
                CurrentState == GameState.WaitingForPlayers)
            {
                CurrentState = GameState.CountdownToStart;
            }
		}
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _infoMap.Remove(otherPlayer);
        _spawnpoints.ReleaseSpawnpoint(otherPlayer);

        PlayerInfoListUpdated?.Invoke(_infoMap.Values.ToArray());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(_currentTimer);
            stream.SendNext(CurrentState);
        }
        else
        {
            _currentTimer = (float)stream.ReceiveNext();
            var syncState = (GameState)stream.ReceiveNext();

            if (_currentState != syncState)
                CurrentState = syncState;
        }
    }

    #endregion

    #region Initialization

    private void InitializeLocalPlayer()
    {
        PlayerData playerData = GetPlayerData(PhotonNetwork.LocalPlayer);

        if (playerData == null)
        {
            Leave();
            return;
        }

        InitializePlayer(PhotonNetwork.LocalPlayer, playerData);

        PlayerInfo info = _infoMap[PhotonNetwork.LocalPlayer];

        InitializePlayerCamera(info.controller);
        _uiManager.Initialize(info, this);

        PlayerInfoListUpdated?.Invoke(_infoMap.Values.ToArray());
    }

    private void InitializePlayer(Player player, PlayerData data)
    {
        var prefab = data.PurchasedCars.Selected.ConfiguratorPrefab;
        var carData = data.PurchasedCars.DataOfSelected;

        Transform spawnpoint = _spawnpoints[player] ?? _spawnpoints.GetFreeSpawnpointForPlayer(player);

        GameObject carGO = PhotonNetwork.Instantiate(data.PurchasedCars.Selected.PrefabPath,
            spawnpoint.position, spawnpoint.rotation);

        var configurator = carGO.GetComponent<CarConfigurator>();
        var stats = carGO.GetComponent<PlayerStats>();
        var controller = carGO.GetComponent<CarController>();
        var view = carGO.GetComponent<PhotonView>();

        configurator.Initialize(carData);

        _infoMap.Add(player, new PlayerInfo()
        {
            nickName = player.NickName,
            controller = controller,
            stats = stats
        });

        PhotonNetwork.RaiseEvent((byte)PUNEvent.NewPlayerInitialized, view.ViewID,
            RaiseEventOptions.Default, SendOptions.SendReliable);
    }

    private void InitializePlayerCamera(CarController carController)
    {
        string username = ProjectContext.Instance.Player.Username;

        GameObject cameraGO = new GameObject($"Camera ({username})");
        cameraGO.AddComponent<Camera>();
        cameraGO.tag = "MainCamera";

        CarCamera carCamera = cameraGO.AddComponent<CarCamera>();
        carCamera.Initialize(carController);
    }

    private PlayerData GetPlayerData(Player player)
    {
        PlayerData playerData = null;

        if (player.CustomProperties.TryGetValue("PlayerData", out var playerJson))
        {
            try
            {
                var serializationData = (PlayerSerializationData)JsonUtility.FromJson(playerJson.ToString(),
                    typeof(PlayerSerializationData));

                return serializationData.Deserialize();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        if (playerData == null)
            Debug.LogError("Unable to get player syncData");

        return playerData;
    }

    #endregion

    private void BeginGameplay()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;

        foreach (var key in _infoMap.Keys)
            _infoMap[key].controller.IsActive = true;

        _currentTimer = _gameplayTime;
    }

    private async void GameOver()
    {
        foreach (var key in _infoMap.Keys)
            _infoMap[key].controller.IsActive = false;

        int defaultReward = CalculateDefaultReward();
        int reward = await _uiManager.ShowGameOverScreenAndCalculateReward(defaultReward);

        ProjectContext.Instance.Player.MoneyAmount += reward;
        Leave();
    }

    private int CalculateDefaultReward()
    {
        int score = _infoMap[PhotonNetwork.LocalPlayer].stats.Score;
        return score / 2;
    }

    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
        ProjectContext.Instance.SceneLoader.LoadSceneAsync(Constants.Scenes.MainMenu);
    }
}

public struct PlayerInfo
{
    public string nickName;
    public PlayerStats stats;
    public CarController controller;
}

public enum GameState
{
    WaitingForPlayers,
    CountdownToStart,
    Playing,
    GameOver
}

public enum PUNEvent : byte
{
    SpawnpointSync = 1,
    SpawnpointTaken,
    SpawnpointReleased,
    NewPlayerInitialized
}