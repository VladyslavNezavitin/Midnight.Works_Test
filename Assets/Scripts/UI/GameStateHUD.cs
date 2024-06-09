using TMPro;
using UnityEngine;

public class GameStateHUD : MonoBehaviour
{
    [Header("Root Modules")]
    [SerializeField] private Leaderboard _leaderboard;
    [SerializeField] private GameObject _scoreGO;
    [SerializeField] private CanvasGroup _group;

    [Space, Header("Text Modules")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _gameTimerText;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private TextMeshProUGUI _currentStateText;

    public Leaderboard Leaderboard => _leaderboard;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            _group.alpha = value ? 1f : 0f;
        }
    }

    public void SetScore(int score) => _scoreText.text = $"Score: {score}";

    public void SetCountdownTime(float remainingTime)
    {
        if (_countdownText.gameObject.activeSelf)
            _countdownText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    public void SetGameTime(float remainingTime)
    {
        if (_gameTimerText.gameObject.activeSelf)
            _gameTimerText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    public void OnPlayerListUpdated(PlayerInfo[] players) =>
        _leaderboard.OnPlayerListUpdated(players);

    public void UpdateWaitingForPlayersText(int playersCount, int requiredCount) =>
        _currentStateText.text = $"Waiting for players\n({playersCount}/{requiredCount})";

    public void UpdateLeaderboard() => 
        _leaderboard.UpdateTable();

    public void LoadWaitingForPlayersHUD()
    {
        _currentStateText.gameObject.SetActive(true);
        _scoreGO.SetActive(true);

        _leaderboard.gameObject.SetActive(false);
        _countdownText.gameObject.SetActive(false);
        _gameTimerText.gameObject.SetActive(false);
    }

    public void LoadCountdownHUD()
    {
        _countdownText.gameObject.SetActive(true);

        _currentStateText.gameObject.SetActive(false);
        _gameTimerText.gameObject.SetActive(false);
    }

    public void LoadPlayingHUD()
    {
        _leaderboard.gameObject.SetActive(true);
        _scoreGO.SetActive(true);
        _gameTimerText.gameObject.SetActive(true);

        _currentStateText.gameObject.SetActive(false);
        _countdownText.gameObject.SetActive(false);
    }
}