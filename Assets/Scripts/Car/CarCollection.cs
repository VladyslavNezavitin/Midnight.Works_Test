using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct CarData
{
    public CarConfig config;
    public List<CarEntity> availableEntities;
    public List<CarEntity> selectedEntities;

    public CarData(CarConfig config, List<CarEntity> availableEntities = null, List<CarEntity> selectedEntities = null)
    {
        if (config == null)
            throw new ArgumentException("Config cannot be null!");
            
        this.config = config;
        this.availableEntities = availableEntities?.ToList() ?? config.StockEntities;
        this.selectedEntities = selectedEntities?.ToList() ?? this.availableEntities
            .GroupBy(e => e.Type)
            .Select(g => g.First())
            .ToList();
    }

    public void SelectEntity(CarEntity entity)
    {
        if (availableEntities.Contains(entity))
        {
            selectedEntities.RemoveAll(e => e.Type == entity.Type);
            selectedEntities.Add(entity);
        }
        else
            Debug.Log("Failed to select entity"); 
    }
}

public struct CarCollection
{
    private Dictionary<CarConfig, CarData> _carMap;

    public List<CarData> Cars { get; private set; }
    public CarConfig Selected { get; private set; }
    public IEnumerable<KeyValuePair<CarConfig, CarData>> CarMap => _carMap.AsReadOnlyCollection();

    public bool Contains(CarConfig car) => _carMap.ContainsKey(car);
    public int IndexOfSelected => Cars.IndexOf(_carMap[Selected]);
    public CarData DataOfSelected => _carMap[Selected];



    public CarCollection(List<CarConfig> cars, CarConfig selected = null)
    {
        if (cars == null || cars.Count == 0)
            throw new InvalidOperationException("Cannot create CarCollection from empty list!");

        _carMap = new Dictionary<CarConfig, CarData>();
        Cars = new List<CarData>();


        if (cars != null)
        {
            foreach (var car in cars)
            {
                CarData carData = new CarData(car);

                _carMap.Add(car, carData);
                Cars.Add(carData);
            }
        }

        Selected = selected ?? Cars?.First().config;

        if (Selected == null)
            Debug.LogWarning("Car collection is empty!");
    }

    public CarCollection(Dictionary<CarConfig, CarData> carMap, CarConfig selected)
    {
        _carMap = new Dictionary<CarConfig, CarData>(carMap);
        Selected = selected;
        Cars = carMap.Values.ToList();
    }

    public bool TrySelectCar(CarConfig car)
    {
        if (_carMap.ContainsKey(car))
        {
            Selected = car;
            return true;
        }

        return false;
    }

    public bool TryAdd(CarConfig car)
    {
        if (_carMap.ContainsKey(car))
            return false;

        CarData data = new CarData(car);
        _carMap.Add(car, data);
        Cars.Add(data);

        return true;
    }

    public bool TryAddEntity(CarConfig car, CarEntity entity)
    {
        if (!_carMap.ContainsKey(car) || _carMap[car].availableEntities.Contains(entity))
            return false;

        _carMap[car].availableEntities.Add(entity);
        return true;
    }

    public bool TrySelectEntity(CarConfig car, CarEntity entity)
    {
        if (!_carMap.ContainsKey(car) || !_carMap[car].availableEntities.Contains(entity))
            return false;

        _carMap[car].SelectEntity(entity);
        return true;
    }

    public bool TryGetData(CarConfig car, out CarData data)
    {
        if (!_carMap.ContainsKey(car))
        {
            data = new CarData();
            return false;
        }
        else
        {
            data = _carMap[car];
            return true;
        }
    }
}

[Serializable]
public struct CarCollectionSerializationData
{
    public List<string> carMapConfigSOKeyNames;
    public List<CarDataSerializationData> carMapCarDataValuesData;
    public string selectedConfigSOName;

    public CarCollectionSerializationData(CarCollection collection)
    {
        carMapConfigSOKeyNames = new List<string>();
        carMapCarDataValuesData = new List<CarDataSerializationData>();
        selectedConfigSOName = collection.Selected.name;

        foreach (var kvp in collection.CarMap)
        {
            carMapConfigSOKeyNames.Add(kvp.Key.name);
            carMapCarDataValuesData.Add(new CarDataSerializationData(kvp.Value));
        }
    }

    public CarCollection Deserialize()
    {
        var carMap = new Dictionary<CarConfig, CarData>();

        string path = Path.Combine(Constants.Resources.CarDataFolder, selectedConfigSOName);
        CarConfig selected = Resources.Load<CarConfig>(path);

        for (int i = 0; i < carMapConfigSOKeyNames.Count; i++)
        {
            path = Path.Combine(Constants.Resources.CarDataFolder, carMapConfigSOKeyNames[i]);

            CarConfig key = Resources.Load<CarConfig>(path);
            CarData value = carMapCarDataValuesData[i].Deserialize();

            carMap.Add(key, value);
        }

        return new CarCollection(carMap, selected);
    }
}

[Serializable]
public struct CarDataSerializationData
{
    public string configSOName;
    public List<string> availableEntitiesSONames;
    public List<string> selectedEntitiesSONames;

    public CarDataSerializationData(CarData data)
    {
        configSOName = data.config.name;
        availableEntitiesSONames = new List<string>();
        selectedEntitiesSONames = new List<string>();

        foreach (var available in data.availableEntities)
            availableEntitiesSONames.Add(available.name);

        foreach (var selected in data.selectedEntities)
            selectedEntitiesSONames.Add(selected.name);
    }

    public CarData Deserialize()
    {
        var availableEntities = new List<CarEntity>();
        var selectedEntities = new List<CarEntity>();

        string path = Path.Combine(Constants.Resources.CarDataFolder, configSOName);
        CarConfig config = Resources.Load<CarConfig>(path);

        foreach (var available in availableEntitiesSONames)
        {
            path = Path.Combine(Constants.Resources.CarDataFolder, available);
            CarEntity entity = Resources.Load<CarEntity>(path);

            availableEntities.Add(entity);
        }

        foreach (var selected in selectedEntitiesSONames)
        {
            path = Path.Combine(Constants.Resources.CarDataFolder, selected);
            CarEntity entity = Resources.Load<CarEntity>(path);

            selectedEntities.Add(entity);
        }

        return new CarData(config, availableEntities, selectedEntities);
    }
}