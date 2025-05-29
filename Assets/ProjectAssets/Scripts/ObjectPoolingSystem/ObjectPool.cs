using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public interface IPoolable
{
    void OnObjectSpawned();   // Se llama cuando el objeto es tomado del pool
    void OnObjectReturned();  // Se llama cuando el objeto es devuelto al pool
}

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private PoolObjectConfig poolObject;
    [SerializeField] private int initialSize = 10;
    [SerializeField] private bool expandable = true;  // Permite crear nuevos objetos si se agota el pool

    [ReadOnly][SerializeField] private Queue<GameObject> availableObjects = new Queue<GameObject>();
    [ReadOnly][SerializeField] private List<GameObject> allObjects = new List<GameObject>();
    private bool usesPoolableInterface;

    private void Awake()
    {
        ValidatePoolObject();
        PrewarmPool();
    }

    private void ValidatePoolObject()
    {
        if (poolObject == null || poolObject.Prefab == null)
        {
            Debug.LogError("PoolObjectConfig no está configurado correctamente");
            return;
        }

        usesPoolableInterface = poolObject.Prefab.GetComponent<IPoolable>() != null;
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < initialSize; ++i)
        {
            CreatePoolObject();
        }
    }

    private GameObject CreatePoolObject()
    {
        GameObject newObj = Instantiate(poolObject.Prefab, transform);
        newObj.SetActive(false);
        availableObjects.Enqueue(newObj);
        allObjects.Add(newObj);
        return newObj;
    }

    public GameObject RequestPoolObject()
    {
        if (availableObjects.Count == 0)
        {
            if (!expandable)
            {
                Debug.LogWarning("Pool agotado y no es expandible");
                return null;
            }
            CreatePoolObject();
        }

        GameObject obj = availableObjects.Dequeue();
        obj.SetActive(true);

        if (usesPoolableInterface)
        {
            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnObjectSpawned();
        }

        return obj;
    }

    public void ReturnPoolObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        availableObjects.Enqueue(obj);

        if (usesPoolableInterface)
        {
            IPoolable poolable = obj.GetComponent<IPoolable>();
            poolable?.OnObjectReturned();
        }
    }

    public List<GameObject> GetAllActiveObjects()
    {
        List<GameObject> activeObjects = new List<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj.activeInHierarchy)
            {
                activeObjects.Add(obj);
            }
        }

        return activeObjects;
    }
}