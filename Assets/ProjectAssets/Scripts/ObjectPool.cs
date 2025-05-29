using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private PoolObjectConfig poolObject;
    [SerializeField] private int initialSize;
    [SerializeField] private List<GameObject> objectPool;

    private void Start()
    {
        AddPoolObjectsToPool(initialSize);
    }

    private void AddPoolObjectsToPool(int amount)
    {
        for(int i = 0; i < amount; ++i)
        {
            GameObject poolObjectGenerated = Instantiate(poolObject.Prefab);
            poolObject.Prefab.SetActive(false);
            objectPool.Add(poolObjectGenerated);
            poolObjectGenerated.transform.parent = transform;
        }
    }

    public GameObject RequestPoolObject()
    {
        for(int i = 0; i < objectPool.Count; ++i)
        {
            if (objectPool[i].activeSelf == false)
            {
                objectPool[i].SetActive(true);
                return objectPool[i];
            }
        }
        AddPoolObjectsToPool(1);
        objectPool[objectPool.Count - 1].SetActive(true);
        return objectPool[objectPool.Count - 1];
    }
}
