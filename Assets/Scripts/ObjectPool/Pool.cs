using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

public class Pool<T>
{
    protected Func<T> m_factory;

    protected readonly Action<T> m_reset;

    protected readonly List<T> m_available;

    protected readonly List<T> m_all;

    public Pool(Func<T> factory, Action<T> reset, int initialCapacity)
    {
        if (factory == null)
            throw new ArgumentNullException("factory");

        m_available = new List<T>();
        m_all = new List<T>();
        m_factory = factory;
        m_reset = reset;

        if (initialCapacity > 0)
            Grow(initialCapacity);
    }

    public Pool(Func<T> factory) : this(factory, null, 0) { }

    public Pool(Func<T> factory, int initialCapacity) : this(factory, null, initialCapacity) { }

    public virtual T Get()
    {
        return Get(null);
    }

    public virtual T Get(Action<T> resetOverride)
    {
        if (m_available.Count == 0)
            Grow(1);

        if (m_available.Count == 0)
            throw new InvalidOperationException("Failed to grow pool");

        int itemIndex = m_available.Count - 1;
        T item = m_available[itemIndex];
        m_available.RemoveAt(itemIndex);

        if (resetOverride != null)
            resetOverride(item);

        return item;
    }

    public virtual bool Contains(T pooledItem)
    {
        return m_all.Contains(pooledItem);
    }

    public virtual void Return(T pooledItem)
    {
        if (m_all.Contains(pooledItem) && !m_available.Contains(pooledItem))
            ReturnToPoolInternal(pooledItem);
        else
            throw new InvalidOperationException("Trying to return an item to a pool that does not contain it " + pooledItem + ", " + this);
    }

    public virtual void ReturnAll()
    {
        ReturnAll(null);
    }

    public virtual void ReturnAll(Action<T> preReturn)
    {
        for(int i = 0; i < m_all.Count; ++i)
        {
            T item = m_all[i];
            if(!m_available.Contains(item))
            {
                if (preReturn != null)
                    preReturn(item);

                ReturnToPoolInternal(item);
            }
        }
    }

    public void Grow(int amount)
    {
        for (int i = 0; i < amount; ++i)
            AddNewElement();
    }

    protected virtual T AddNewElement()
    {
        T newElement = m_factory();
        m_all.Add(newElement);
        m_available.Add(newElement);

        return newElement;
    }

    protected virtual void ReturnToPoolInternal(T element)
    {
        m_available.Add(element);
    }

    protected static T DummyFactory()
    {
        return default(T);
    }
}

public class UnityComponentPool<T>: Pool<T> where T : Component
{
    public UnityComponentPool(Func<T> factory, Action<T> reset, int initialCapacity) : base(factory, reset, initialCapacity) { }

    public UnityComponentPool(Func<T> factory) : base(factory) { }

    public UnityComponentPool(Func<T> factory, int initialCapacity) : base(factory, initialCapacity) { }

    public override T Get(Action<T> resetOverride)
    {
        T element = base.Get(resetOverride);
        element.gameObject.SetActive(true);

        return element;
    }

    protected override void ReturnToPoolInternal(T element)
    {
        element.gameObject.SetActive(false);

        base.ReturnToPoolInternal(element);
    }

    protected override T AddNewElement()
    {
        T newElement = base.AddNewElement();

        newElement.gameObject.SetActive(false);

        return newElement;
    }
}

public class GameObjectPool: Pool<GameObject>
{
    public GameObjectPool(Func<GameObject> factory, Action<GameObject> reset, int initialCapacity) : base(factory, reset, initialCapacity) { }

    public GameObjectPool(Func<GameObject> factory) : base(factory) { }

    public GameObjectPool(Func<GameObject> factory, int initialCapacity) : base(factory, initialCapacity) { }

    public override GameObject Get(Action<GameObject> resetOverride)
    {
        GameObject element = base.Get(resetOverride);
        element.SetActive(true);

        return element;
    }

    protected override void ReturnToPoolInternal(GameObject element)
    {
        element.SetActive(false);

        base.ReturnToPoolInternal(element);
    }

    protected override GameObject AddNewElement()
    {
        GameObject newElement =  base.AddNewElement();
        newElement.SetActive(false);

        return newElement;
    }
}

public class AutoGameObbjectPrefabPool: GameObjectPool
{
    GameObject PrefabFactory()
    {
        GameObject newElement = Object.Instantiate(m_prefab);
        if (m_initialize != null)
            m_initialize(newElement);

        return newElement;
    }

    protected readonly GameObject m_prefab;

    protected readonly Action<GameObject> m_initialize;

    public AutoGameObbjectPrefabPool(GameObject prefab, Action<GameObject> initialize, Action<GameObject> reset, int initialCapacity): base(DummyFactory, reset, 0)
    {
        m_initialize = initialize;
        m_prefab = prefab;
        m_factory = PrefabFactory;

        if (initialCapacity > 0)
            Grow(initialCapacity);
    }

    public AutoGameObbjectPrefabPool(GameObject prefab) : this(prefab, null, null, 0) { }

    public AutoGameObbjectPrefabPool(GameObject prefab, Action<GameObject> initialize) : this(prefab, initialize, null, 0) { }

    public AutoGameObbjectPrefabPool(GameObject prefab, Action<GameObject> initialize, Action<GameObject> reset) : this(prefab, initialize, reset, 0) { }

    public AutoGameObbjectPrefabPool(GameObject prefab, int initialCapacity) : this(prefab, null, null, initialCapacity) { }
}

public class AutoComponentPrefabPool<T> : UnityComponentPool<T> where T : Component
{
    T PrefabFactory()
    {
        T newElement = Object.Instantiate(m_prefab);
        if (m_initialize != null)
            m_initialize(newElement);

        return newElement;
    }

    protected readonly T m_prefab;

    protected readonly Action<T> m_initialize;

    public AutoComponentPrefabPool(T prefab, Action<T> initialize, Action<T> reset, int initialCapacity)
    : base(DummyFactory, reset, 0)
    {
        // Pass 0 to initial capacity because we need to set ourselves up first
        // We then call Grow again ourselves
        m_initialize = initialize;
        m_prefab = prefab;
        m_factory = PrefabFactory;
        if (initialCapacity > 0)
        {
            Grow(initialCapacity);
        }
    }

    public AutoComponentPrefabPool(T prefab) : this(prefab, null, null, 0) { }

    public AutoComponentPrefabPool(T prefab, Action<T> initialize) : this(prefab, initialize, null, 0) { }

    public AutoComponentPrefabPool(T prefab, Action<T> initialize, Action<T> reset) : this(prefab, initialize, reset, 0) { }

    public AutoComponentPrefabPool(T prefab, int initialCapacity) : this(prefab, null, null, initialCapacity) { }
}