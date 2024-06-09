using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    private const string APP_KEY_ANDROID = "1ebf8e545";
    private TaskCompletionSource<bool> _rewardedVideoTCS;
    private bool _rewardedVideoAvailable;
    private bool _rewardedVideoFailed;

    private void Start()
    {
        IronSource.Agent.validateIntegration();
        IronSource.Agent.init(APP_KEY_ANDROID);
    }

    private void OnEnable()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent += SDK_Initialized;

        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
    }

    private void OnDisable()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent -= SDK_Initialized;

        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }

    private void SDK_Initialized() =>
        Debug.Log("Iron Source SDK initialized successfully!");

    private void RewardedVideoOnAdAvailable(IronSourceAdInfo info) =>
        _rewardedVideoAvailable = true;
    private void RewardedVideoOnAdUnavailable() =>
        _rewardedVideoFailed = true;
    private void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info) =>
        _rewardedVideoFailed = true;
    private void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo info) =>
        _rewardedVideoTCS.SetResult(true);

    public Task<bool> ShowRewardedVideo()
    {
        _rewardedVideoTCS = new TaskCompletionSource<bool>();

        IronSource.Agent.loadRewardedVideo();
        StartCoroutine(ShowVideoWhenReadyRoutine());

        return _rewardedVideoTCS.Task;
    }

    private IEnumerator ShowVideoWhenReadyRoutine()
    {
        while (!_rewardedVideoAvailable)
        {
            if (_rewardedVideoFailed)
            {
                Debug.LogError("Failed to load a video");

                _rewardedVideoTCS.SetResult(false);
                yield break;
            }

            yield return null;
        }

        IronSource.Agent.showRewardedVideo();
    }
}