using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class UpgradeItem
{
    public Sprite itemImage;// Image of the item to be displayed in the shop
    public string itemName;// Name of the item to be displayed in the shop
    public string itemDescription;// Description of the item to be displayed in the shop
    public int itemQuantity = 1;
    public bool isPurchased = false; // Flag to check if the item is purchased
    public ItemEffectBase[] effects;// Event to register all the fuctions which need to be called on purchase of item
}