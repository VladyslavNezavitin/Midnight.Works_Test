using Photon.Pun;
using UnityEngine;

public class CarEffects : MonoBehaviour, IPunObservable
{
    [SerializeField] private TrailRenderer[] _trails;

    private PhotonView _photonView;
    
    private bool _emittingTireTrails;
    private bool EmittingTireTrails
    {
        get => _emittingTireTrails;
        set
        {
            if (value == _emittingTireTrails)
                return;

            _emittingTireTrails = value;

            foreach (var trail in _trails)
                trail.emitting = _emittingTireTrails;
        }
    }

    private void Awake() => _photonView = GetComponent<PhotonView>();

    private void Update()
    {
        HandleTrails();
    }

    private void HandleTrails()
    {
        if (_photonView.IsMine)
        {
            EmittingTireTrails = InputManager.Instance.BrakePressed;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(EmittingTireTrails);
        }
        else
        {
            EmittingTireTrails = (bool)stream.ReceiveNext();
        }
    }
}
