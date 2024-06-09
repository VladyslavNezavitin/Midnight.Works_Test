using UnityEngine;

[CreateAssetMenu(fileName = "CarWheels", menuName = "Scriptable Objects/Car Color")]
public class CarColor : CarEntity
{
    [SerializeField] private Color _color;
    public Color Color => _color;
}