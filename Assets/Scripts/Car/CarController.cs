using System;
using Photon.Pun;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private Transform _cameraPoint;
    [SerializeField] private Transform _cameraTarget;

    [SerializeField] private float _motorForce;
    [SerializeField] private float _handBrakeForce;
    [SerializeField] private float _brakeForce;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _handBrakeFrictionMultiplier;
    [SerializeField] private float _steeringSensitivity;
    [SerializeField] private float _driftSensitivity;
    [SerializeField] private float _downForce;

    [SerializeField] private DriveUnit _driveUnit;
    [SerializeField] private WheelData[] _wheels;

    private float _ackermannRadius = 6f;
    private float _driftFactor;

    private Rigidbody _rb;
    private InputManager _input;
    private PhotonView _photonView;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;

            if (_isActive)
            {
                _photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
                _rb.isKinematic = false;
            }
            else
            {
                _photonView.Synchronization = ViewSynchronization.Off;
                _rb.isKinematic = true;
            }
        }
    }

    public float CurrentSpeed { get; private set; }
    public float CurrentDriftAngle { get; private set; }
    public float CurrentSpeedKPH => CurrentSpeed * 3.6f;
    public float MaxSpeedKPH => _maxSpeed;
    public Transform CameraPoint => _cameraPoint;
    public Transform CameraTarget => _cameraTarget;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _photonView = GetComponent<PhotonView>();
        _input = InputManager.Instance;
        IsActive = false;
    }

    private void Update()
    {
        if (!IsActive)
            return;

        UpdateWheelVisuals();
    }

    private void FixedUpdate()
    {
        if (!_isActive || !_photonView.IsMine)
            return;

        CurrentSpeed = _rb.velocity.magnitude;

        if (CurrentSpeed > 1f)
        {
            Vector3 forceDirection = _rb.velocity / CurrentSpeed;
            CurrentDriftAngle = Vector3.SignedAngle(transform.forward, forceDirection, Vector3.up);
        }
        else
        {
            CurrentDriftAngle = 0f;
        }

        if (_photonView.IsMine)
        {
            HandleDrift();
            HandleSteering();
            HandleMotor();
            ApplyDownForce();

            _ackermannRadius = 6f + CurrentSpeedKPH / 20f;
        }  
    }

    private void HandleMotor()
    {
        float motorTorqueFactor = _driveUnit switch
        {
            DriveUnit.FourWheel => 0.25f,
            _ => 0.5f,
        };

        float brakeTorque = 0f;

        if (!_input.BrakePressed && Mathf.Abs(_input.MoveInput.y) < 0.01f)
        {
            brakeTorque = _brakeForce;
        }
        else
        {
            float dot = Vector3.Dot(_rb.velocity, transform.forward * _input.MoveInput.y);
            if (dot < -0.5f)
                brakeTorque = _brakeForce * -dot;
        }

        float motorForce = _motorForce * (1f - Mathf.Clamp01(CurrentSpeedKPH / MaxSpeedKPH));
        float motorTorque = CurrentSpeedKPH <= _maxSpeed ? _input.MoveInput.y * motorForce * motorTorqueFactor : 0f;

        for (int i = 0; i < _wheels.Length; i++)
        {
            if (_driveUnit == DriveUnit.FourWheel ||
                _wheels[i].type == WheelType.Front && _driveUnit == DriveUnit.Front ||
                _wheels[i].type == WheelType.Rear && _driveUnit == DriveUnit.Rear)
            {
                _wheels[i].collider.motorTorque = motorTorque;
                _wheels[i].collider.brakeTorque = brakeTorque;
            }

            if (_wheels[i].type == WheelType.Rear)
            {
                if (_input.BrakePressed)
                    _wheels[i].collider.brakeTorque = _handBrakeForce;
                else
                    _wheels[i].collider.brakeTorque = brakeTorque;
            }
        }
    }

    private void HandleSteering()
    {
        float targetAngle1 = 0f;
        float targetAngle2 = 0f;

        if (Mathf.Abs(_input.MoveInput.x) > 0.1f)
        {
            if (_input.MoveInput.x > 0)
            {
                targetAngle1 = Mathf.Rad2Deg * Mathf.Atan(2.55f / (_ackermannRadius + (1.5f / 2f))) * _input.MoveInput.x;
                targetAngle2 = Mathf.Rad2Deg * Mathf.Atan(2.55f / (_ackermannRadius - (1.5f / 2f))) * _input.MoveInput.x;
            }
            else
            {
                targetAngle2 = Mathf.Rad2Deg * Mathf.Atan(2.55f / (_ackermannRadius + (1.5f / 2f))) * _input.MoveInput.x;
                targetAngle1 = Mathf.Rad2Deg * Mathf.Atan(2.55f / (_ackermannRadius - (1.5f / 2f))) * _input.MoveInput.x;
            }
        }

        _wheels[0].collider.steerAngle = Mathf.MoveTowardsAngle(_wheels[0].collider.steerAngle, targetAngle1, _steeringSensitivity);
        _wheels[1].collider.steerAngle = Mathf.MoveTowardsAngle(_wheels[1].collider.steerAngle, targetAngle2, _steeringSensitivity);
    }

    private void ApplyDownForce()
    {
        _rb.AddForce(-transform.up * _downForce * CurrentSpeed);
    }

    private void HandleDrift()
    {
        WheelFrictionCurve forwardFriction = _wheels[0].collider.forwardFriction;
        WheelFrictionCurve sidewaysFriction = _wheels[0].collider.sidewaysFriction;

        if (_input.BrakePressed)
        {
            float targetValue = _driftFactor * _handBrakeFrictionMultiplier;

            forwardFriction.extremumValue = forwardFriction.asymptoteValue =
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
            Mathf.MoveTowards(forwardFriction.extremumValue, targetValue, _driftSensitivity);

            for (int i = 0; i < _wheels.Length; i++)
            {
                _wheels[i].collider.forwardFriction = forwardFriction;
                _wheels[i].collider.sidewaysFriction = sidewaysFriction;
            }

            forwardFriction.extremumValue = forwardFriction.asymptoteValue =
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = 1.1f;

            for (int i = 0; i < _wheels.Length; i++)
            {
                if (_wheels[i].type == WheelType.Front)
                {
                    _wheels[i].collider.forwardFriction = forwardFriction;
                    _wheels[i].collider.sidewaysFriction = sidewaysFriction;
                }
            }

            _rb.AddForce(transform.forward * Mathf.Abs(CurrentDriftAngle) * 150);
            _rb.AddTorque(0f, CurrentDriftAngle * 15, 0f);
        }
        else
        {
            forwardFriction.extremumValue = forwardFriction.asymptoteValue =
            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue =
            (CurrentSpeedKPH * _handBrakeFrictionMultiplier / 300f) + 1f;

            for (int i = 0; i < _wheels.Length; i++)
            {
                _wheels[i].collider.forwardFriction = forwardFriction;
                _wheels[i].collider.sidewaysFriction = sidewaysFriction;
            }
        }

        for (int i = 0; i < _wheels.Length; i++)
        {
            if (_wheels[i].type == WheelType.Rear)
            {
                float targetStiffness = _input.BrakePressed ? .5f : 1f;

                forwardFriction.stiffness = Mathf.MoveTowards(forwardFriction.stiffness, targetStiffness, _driftSensitivity);
                sidewaysFriction.stiffness = Mathf.MoveTowards(forwardFriction.stiffness, targetStiffness, _driftSensitivity);

                _wheels[i].collider.forwardFriction = forwardFriction;
                _wheels[i].collider.sidewaysFriction = sidewaysFriction;

                _wheels[i].collider.GetGroundHit(out var hit);

                if (hit.sidewaysSlip > 0)
                    _driftFactor = (1f - _input.MoveInput.x) * Mathf.Abs(hit.sidewaysSlip);
                else
                    _driftFactor = (1f + _input.MoveInput.x) * Mathf.Abs(hit.sidewaysSlip);
            }
        }
    }

    private void UpdateWheelVisuals()
    {
        for (int i = 0; i < _wheels.Length; i++)
        {
            _wheels[i].collider.GetWorldPose(out var position, out var rotation);
            _wheels[i].mesh.transform.eulerAngles = rotation.eulerAngles;
            _wheels[i].mesh.transform.Rotate(_wheels[i].meshRotationOffset);
        }
    }
}


[Serializable]
public struct WheelData
{
    public GameObject mesh;
    public WheelCollider collider;
    public WheelType type;
    public Vector3 meshRotationOffset;
}

public enum WheelType
{
    Rear,
    Front
}

public enum DriveUnit
{
    Front,
    Rear,
    FourWheel
}