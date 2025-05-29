using UnityEngine;

[CreateAssetMenu(fileName = "PoolObjectConfig", menuName = "ScriptableObjects/ObjectPool/PoolObjectConfig")]
public class PoolObjectConfig : ScriptableObject
{
    [SerializeField] private GameObject prefab;

    public GameObject Prefab
    {
        get
        {
            return prefab;
        }
        set
        {
            prefab = value;
        }
    }
}