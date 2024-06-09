using System;
using UnityEngine;

public class PlayerData
{
    public event Action<float> MoneyAmountChanged;
    public event Action<float> GoldAmountChanged;
    public event Action<string> UsernameChanged;

    private CarCollection _purchasedCars;
    private int _moneyAmount;
    private int _goldAmount;
    private string _username;

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            UsernameChanged?.Invoke(_username);
        }
    }

    public int MoneyAmount
    {
        get => _moneyAmount;
        set
        {
            _moneyAmount = value;
            MoneyAmountChanged?.Invoke(_moneyAmount);
        }
    }

    public int GoldAmount
    {
        get => _goldAmount;
        set
        {
            _goldAmount = value;
            GoldAmountChanged?.Invoke(_goldAmount);
        }
    }

    public CarCollection PurchasedCars => _purchasedCars;
      
    public PlayerData(string username, int moneyAmount, int goldAmount, CarCollection carCollection)
    {
        Username = username;
        MoneyAmount = moneyAmount;
        GoldAmount = goldAmount;
        _purchasedCars = carCollection;
    }

    public void SelectCar(CarConfig car) => _purchasedCars.TrySelectCar(car);

    public void SelectCarEntity(CarConfig car, CarEntity entity)
    {
        if (_purchasedCars.TryGetData(car, out var data))
            data.SelectEntity(entity);
    }

    public bool TryPurchaseCar(CarConfig car)
    {
        if (MoneyAmount <= car.Price)
            return false;

        if (_purchasedCars.TryAdd(car) && _purchasedCars.TrySelectCar(car))
        {
            MoneyAmount -= car.Price;
            return true;
        }

        Debug.LogError("Failed to purchase a car");
        return false;
    }

    public bool TryPurchaseCarEntity(CarConfig car, CarEntity entity)
    {
        if (MoneyAmount <= entity.Price)
            return false;

        if (_purchasedCars.TryAddEntity(car, entity) &&
            _purchasedCars.TrySelectEntity(car, entity))
        {
            MoneyAmount -= entity.Price;
            return true;
        }

        Debug.LogError("Failed to purchase a car entity!");
        return false;
    }
}

[Serializable]
public struct PlayerSerializationData
{
    public string username;
    public int moneyAmount;
    public int goldAmount;
    public CarCollectionSerializationData purchasedCarsData;

    public PlayerSerializationData(PlayerData player)
    {
        username = player.Username;
        moneyAmount = player.MoneyAmount;
        goldAmount = player.GoldAmount;

        purchasedCarsData = new CarCollectionSerializationData(player.PurchasedCars);
    }

    public PlayerData Deserialize()
    {
        CarCollection purchasedCars = purchasedCarsData.Deserialize();
        return new PlayerData(username, moneyAmount, goldAmount, purchasedCars);
    }
}