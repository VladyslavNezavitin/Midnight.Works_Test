using System.Collections;
using UnityEngine;

public class PlayerGameHUD : GameGUI
{
    [Header("Modules")]
    [SerializeField] private SpeedometerHUD _speedometerHUD;
    [SerializeField] private GameStateHUD _gameStateHUD;

    public Leaderboard Leaderboard => _gameStateHUD.Leaderboard;
    private Game _gameManager;
    private PlayerStats _stats;

    private void Update()
    {
        if (_gameManager == null)
            return;

        if (_gameManager.CurrentState == GameState.CountdownToStart ||
            _gameManager.CurrentState == GameState.Playing)
        {
            OnTimerTick(_gameManager.CurrentTimer);
        }
    }

    public void Initialize(PlayerInfo info, Game gameManager)
    {
        _gameManager = gameManager;
        _gameManager.GameStateChanged += Game_OnStateChanged;
        _gameManager.PlayerInfoListUpdated += Game_OnPlayerListUpdated;

        _speedometerHUD.Initialize(info.controller);
        _stats = info.stats;
        _stats.ScoreChanged += Player_OnScoreChanged;

        Player_OnScoreChanged(_stats.Score);
        Game_OnStateChanged(_gameManager.CurrentState);
        Game_OnPlayerListUpdated(_gameManager.Players);
    }

    private void Game_OnStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.WaitingForPlayers:
                _speedometerHUD.IsActive = false;
                _gameStateHUD.IsActive = true;
                _gameStateHUD.LoadWaitingForPlayersHUD();
                break;
            case GameState.CountdownToStart:
                _speedometerHUD.IsActive = false;
                _gameStateHUD.LoadCountdownHUD();
                break;
            case GameState.Playing:
                _speedometerHUD.IsActive = true;
                _gameStateHUD.LoadPlayingHUD();
                StartCoroutine(LeaderboardUpdateRoutine());
                break;
            case GameState.GameOver:
                StopAllCoroutines();
                _speedometerHUD.IsActive = true;
                _gameStateHUD.IsActive = false;
                break;
        }
    }

    private void OnTimerTick(float remainingTime)
    {
        switch (_gameManager.CurrentState)
        {
            case GameState.CountdownToStart: OnCountdownToStartTick(remainingTime); break;
            case GameState.Playing: OnGameTimerTick(remainingTime); break;
        }
    }

    private void Player_OnScoreChanged(int score) => _gameStateHUD.SetScore(score);
    private void OnCountdownToStartTick(float remainingTime) => _gameStateHUD.SetCountdownTime(remainingTime);
    private void OnGameTimerTick(float remainingTime) => _gameStateHUD.SetGameTime(remainingTime);

    private void Game_OnPlayerListUpdated(PlayerInfo[] players)
    {
        _gameStateHUD.OnPlayerListUpdated(players);
        _gameStateHUD.UpdateWaitingForPlayersText(_gameManager.Players.Length, _gameManager.PlayersRequiredToStart);
    }
    
    public void UI_LeaveButtonPressed() => _gameManager.Leave();

    private IEnumerator LeaderboardUpdateRoutine()
    {
        while (_gameManager.CurrentState == GameState.Playing)
        {
            _gameStateHUD.UpdateLeaderboard();
            yield return new WaitForSeconds(1);
        }
    }
}