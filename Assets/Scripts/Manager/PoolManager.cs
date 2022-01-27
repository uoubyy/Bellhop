using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    public List<Poolable> poolables;

    protected Dictionary<Poolable, AutoComponentPrefabPool<Poolable>> m_pools;

    void Initialize(Component poolable)
    {
        poolable.transform.SetParent(transform, false);
    }

    public Poolable GetPoolable(Poolable poolablePrefab)
    {
        if(!m_pools.ContainsKey(poolablePrefab))
            m_pools.Add(poolablePrefab, new AutoComponentPrefabPool<Poolable>(poolablePrefab, Initialize, null, poolablePrefab.initialPoolCapacity));

        AutoComponentPrefabPool<Poolable> pool = m_pools[poolablePrefab];
        Poolable spawnedInstance = pool.Get();
        spawnedInstance.pool = pool;
        return spawnedInstance;
    }

    public void ReturnPoolable(Poolable poolable)
    {
        poolable.pool.Return(poolable);
    }

    void Start()
    {
        m_pools = new Dictionary<Poolable, AutoComponentPrefabPool<Poolable>>();

        foreach(var poolable in poolables)
        {
            if (poolable == null)
                continue;

            m_pools.Add(poolable, new AutoComponentPrefabPool<Poolable>(poolable, Initialize, null, poolable.initialPoolCapacity));
        }
    }
}
