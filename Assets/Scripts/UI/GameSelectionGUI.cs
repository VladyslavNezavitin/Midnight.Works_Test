using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectionGUI : GameGUI
{
    [SerializeField] private Transform _roomListContentParent;
    [SerializeField] private Button _roomButtonPrefab;
    [SerializeField] private TMP_InputField _newRoomNameField;

    private NetworkManager NetworkManager => ProjectContext.Instance.NetworkManager;

    private void OnEnable() => NetworkManager.RoomListUpdated += Network_OnRoomListUpdated;
    private void OnDisable() => NetworkManager.RoomListUpdated -= Network_OnRoomListUpdated;

    private void Network_OnRoomListUpdated(List<RoomInfo> rooms)
    {
        foreach (Transform child in _roomListContentParent)
            Destroy(child.gameObject);

        foreach (var room in rooms.Where(r => !r.RemovedFromList && r.IsVisible))
        {
            Button button = Instantiate(_roomButtonPrefab, _roomListContentParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = room.Name;
            button.onClick.AddListener(() => OnJoinRoomButtonPressed(room.Name));
        }
    }

    private async void OnJoinRoomButtonPressed(string roomName)
    {
        await NetworkManager.UntilConnectedToLobby();
        NetworkManager.JoinRoom(roomName);
    } 


    // TODO: instantiate level list from scriptable object procedurally
    
    private string _selectedSceneName;

    public void OnLevelButtonPressed(string levelName)
    {
        _selectedSceneName = levelName;
    }

    public async void OnCreateNewRoomButtonPressed()
    {
        if (string.IsNullOrEmpty(_selectedSceneName))
        {
            Debug.LogError("Select desired level!");
            return;
        }

        if (_newRoomNameField.text.Length < 3 || _newRoomNameField.text.Length > 16)
        {
            Debug.LogError("Room name should be 3 to 16 characters long!");
            return;
        }

        await NetworkManager.UntilConnectedToLobby();
        NetworkManager.CreateRoom(_newRoomNameField.text, _selectedSceneName);
    }

    public void OnBackButtonPressed() => ExitGUI();
}