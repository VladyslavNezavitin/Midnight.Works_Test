using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class SpawnpointCollection : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] private List<Transform> _spawnpointTransforms;
    private List<Spawnpoint> _spawnpoints;
    private TaskCompletionSource<bool> _syncTCS;

    private void Awake()
    {
        _spawnpoints = new List<Spawnpoint>();
        _syncTCS = new TaskCompletionSource<bool>();

        for (int i = 0; i < _spawnpointTransforms.Count; i++)
        {
            _spawnpoints.Add(new Spawnpoint()
            {
                index = i,
                spawnpoint = _spawnpointTransforms[i],
            });
        }
    }

    public Task UntilSync() => _syncTCS.Task;

    public void OnEvent(EventData photonEvent)
    {
        switch ((PUNEvent)photonEvent.Code)
        {
            case PUNEvent.SpawnpointSync:
                OnSyncDataReceived((SpawnpointsSyncData)photonEvent.CustomData); 
                break;
            case PUNEvent.SpawnpointTaken:
                object[] data = (object[])photonEvent.CustomData;
                OnSpawnpointTaken((int)data[0], (Player)data[1]); 
                break;
            case PUNEvent.SpawnpointReleased:
                OnSpawnpointReleased((int)photonEvent.CustomData); 
                break;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) => SendSyncData(newPlayer);

    private void SendSyncData(Player target)
    {
        SpawnpointsSyncData syncData = new SpawnpointsSyncData()
        {
            takenIndices = _spawnpoints
                .Where(s => s.isTaken)
                .Select(s => s.index)
                .ToArray(),

            ownerActorNumbers = _spawnpoints
                .Where(s => s.isTaken)
                .Select(s => s.player.ActorNumber)
                .ToArray()
        };

        PhotonNetwork.RaiseEvent((byte)PUNEvent.SpawnpointSync, syncData,
            new RaiseEventOptions { TargetActors = new int[] { target.ActorNumber } }, SendOptions.SendReliable);
    }

    private void OnSyncDataReceived(SpawnpointsSyncData syncData)
    {
        var players = PhotonNetwork.CurrentRoom.Players;

        foreach (var p in players)
            Debug.Log(p.Value);

        for (int i = 0; i < syncData.takenIndices.Length; i++)
        {
            _spawnpoints[syncData.takenIndices[i]].isTaken = true;
            _spawnpoints[syncData.takenIndices[i]].player = players[syncData.ownerActorNumbers[i]];
        }

        _syncTCS.SetResult(true);
    }

    private void OnSpawnpointTaken(int index, Player player)
    {
        _spawnpoints[index].player = player;
        _spawnpoints[index].isTaken = true;
    }

    private void OnSpawnpointReleased(int index)
    {
        _spawnpoints[index].player = null;
        _spawnpoints[index].isTaken = false;
    }

    public Transform this[Player player]
    {
        get
        {
            foreach (var spawnpoint in _spawnpoints)
            {
                if (spawnpoint.player != null)
                {
                    if (spawnpoint.player.ActorNumber == player.ActorNumber)
                        return spawnpoint.spawnpoint;
                }
            }

            return null;
        }
    }

    public Transform GetFreeSpawnpointForPlayer(Player player)
    {
        for (int i = 0; i < _spawnpoints.Count; i++)
        {
            if (!_spawnpoints[i].isTaken)
            {
                _spawnpoints[i].player = player;
                _spawnpoints[i].isTaken = true;

                PhotonNetwork.RaiseEvent((byte)PUNEvent.SpawnpointTaken, new object[] { i, player },
                    RaiseEventOptions.Default, SendOptions.SendReliable);

                return _spawnpoints[i].spawnpoint;
            }
        }

        return null;
    }

    public void ReleaseSpawnpoint(Player player)
    {
        var spawnpoint = _spawnpoints.Find(s => s.player.ActorNumber == player.ActorNumber);
        
        if (spawnpoint != null)
        {
            spawnpoint.player = null;
            spawnpoint.isTaken = false;

            PhotonNetwork.RaiseEvent((byte)PUNEvent.SpawnpointReleased, spawnpoint.index,
                RaiseEventOptions.Default, SendOptions.SendReliable);
        }
    }

    public int GetIndexOfTaken(Player player)
    {
        foreach (var spawnpoint in _spawnpoints)
        {
            if (spawnpoint.isTaken && spawnpoint.player.ActorNumber == player.ActorNumber)
                return spawnpoint.index;
        }

        return -1;
    }
}

[Serializable]
public struct SpawnpointsSyncData
{
    public int[] ownerActorNumbers;
    public int[] takenIndices;

    public static byte[] Serialize(object data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }
    }

    public static object Deserialize(byte[] data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream(data))
        {
            return formatter.Deserialize(stream);
        }
    }
}

[Serializable]
public class Spawnpoint
{
    public int index;
    public Transform spawnpoint;
    public Player player;
    public bool isTaken;
}