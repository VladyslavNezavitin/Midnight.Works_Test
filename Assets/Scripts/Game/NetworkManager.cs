using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;
using UnityEngine.Assertions.Must;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public event Action<List<RoomInfo>> RoomListUpdated;
    public event Action Connected;
    public event Action Disconnected;

    [SerializeField] private Canvas _disconnectedCanvas;
    [SerializeField] private TextMeshProUGUI _reconnectTimerText;

    private TaskCompletionSource<bool> _connectedToRoomTCS;
    private TaskCompletionSource<bool> _connectedToLobbyTCS;

    private void Awake()
    {
        PhotonPeer.RegisterType(typeof(SpawnpointsSyncData), 2,
            SpawnpointsSyncData.Serialize, SpawnpointsSyncData.Deserialize);
    }

    public Task UntilConnectedToLobby() => _connectedToLobbyTCS.Task;
    public Task UntilConnectedToRoom() => _connectedToRoomTCS.Task;

    public void InitializeAndConnect(PlayerData player)
    {
        player.UsernameChanged += Player_OnUsernameChanged;

        _connectedToLobbyTCS = new TaskCompletionSource<bool>();
        _connectedToRoomTCS = new TaskCompletionSource<bool>();

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void UpdatePlayerData(PlayerData player)
    {
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        PhotonNetwork.LocalPlayer.NickName = player.Username;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "PlayerData", SaveSystem.GetPlayerJSON() }
        });
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        _disconnectedCanvas.enabled = false;

        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();

        Connected?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon Server: " + cause.ToString());

        Disconnected?.Invoke();
        StartCoroutine(ReconnectionRoutine());
    }

    private IEnumerator ReconnectionRoutine()
    {
        int timer = 3;

        if (PhotonNetwork.IsConnectedAndReady)
            yield break;

        while (!PhotonNetwork.IsConnectedAndReady)
        {
            _disconnectedCanvas.enabled = true;

            if (timer <= 0)
            {
                PhotonNetwork.ConnectUsingSettings();
                break;
            }

            _reconnectTimerText.text = timer.ToString();

            yield return new WaitForSeconds(1);
            timer -= 1;
        }

        if (SceneManager.GetActiveScene().name != Constants.Scenes.MainMenu)
            ProjectContext.Instance.SceneLoader.LoadSceneAsync(Constants.Scenes.MainMenu);
    }

    public override void OnJoinedLobby()
    {
        _connectedToLobbyTCS.TrySetResult(true);
    }

    public override void OnLeftLobby()
    {
        _connectedToLobbyTCS = new TaskCompletionSource<bool>();
    }

    public void CreateRoom(string roomName, string sceneName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
        ProjectContext.Instance.SceneLoader.LoadSceneAsync(sceneName);
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room");
        _connectedToRoomTCS.TrySetResult(true);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
        _connectedToRoomTCS = new TaskCompletionSource<bool>();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) => RoomListUpdated?.Invoke(roomList);

    private void Player_OnUsernameChanged(string name) => PhotonNetwork.LocalPlayer.NickName = name;
}