using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour
{
    ObjectPooler pool;
    Rigidbody rb;
    [SerializeField] int poolIndex = 1;
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

            yield return new WaitForSeconds(.4f);
            transform.DOScale(Vector3.zero, 0.8f);
            yield return new WaitForSeconds(0.5f);

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (pool != null)
        {
            transform.localScale = Vector3.one;
            pool.ReturnObject(gameObject, poolIndex);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == fallerLayer)
        {
            StartCoroutine(passBy());
        }

        if (other.gameObject.layer == counterLayer)
        {
            if (GameManager.instance != null) PointsManager.instance.addScore(1);
        }
    }
}
