using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class pickableItem : MonoBehaviour
{
    public delegate void pickAction(PowerUpScriptableObject obj);
    public static event pickAction OnItemPick;

    public PowerUpScriptableObject obj;
    public int layers;

    private void Update()
    {
        transform.Rotate(80f * Time.deltaTime, 100f * Time.deltaTime, 50f * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == layers)
        {
            if(OnItemPick != null) OnItemPick(obj);
            Destroy(gameObject);
        }
    }
}
