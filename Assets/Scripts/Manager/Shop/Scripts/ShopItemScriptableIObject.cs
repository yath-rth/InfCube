using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
[CreateAssetMenu(fileName = "ShopItem", menuName = "Shop Items")]
public class ShopItemScriptableIObject : ScriptableObject
{
    public Sprite itemImage;// Image of the item to be displayed in the shop
    public Color itemImageTint = Color.white; // Tint color for the item image
    public string itemName;// Name of the item to be displayed in the shop
    public string itemDescription;// Description of the item to be displayed in the shop
    public GameObject itemPrefab;// Prefab of the item to be instantiated when purchased
    public SaveableVariables parameters;
    public ItemEffectBase[] effects;// Event to register all the fuctions which need to be called on purchase of item
}

[System.Serializable]
public class SaveableVariables
{
    public int itemPrice;// Price of the item in the shop
    public int itemQuantity;// Quantity of the item available in the shop
    public bool isPurchased;// Flag to check if the item has been purchased
}