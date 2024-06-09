using UnityEngine;

public interface IShopItem
{
    string Name { get; }
    int Price { get; }
    Sprite Icon { get; }
}