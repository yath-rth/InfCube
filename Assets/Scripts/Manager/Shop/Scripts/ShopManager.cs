using System.Collections.Generic;
using System.Linq;
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

    bool loadedFromSave = false;

    public string id => Name;

    int cost;

    void Start()
    {
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
            Debug.Log("Loaded data for shop");
        }

        if (!loadedFromSave)
        {
            for (int i = 0; i < AllShopItems.Count; i++)
            {
                ShopItemsInstances.Add(Instantiate(AllShopItems[i]));
            }
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

        if (item.parameters.itemQuantity <= 0)
        {
            Debug.Log("Item is out of stock.");
            if (item.parameters.isPurchased)
            {
                Debug.Log("Item is already purchased but out of stock.");
                foreach (ItemEffectBase effect in item.effects)
                {
                    effect.ApplyEffect(item); // Invoke all the effects associated with the item
                }
            }

            return;
        }

        // Assuming we have a method to check player's currency
        if (item.parameters.itemPrice <= playerCurrency)
        {
            item.parameters.isPurchased = true;// Mark the item as purchased
            cost = item.parameters.itemPrice; // Deduct the item's price from player's currency
            item.parameters.itemQuantity--;// Decrease the item's quantity

            Debug.Log($"Purchased {item.itemName} for {item.parameters.itemPrice} coins.");

            foreach (ItemEffectBase effect in item.effects)
            {
                effect.ApplyEffect(item); // Invoke all the effects associated with the item
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
        if (data is ShopData d)
        {
            ShopItemsInstances = new List<ShopItemScriptableIObject>();

            for (int i = 0; i < d.shopItems.Count; i++)
            {
                var baseItem = Instantiate(AllShopItems[i]);
                baseItem.parameters = d.shopItems[i];
                ShopItemsInstances.Add(baseItem);
            }

            loadedFromSave = true;
        }
    }

    public object SaveData()
    {
        return new ShopData
        {
            shopItems = ShopItemsInstances.Select(item => item.parameters).ToList()
        };
    }
}

public class ShopData
{
    public List<SaveableVariables> shopItems;
}