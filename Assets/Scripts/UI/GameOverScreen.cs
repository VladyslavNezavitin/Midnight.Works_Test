using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class GameOverScreen : GameGUI
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _rewardText;

    private TaskCompletionSource<int> _rewardTCS;
    private int _currentReward;

    public Task<int> InitializeAndGetFinalReward(int score, int initialReward)
    {
        _rewardTCS = new TaskCompletionSource<int>();

        _currentReward = initialReward;

        _scoreText.text = $"Score: {score}";
        _rewardText.text = $"Reward: {_currentReward}";

        Show();

        return _rewardTCS.Task;
    }

    public async void OnGetDoubleRewardButtonPressed()
    {
#if UNITY_EDITOR

        Debug.LogError("Cannot show ad in unity editor (reward will be doubled anyway)");
        _currentReward *= 2;
#else
        bool result = await ProjectContext.Instance.AdManager.ShowRewardedVideo();

        if (result)
            _currentReward *= 2;
#endif
        _rewardTCS.SetResult(_currentReward);
    }

    public void OnLeaveButtonPressed()
    {
        _rewardTCS.SetResult(_currentReward);
    }
}