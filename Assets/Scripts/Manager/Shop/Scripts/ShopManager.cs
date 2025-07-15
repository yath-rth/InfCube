using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour, ISaveFuncs
{
    public string Name; // Name of the shop, can be used for identification for each individual shop
    [SerializeField] private List<ShopItemScriptableIObject> AllShopItems; // List of shop items which are available in the shop
    public List<ShopItemScriptableIObject> ShopItemsInstances { get; private set; } = new List<ShopItemScriptableIObject>(); // List of instanced items and can be accesed from other scripts
    [SerializeField] private GameObject shopTile;// Prefab for each shop item tile
    [SerializeField] private Transform tileParent; // Parent transform where shop tiles will be instantiated and the content of the scroll rect
    [SerializeField] private TMP_Text currencyDisplay;
    public int playerCurrency;// Player's current currency amount
    public static ShopManager Instance { get; private set; } // Singleton instance of the ShopManager

    public string id => Name;

    int cost;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        //To first check if the scroll rect has been assigned a content container
        if (tileParent == null)
        {
            Debug.LogError("Tile Parent is not assigned in the ShopManager.");
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterObject(this);
            SaveManager.Instance.LoadDataForObject(this);
        }

        if (PointsManager.instance != null)
        {
            playerCurrency = PointsManager.instance.Allcoins;
            if (currencyDisplay != null) currencyDisplay.text = playerCurrency.ToString("D4");
        }

        for (int i = 0; i < AllShopItems.Count; i++)
        {
            GameObject tile = Instantiate(shopTile, tileParent);
            if (tile != null)//Null check to avoid null pointer error
            {
                if (AllShopItems[i] != null)
                {
                    if(ShopItemsInstances.Count == 0) ShopItemsInstances.Add(Instantiate(AllShopItems[i]));
                    if (tile != null)
                    {
                        ShopItemTile item = tile.GetComponent<ShopItemTile>();
                        if (item != null) item.setupTile(ShopItemsInstances[i], PurchaseItem);
                    }
                }
            }
        }
    }

    public void PurchaseItem(ShopItemScriptableIObject item)
    {
        playerCurrency = PointsManager.instance.Allcoins;

        if (item.itemQuantity <= 0)
        {
            Debug.Log("Item is out of stock.");
            if (item.isPurchased)
            {
                Debug.Log("Item is already purchased but out of stock.");
                foreach (ShopItemEffects effect in item.effects)
                {
                    effect.Apply(item); // Invoke all the effects associated with the item
                }
            }

            return;
        }

        if (item.isPurchased)
        {
            Debug.Log("Item is already purchased and out of stock.");

            foreach (ShopItemEffects effect in item.effects)
            {
                effect.Apply(item); // Invoke all the effects associated with the item
            }

            return;
        }

        // Assuming we have a method to check player's currency
        if (item.itemPrice <= playerCurrency)
        {
            item.isPurchased = true;// Mark the item as purchased
            cost = item.itemPrice; // Deduct the item's price from player's currency
            item.itemQuantity--;// Decrease the item's quantity

            Debug.Log($"Purchased {item.itemName} for {item.itemPrice} coins.");

            foreach (ShopItemEffects effect in item.effects)
            {
                effect.Apply(item); // Invoke all the effects associated with the item
            }
        }
        else
        {
            Debug.Log("Not enough currency to purchase this item.");
        }

        if (PointsManager.instance != null)
        {
            PointsManager.instance.removeCoins(cost);
            if (currencyDisplay != null) currencyDisplay.text = PointsManager.instance.Allcoins.ToString("D4");

            SaveManager.Instance.SaveData();
        }
    }

    public void LoadData(object data)
    {
        if(data is ShopData d)
        {
            foreach (var item in d.shopItems)
            {
                ShopItemsInstances.Add(item);
            }
        }
    }

    public object SaveData()
    {
        return new ShopData
        {
            shopItems = ShopItemsInstances
        };
    }
}

public class ShopData
{
    public List<ShopItemScriptableIObject> shopItems;
}