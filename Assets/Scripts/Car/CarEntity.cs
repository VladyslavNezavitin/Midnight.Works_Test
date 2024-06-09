using System;
using UnityEngine;


public abstract class CarEntity : ScriptableObject, IShopItem
{
    [SerializeField] private string _name;
    [SerializeField] private int _price;
    [SerializeField] private Sprite _icon;
    [SerializeField] private CarEntityType _type;

    public string Name => _name;
    public int Price => _price;
    public Sprite Icon => _icon;
    public CarEntityType Type => _type;
}

public enum CarEntityType
{
    Car,
    Engine,
    Spoiler,
    Wheels,
    Color
}