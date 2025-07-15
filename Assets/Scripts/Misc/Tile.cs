using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    ObjectPooler pool;
    Rigidbody rb;
    [SerializeField] int fallerLayer, counterLayer;

    private void Awake()
    {
        pool = ObjectPooler.instance;
        rb = GetComponent<Rigidbody>();
    }

    public void setfallerLayer(int layer)
    {
        fallerLayer = layer;
    }

    IEnumerator passBy()
    {
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            yield return new WaitForSeconds(.7f);

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (pool != null) pool.ReturnObject(this.gameObject, 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == fallerLayer)
        {
            StartCoroutine(passBy());
        }

        if(other.gameObject.layer == counterLayer)
        {
            if(GameManager.instance != null) PointsManager.instance.addScore(1);
        }
    }
}
