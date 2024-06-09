using UnityEngine;

[CreateAssetMenu(fileName = "CarPart", menuName = "Scriptable Objects/Car Part")]
public class CarPart : CarEntity
{
    [SerializeField] private GameObject _model;
    [SerializeField] private CarStats _stats;

    public GameObject Model => _model;
    public CarStats Stats => _stats;
}