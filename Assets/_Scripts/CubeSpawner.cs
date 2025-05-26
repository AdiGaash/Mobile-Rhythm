using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CubeSpawner : MonoBehaviour
{
// Prefab to spawn
    public GameObject prefab;

    // List to store the pool of objects
    private List<GameObject> pool = new List<GameObject>();

    // Number of objects to pre-instantiate
    public int initialPoolSize = 10;

    void Start()
    {
        // Create initial pool
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledItem();
        }
    }

    // Not static! It works with the specific pool instance
    private void CreatePooledItem()
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false); // deactivate initially
        pool.Add(obj);        // add to pool list
    }

    private GameObject poolObject;
    // Method to get an object from the pool
    public GameObject GetPooledObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        // If none available, create a new one (optional)
        CreatePooledItem();
        return pool[pool.Count - 1];
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
             poolObject = GetPooledObject();
             poolObject.SetActive(true);
        }

        if (Input.GetKey(KeyCode.B))
        {
            poolObject.SetActive(false);
        }
    }
}
