using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Car : MonoBehaviour
{
    [SerializeField] private CarConfig _config;
    [SerializeField] private CarConfigurator _configurator;

    public string Name => _config.name;
    public float Price => _config.Price;
    public Sprite Icon => _config.Icon;
    public CarStats BaseStats => _config.BaseStats;
    public TunableCollection Tunables => _config.Tunables;
    public List<CarEntity> StockEntities => _config.StockEntities;
    public List<CarEntity> AppliedEntities => _configurator.AppliedEntities;
}