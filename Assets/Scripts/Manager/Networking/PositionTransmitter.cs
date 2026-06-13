using UnityEngine;

public class PositionTransmitter : MonoBehaviour
{
    [SerializeField] private float sendInterval = 1.0f;
    float timer = 0f;

    void Update()
    {
        if (sceneManager.GameState != 1) return;
        if (GameManager.instance.isGameOver) return;
        timer += Time.deltaTime;

        if (ConnectionManager.instance != null && Player.instance != null && timer >= sendInterval)
        {
            timer = 0f;
            ConnectionManager.instance.SendPosition(
                new PositionPayload(Player.instance.transform.position)
            );
        }
    }
}
