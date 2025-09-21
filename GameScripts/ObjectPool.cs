// ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (prefab == null) Debug.LogError("ObjectPool prefab not set.");
        for (int i = 0; i < initialSize; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            var g = pool.Dequeue();
            g.SetActive(true);
            return g;
        }
        else
        {
            var g = Instantiate(prefab, transform);
            return g;
        }
    }

    public void Return(GameObject g)
    {
        g.SetActive(false);
        g.transform.SetParent(transform, false);
        pool.Enqueue(g);
    }
}
