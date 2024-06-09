using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private Transform _container;
    [SerializeField] private LeaderboardItem _itemPrefab;

    private Dictionary<LeaderboardItem, PlayerInfo> _itemTable = new();

    public void OnPlayerListUpdated(PlayerInfo[] players)
    {
        _itemTable.Clear();

        foreach (Transform child in _container)
            Destroy(child.gameObject);

        foreach (var player in players)
        {
            var item = Instantiate(_itemPrefab, _container);
            _itemTable.Add(item, player);
        }

        UpdateTable();
    }

    public void UpdateTable()
    {
        var sortedTable = _itemTable.OrderByDescending(i => i.Value.stats.Score).ToArray();

        int i = 0;
        foreach (var item in _itemTable.Keys)
        {
            item.PlayerName = sortedTable[i].Value.nickName;
            item.Score = sortedTable[i].Value.stats.Score;
            item.TopPosition = i + 1;
            item.transform.SetSiblingIndex(i);

            item.UpdateItem();

            i++;
        }
    }
}