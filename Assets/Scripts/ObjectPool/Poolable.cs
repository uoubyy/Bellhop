using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    public int initialPoolCapacity = 10;

    public Pool<Poolable> pool;

    protected static void TryPool(GameObject gameObject)
    {
        var poolable = gameObject.GetComponent<Poolable>();
        if (poolable != null && poolable.pool != null && PoolManager.InstanceExists)
            poolable.RePool();
        else
            Destroy(gameObject);
    }

    protected virtual void RePool()
    {
        transform.SetParent(PoolManager.Instance.transform, false);
        pool.Return(this);
    }

    public static T TryGetPoolable<T>(GameObject prefab) where T : Component
    {
        var poolable = prefab.GetComponent<Poolable>();
        T instance = (poolable != null && PoolManager.InstanceExists) ? PoolManager.Instance.GetPoolable(poolable).GetComponent<T>() : Instantiate(prefab).GetComponent<T>();
        return instance;
    }

    public static GameObject TryGetPoolable(GameObject prefab)
    {
        var poolable = prefab.GetComponent<Poolable>();
        GameObject instance = (poolable != null && PoolManager.InstanceExists) ? PoolManager.Instance.GetPoolable(poolable).gameObject : Instantiate(prefab);
        return instance;
    }
}
