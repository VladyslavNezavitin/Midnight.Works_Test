using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _progressText;
    private bool _playingFadeAnimation;

    public async void LoadSceneAsync(string sceneName)
    {
        _progressText.text = "0%";
        _canvasGroup.gameObject.SetActive(true);

        StartCoroutine(FadeAnimationRoutine(0f, 1f));
        while (_playingFadeAnimation)
            await Task.Delay(100);

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!op.isDone)
        {
            _progressText.text = $"{(int)(op.progress * 100)}%";
            await Task.Delay(100);
        }

        _progressText.text = "100%";

        StartCoroutine(FadeAnimationRoutine(1f, 0f));   
        while (_playingFadeAnimation)
            await Task.Delay(100);

        _canvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator FadeAnimationRoutine(float initialAlpha, float targetAlpha)
    {
        _playingFadeAnimation = true;
        _canvasGroup.alpha = initialAlpha;

        float time = 1f;
        float timer = 0f;

        while (timer < 1f)
        {
            _canvasGroup.alpha = Mathf.Lerp(initialAlpha, targetAlpha, timer);
            timer += Time.deltaTime / time;
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        _playingFadeAnimation = false;
    }
}