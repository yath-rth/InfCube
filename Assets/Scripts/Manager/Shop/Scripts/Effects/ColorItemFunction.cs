using UnityEngine;

[CreateAssetMenu(menuName = "Shop Item Effects/Change Player Color")]
public class ColorItemFunction : ItemEffectBase
{
    public Material player;
    public override void ApplyEffect(object obj)
    {
        if (obj is ShopItemScriptableIObject item)
            player.SetColor("_BaseColor", item.itemImageTint);
    }

    public override void RemoveEffect(object obj)
    {
        throw new System.NotImplementedException();
    }
}
