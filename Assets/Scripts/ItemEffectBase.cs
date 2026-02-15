using UnityEngine;

public abstract class ItemEffectBase : ScriptableObject
{
    public abstract void ApplyEffect(object obj);
    public abstract void RemoveEffect(object obj);
}
