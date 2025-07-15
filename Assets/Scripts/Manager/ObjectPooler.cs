using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler instance;

    public List<objectPoolItems> items;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;

        for (int i = 0; i < items.Count; i++)
        {
            CreateObject(items[i].StartSize, items[i].Prefab, items[i].objectPool);
        }
    }

    void CreateObject(float size, GameObject prefab, Queue<GameObject> pool)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject bullet = Instantiate(prefab, this.transform);
            pool.Enqueue(bullet);
            bullet.SetActive(false);
        }
    }

    public GameObject GetObject(int i)
    {
        if (items[i].objectPool.Count > 0)
        {
            GameObject obj = items[i].objectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(items[i].Prefab, this.transform);
            items[i].objectPool.Enqueue(obj);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj, int i)
    {
        i -= 1;

        items[i].objectPool.Enqueue(obj);
        obj.SetActive(false);
    }
}

[System.Serializable]
public class objectPoolItems
{
    public float StartSize;
    public GameObject Prefab;
    public Queue<GameObject> objectPool = new Queue<GameObject>();
}
