using Mirror;
using System;
using UnityEngine;

public class ShopInventory : NetworkBehaviour
{
    public CarShopData[] cars;
    public UpgradeShopData[] upgrades;
    public static ShopInventory Core;

    public void Start()
    {
        Core = this;
    }

    [Command]
    public void CmdBuy(int ItemID, ShopItemCatagory itemCatagory)
    {
        switch (itemCatagory)
        {
            case ShopItemCatagory.Car:

                break;
            case ShopItemCatagory.Upgrade:
                break;
        }
    }

    public void buy()
    {
    }
}

public enum ShopItemCatagory
{
    Car = 0,
    Upgrade = 1
}

[Serializable]
public struct CarShopData
{
    public int ShopID;
    public string GameName;
    public GameObject Car;
    public GameObject NetCar;
    public float price;
    public bool isPurchased;
}

public struct UpgradeShopData
{
    public string GameName;
    public GameObject Car;
    public GameObject NetCar;
    public float price;
    public bool isPurchased;
}
