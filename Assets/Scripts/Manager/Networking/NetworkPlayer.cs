using DG.Tweening;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public void movePlayer(Vector3 position)
    {
        Vector3 pos = new Vector3(-position.x, -1f, position.z);
        transform.DOMove(pos, .1f);
    }
}
