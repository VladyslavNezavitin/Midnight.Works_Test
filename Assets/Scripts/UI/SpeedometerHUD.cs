using UnityEngine;
using UnityEngine.UI;

public class SpeedometerHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup _speedometerGroup;
    [SerializeField] private float _needlePeakAngle = -230f;
    [SerializeField] private float _needleMaxAngle = -260f;
    [SerializeField] private float _needleSpeed;
    [SerializeField] private Image _speedometerNeedle;

    private CarController _car;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            _speedometerGroup.alpha = _isActive ? 1f : 0f;
        }
    }

    public void Initialize(CarController car)
    {
        _car = car;
    }

    private void Update()
    {
        if (!IsActive)
            return; 

        float targetRotation = Mathf.LerpUnclamped(0f, _needlePeakAngle, _car.CurrentSpeedKPH / _car.MaxSpeedKPH);
        targetRotation = Mathf.Clamp(targetRotation, _needleMaxAngle, 0f);

        Vector3 currentRotation = _speedometerNeedle.transform.eulerAngles;
        currentRotation.z = Mathf.MoveTowardsAngle(currentRotation.z, targetRotation, _needleSpeed);

        _speedometerNeedle.transform.eulerAngles = currentRotation;
    }
}
