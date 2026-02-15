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
            player player = GameObject.FindWithTag("Player").GetComponent<player>();
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
