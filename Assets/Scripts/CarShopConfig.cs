using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarShopConfig", menuName = "Scriptable Objects/Car Shop Config")]
public class CarShopConfig : ScriptableObject
{
    [SerializeField] private List<CarConfig> _cars;
    public List<CarConfig> AvailableCars => _cars;

    public void InitializeConfigs()
    {
        foreach (var config in _cars)
            config.Initialize();
    }
}