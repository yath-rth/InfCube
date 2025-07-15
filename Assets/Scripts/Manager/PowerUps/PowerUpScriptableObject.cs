using UnityEngine;

[CreateAssetMenu(fileName = "New PowerUp", menuName = "PowerUp")]
public class PowerUpScriptableObject : ScriptableObject
{
    public GameObject powerUpPrefab;
    public string powerUpName;
    public string description;
    public float duration;
    public PowerUpEffects[] effects;
}
