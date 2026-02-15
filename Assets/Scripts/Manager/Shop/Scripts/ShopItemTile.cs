using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ShopItemTile : MonoBehaviour
{
    public Image tileItemImage;
    public TMP_Text tileText;

    public void setupTile(ShopItemScriptableIObject item, UnityAction<ShopItemScriptableIObject> PurchaseItem)
    {
        Button tileButton = GetComponent<Button>();
        if (tileButton != null) // Check if the button and item are not null
        {
            tileButton.onClick.AddListener(() => PurchaseItem(item));
        }
        else Debug.LogError("Button component or ShopItemScriptableIObject is null for tile at index ");

        if (tileItemImage != null && item.itemImage != null) // Check if the image component and item image are not null
        {
            tileItemImage.sprite = item.itemImage; // Set the item image
            tileItemImage.color = item.itemImageTint; // Set the item image tint color
        }
        else Debug.LogError("Image component or item image is null for tile at index ");

        if (tileText != null) // Check if the text component are not null
        {
            tileText.text = item.parameters.itemPrice.ToString(); // Set the item price text
        }
        else Debug.LogError("Text component or item price is null for tile at index ");
    }
}
