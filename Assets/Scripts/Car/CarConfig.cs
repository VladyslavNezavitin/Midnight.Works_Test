using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CarConfig", menuName = "Scriptable Objects/Car Config")]
public class CarConfig : ScriptableObject, IShopItem
{
    [Header("Car info")]
    [SerializeField] private string _name;
    [SerializeField] private int _price;
    [SerializeField] private Sprite _icon;
    [SerializeField] private CarStats _baseStats;
    [SerializeField] private CarConfigurator _configuratorPrefab;

    [Space, Header("Tunable Entities (The firsts are stock)")]
    [SerializeField] private TunableCollection _tunables;

    public void Initialize() => _tunables.Initialize();

    public string Name => _name;
    public int Price => _price;
    public Sprite Icon => _icon;
    public CarStats BaseStats => _baseStats;
    public string PrefabPath => System.IO.Path.Combine(Constants.Resources.CarPrefabFolder, _configuratorPrefab.name);
    public TunableCollection Tunables => _tunables;
    public CarConfigurator ConfiguratorPrefab => _configuratorPrefab;
    public List<CarEntity> StockEntities => _tunables.Groups.Select(g => g.entities.First()).ToList();

    private void OnValidate()
    {
        _tunables.ValidateGroups();
    }
}