using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    [SerializeField] GameObject coinMesh, coinParticles;

    private void Update()
    {
        coinMesh.transform.Rotate(0, 100f * Time.deltaTime, 0, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            if (coinMesh != null) coinMesh.SetActive(false);
            if (GameManager.instance != null) PointsManager.instance.addCoin();
            if (coinParticles != null) coinParticles.SetActive(true);
            StartCoroutine(collected());
        }
    }

    IEnumerator collected()
    {
        yield return new WaitForSeconds(1f);
        if (coinMesh != null) coinMesh.SetActive(true);
        if (coinParticles != null) coinParticles.SetActive(false);
        if (ObjectPooler.instance != null) ObjectPooler.instance.ReturnObject(gameObject, 2);
    }
}
