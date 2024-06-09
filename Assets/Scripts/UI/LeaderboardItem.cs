using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public int TopPosition { get; set; }

    public void UpdateItem()
    {
        _text.text = $"[{TopPosition}] {PlayerName}: {Score}";
    }
}