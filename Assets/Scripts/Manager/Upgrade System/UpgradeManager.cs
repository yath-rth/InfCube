using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class UpgradeManager : MonoBehaviour
{
    public List<UpgradeItem> AllUpgradeItems; // List of shop items which are available in the sho
    public static UpgradeManager Instance; // Singleton instance of the UpgradeManager
    public GameObject upgradeUI;
    public int threshold = 10;
    int lastRecorededScore = 0; // Variable to keep track of the last recorded score

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
    }

    void Update()
    {
        if (GameManager.instance != null)
        {
            if (PointsManager.instance.score - lastRecorededScore >= threshold)
            {
                lastRecorededScore = PointsManager.instance.score; // Update the last recorded score
                if(upgradeUI != null) upgradeUI.SetActive(true); // Show the upgrade UI when the score threshold is reached
            }
        }
    }

    public void PurchaseItem(UpgradeItem item)
    {
        if (item.itemQuantity <= 0)
        {
            Debug.Log("Item is out of stock.");
            if (item.isPurchased)
            {
                Debug.Log("Item is already purchased but out of stock.");
            }

            return;
        }

        if (item.isPurchased)
        {
            Debug.Log("Item is already purchased and out of stock.");
            return;
        }

        // Assuming we have a method to check player's currency
        item.isPurchased = true;// Mark the item as purchased

        foreach (ItemEffectBase effect in item.effects)
        {
            effect.ApplyEffect(item); // Apply all effects associated with the item 
        }

        Time.timeScale = 1f; // Resume the game time after purchase
        if (upgradeUI != null) upgradeUI.SetActive(false); // Hide the upgrade UI after purchase
    }
}