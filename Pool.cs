using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPoolable<T>
{
    public T Clone();
    public void OnReturnToPool(T instance);
    public void OnLeavingPool(T instance);
}

public class GameObjectPoolWrapper : IPoolable<GameObjectPoolWrapper>
{
    private GameObject inner;

    public GameObjectPoolWrapper(GameObject gameObject)
    {
        inner = gameObject;
    }

    public GameObjectPoolWrapper Clone()
    {
        var instance = UnityEngine.Object.Instantiate(inner).AddComponent<LifetimeHooks>();
        instance.gameObject.SetActive(false);
        return new GameObjectPoolWrapper(instance.gameObject);
    }

    public void OnReturnToPool(GameObjectPoolWrapper instance)
    {
        instance.inner.SetActive(false);
    }

    public void OnLeavingPool(GameObjectPoolWrapper instance)
    {
        instance.inner.SetActive(true);
    }

    public GameObject Unwrap() => inner;

    public static implicit operator GameObject(GameObjectPoolWrapper wrapper)
    {
        return wrapper.Unwrap();
    }

    public static implicit operator GameObjectPoolWrapper(GameObject gameObject)
    {
        return new GameObjectPoolWrapper(gameObject);
    }
}

public class PooledObject<T> : IDisposable where T: class, IPoolable<T>
{
    private T _instance;
    private readonly Action<PooledObject<T>> _returnMethod;
    private bool isDisposed;
    public readonly object objectKey;
    public UnityEvent leavingPool = new UnityEvent();

    public PooledObject(T instance, Action<PooledObject<T>> returnMethod)
    {
        _instance = instance;
        _returnMethod = returnMethod;
        isDisposed = false;
        objectKey = instance;

        leavingPool.AddListener(HandleLeaving);

        if (this is PooledObject<GameObjectPoolWrapper> self)
        {
            if (_instance is GameObjectPoolWrapper wrapper)
            {
                wrapper.Unwrap().GetComponent<LifetimeHooks>().Initialize(self);
            }
        }
    }

    /// <summary>
    /// Allows control over the <see cref="PooledObject{T}"/>'s inner <typeparamref name="T"/> data while automatically disposing of it once finished
    /// <para>YOU DO NOT NEED TO MANUALLY DISPOSE USING THIS METHOD</para>
    /// </summary>
    /// <param name="action">Defined delegate to act on the <typeparamref name="T"/> data.</param>
    public void Use(Action<T> action)
    {
        using (this)
        {
            action(Inner);
        }
    }

    /// <summary>
    /// Allows you manual control over the <see cref="PooledObject{T}"/>'s inner <typeparamref name="T"/> data.
    /// <para>IF YOU USE THIS METHOD YOU MUST MANUALLY DISPOSE ONCE FINISHED</para>
    /// </summary>
    /// <returns></returns>
    public T UseManual()
    {
        return Inner;
    }

    public bool IsInScope()
    {
        return Inner == null;
    }

    public static bool IsInScope(PooledObject<T> pObj)
    {
        return pObj == null || pObj._instance == null;
    }

    ~PooledObject()
    {
        HandleDispose(false);
    }

    private T Inner
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(PooledObject<T>));
            return _instance;
        }
    }

    public void Dispose()
    {
        HandleDispose(true);
        GC.SuppressFinalize(this);
    }

    private void HandleLeaving()
    {
        Inner.OnLeavingPool(Inner);
        //isDisposed = false;
    }

    protected void HandleDispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                Inner.OnReturnToPool(Inner);
                _returnMethod?.Invoke(this);
                Debug.Log($"{this} went out of scope disposing");
            }

            //TODO come up with better spots to determine disposed or not. Instance should likely never be null because the pool doesn't want to have to worry about resetting it
            //_instance = null;
            //isDisposed = true;
        }
    }
}

public interface IPool
{
    object Get();
    void Return(object obj);
}

public interface IPool<T> : IPool where T: class, IPoolable<T>
{
    PooledObject<T> GetFromPool();
    void ReturnToPool(PooledObject<T> obj);
}

public class ObjectPool<T> : IPool<T> where T: class, IPoolable<T>
{
    private readonly Queue<PooledObject<T>> poolQueue = new Queue<PooledObject<T>>();
    private T originObject;

    public ObjectPool(int initialSize, T obj)
    {
        originObject = obj;
        for (int i = 0; i < initialSize; i++)
        {
            poolQueue.Enqueue(CreatePooledObject());
        }
    }

    public PooledObject<T> CreatePooledObject()
    {
        return new PooledObject<T>(originObject.Clone(), ReturnToPool);
    }

    public PooledObject<T> GetFromPool()
    {
        if (poolQueue.Count > 0)
        {
            var obj = poolQueue.Dequeue();
            obj.leavingPool.Invoke();
            return obj;
        }
        else
        {
            return CreatePooledObject();
        }
    }

    public void ReturnToPool(PooledObject<T> obj)
    {
        poolQueue.Enqueue(obj);
    }

    object IPool.Get()
    {
        return GetFromPool();
    }

    void IPool.Return(object obj)
    {
        ReturnToPool((PooledObject<T>)obj);
    }
}

//public class Pool<T> : IPool<T> where T: new()
//{
//    private readonly Queue<T> poolQueue = new Queue<T>();

//    public Pool(int initialSize)
//    {
//        for (int i = 0; i < initialSize; i++)
//        {
//            poolQueue.Enqueue(new T());
//        }
//    }

//    public T GetFromPool()
//    {
//        return poolQueue.Count > 0 ? poolQueue.Dequeue() : new T();
//    }

//    public void ReturnToPool(T obj)
//    {
//        poolQueue.Enqueue(obj);
//    }

//    object IPool.Get()
//    {
//        return GetFromPool();
//    }

//    void IPool.Return(object obj)
//    {
//        ReturnToPool((T)obj);
//    }
//}

//public class PrefabPool : IPool<GameObject>
//{
//    public readonly Queue<GameObject> poolQueue = new Queue<GameObject>();
//    public GameObject internalPrefab;
//    public List<GameObject> instances = new List<GameObject>();

//    public PrefabPool(int initialSize, GameObject prefab)
//    {
//        internalPrefab = prefab;

//        for (int i = 0; i < initialSize; i++)
//        {
//            var instance = UnityEngine.Object.Instantiate(prefab);
//            instances.Add(instance);
//            instance.SetActive(false);
//            poolQueue.Enqueue(instance);
//        }
//    }

//    public GameObject GetFromPool()
//    {
//        if (poolQueue.Count > 0)
//        {
//            var instance = poolQueue.Dequeue();
//            instance.SetActive(true);
//            return instance;
//        }
//        else
//        {
//            var instance = UnityEngine.Object.Instantiate(internalPrefab);
//            instances.Add(instance);
//            return instance;
//        }
//    }

//    public void ReturnToPool(GameObject obj)
//    {
//        obj.SetActive(false);
//        poolQueue.Enqueue(obj);
//    }

//    object IPool.Get()
//    {
//        return GetFromPool();
//    }

//    void IPool.Return(object obj)
//    {
//        ReturnToPool((GameObject)obj);
//    }
//}