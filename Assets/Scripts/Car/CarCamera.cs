using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CarCamera : MonoBehaviour
{
    [SerializeField] private CarController _controller;
    [SerializeField] private float _followSpeed = 8f;
    [SerializeField] private float _lookAtSpeed = 15f;

    private Transform _cameraPosition;
    private Transform _target;

    public void Initialize(CarController carController)
    {
        _controller = carController;
        _cameraPosition = carController.CameraPoint;
        _target = carController.CameraTarget;
    }

    private void FixedUpdate()
    {
		if (_controller == null)
			return;
		
        transform.position = Vector3.Lerp(transform.position, _cameraPosition.position, _followSpeed * Time.fixedDeltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(_target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _lookAtSpeed * Time.fixedDeltaTime);
    }
}