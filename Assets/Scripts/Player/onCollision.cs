using UnityEngine;
using UnityEngine.Events;

public class onCollision : MonoBehaviour
{
    [SerializeField] LayerMask layers;
    public UnityEvent collided;

    private void OnTriggerEnter(Collider other) {
        if((layers.value & (1 << other.gameObject.layer)) != 0)
        {
            collided?.Invoke();
        }
    }
}
