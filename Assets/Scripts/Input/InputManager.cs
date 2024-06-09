using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInputActions _actions;

    public Vector2 MoveInput { get; private set; }
    public bool BrakePressed { get; private set; }

    public static InputManager Instance { get; private set; }



    private void Awake()
    {
        _actions = new PlayerInputActions();

        Instance = this;
    }

    private void OnEnable()
    {
        _actions.Enable();
    }

    private void OnDisable()
    {
        _actions.Disable();
    }

    private void Update()
    {
        MoveInput = _actions.Gameplay.Movement.ReadValue<Vector2>();
        BrakePressed = _actions.Gameplay.Brake.IsPressed();
    }
}
