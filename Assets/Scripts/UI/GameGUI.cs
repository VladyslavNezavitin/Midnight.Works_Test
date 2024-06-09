using System;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class GameGUI : MonoBehaviour
{
    public event Action ExitRequested;

    private Canvas _canvas;

    private void Awake() => _canvas = GetComponent<Canvas>();
    public void Show() => _canvas.enabled = true;
    public void Hide() => _canvas.enabled = false;
    protected void ExitGUI() => ExitRequested?.Invoke();
}