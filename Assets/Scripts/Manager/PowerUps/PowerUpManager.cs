using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager instance; // Singleton instance of the PowerUpManager
    public PowerUpScriptableObject[] powerUps;
    public PowerUpScriptableObject activePowerUp;
    float spawnedTime = 0f;

    void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;

        pickableItem.OnItemPick += ActivatePowerUp;
    }

    public void Update()
    {
        if (activePowerUp != null)
        {
            // Check if the power-up duration has expired
            if (Time.time - spawnedTime > activePowerUp.duration) RemoveActivePowerUp();
        }
    }

    public void SpawnPowerUp(Vector3 position)
    {
        float a = Random.Range(0f, 1f);

        if (a < -0.3f) //Change the number to a negative number to stop spawning powerups
        {
            if (powerUps.Length == 0)
            {
                Debug.LogWarning("No power-ups available to spawn.");
                return;
            }

            int randomIndex = Random.Range(0, powerUps.Length);
            GameObject powerUp = Instantiate(powerUps[randomIndex].powerUpPrefab, position, Quaternion.identity, transform);//Can be configured to use a object pool instead of instantiating every time}
        }
    }

    public void ActivatePowerUp(PowerUpScriptableObject obj)
    {
        if (activePowerUp == null)
        {
            activePowerUp = obj;
            foreach (PowerUpEffects effect in activePowerUp.effects)
            {
                effect.ApplyEffect(activePowerUp);
            }

            spawnedTime = Time.time; // Record the time when the power-up was spawned
        }
    }

    public void RemoveActivePowerUp()
    {
        if (activePowerUp != null)
        {
            foreach (PowerUpEffects effect in activePowerUp.effects)
            {
                effect.RemoveEffect(activePowerUp);
            }

            activePowerUp = null;
        }
    }
}
