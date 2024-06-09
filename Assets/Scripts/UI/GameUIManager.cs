using System.Threading.Tasks;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private PlayerGameHUD _playerHud;
    [SerializeField] private GameOverScreen _gameOverScreen;
    [SerializeField] private GameGUI _carControlGUI;

    private PlayerStats _stats;

    public void Initialize(PlayerInfo playerInfo, Game game)
    {
        game.GameStateChanged += Game_OnStateChanged;
        _playerHud.Initialize(playerInfo, game);
        _stats = playerInfo.stats;

        _playerHud.Show();
        _carControlGUI.Show();
        _gameOverScreen.Hide();
    }

    private void Game_OnStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            _playerHud.Hide();
            _carControlGUI.Hide();
            _gameOverScreen.Show();
        }
    }

    public Task<int> ShowGameOverScreenAndCalculateReward(int initialReward)
    {
        return _gameOverScreen.InitializeAndGetFinalReward(_stats.Score, initialReward);
    }
}