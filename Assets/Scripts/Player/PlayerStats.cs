using Photon.Pun;
using System;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class PlayerStats : MonoBehaviour, IPunObservable
{
    public event Action<int> ScoreChanged;

    private CarController _carController;
    private bool _isDrifting;

    private int _score;
    public int Score
    {
        get => _score;
        private set
        {
            _score = value;
            ScoreChanged?.Invoke(_score);
        }
    }

    private void Awake() => _carController = GetComponent<CarController>();

    public void Update()
    {
        _isDrifting = InputManager.Instance.BrakePressed;

        if (_isDrifting)
        {
            int points = (int)Mathf.Abs(_carController.CurrentDriftAngle / 10);
            Score += points;
        }
    }

    public void ShowAngle() => Debug.LogError(_carController.CurrentDriftAngle);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(Score);
        else
            Score = (int)stream.ReceiveNext();
    }
}