using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade Item Effects/Increase Size")]
public class IncreaseSize : ItemEffectBase
{
    public float sizeIncreaseAmount = 0.5f; // Amount to increase the size by
    public override void ApplyEffect(object obj)
    {
        if (obj is UpgradeItem item)
        {
            // Assuming the player has a method to increase size
            GameObject manager = GameObject.FindWithTag("Game Manager");
            if (manager != null)
            {
                manager.GetComponent<pathSpawner>().tileSize += sizeIncreaseAmount; // Increase the tile size
            }
        }
    }

    public override void RemoveEffect(object obj)
    {
        
    }
}
