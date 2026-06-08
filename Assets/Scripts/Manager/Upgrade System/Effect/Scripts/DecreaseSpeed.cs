using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade Item Effects/Decrease Speed")]
public class DecreaseSpeed : ItemEffectBase
{
    public float speedDecreaseAmount = 0.5f; // Amount to decrease the speed by

    public override void ApplyEffect(object obj)
    {
        if (obj is UpgradeItem item)
        {
            // Assuming the player has a method to decrease speed
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();
            if (player != null)
            {
                player.DecreaseSpeed(speedDecreaseAmount);
            }
        }
    }

    public override void RemoveEffect(object obj)
    {
        throw new System.NotImplementedException();
    }
}
