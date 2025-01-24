using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectPoolInterface
{
    public static GameObjectPoolWrapper ToWrapped(this GameObject obj) => new GameObjectPoolWrapper(obj);

    //public static GameObject GetCopyFromPool(this GameObject obj)
    //{
    //    foreach (var kvp in Pooler.Instance.prefabPools)
    //    {
    //        if (kvp.Value.internalPrefab == obj)
    //        {
    //            return kvp.Value.GetFromPool();
    //        }
    //        else
    //        {
    //            return new PrefabPool(10, obj).GetFromPool();
    //        }
    //    }
    //    var newPool = new PrefabPool(10, obj);
    //    Pooler.Instance.prefabPools.Add(obj.name, newPool);
    //    return newPool.GetFromPool();
    //}

}

public class PoolManager : MonoBehaviour
{
    private static Pooler pooler = Pooler.Instance;

    public static PooledObject<T> Get<T>(T obj) where T: class, IPoolable<T>
    {
        if (pooler.pools.TryGetValue(obj, out var pool))
        {
            return (PooledObject<T>)pool.Get();
        }
        else
        {
            pool = new ObjectPool<T>(10, obj);
            pooler.pools[obj] = pool;
            return (PooledObject<T>)pool.Get();
        }
    }

    public static PooledObject<T> Get<T>(string name) where T: class, IPoolable<T>
    {
        if (pooler.originPrefabMap.TryGetValue(name, out var definedPrefab))
        {
            if (pooler.pools.TryGetValue(definedPrefab, out IPool pool))
            {
                return (PooledObject<T>)pool.Get();
            }
            else throw new InvalidOperationException($"No pool has been mapped to the object {definedPrefab}. There could be an issue with the Pooler as this mapping is done during its initialization.");
        }
        else throw new InvalidOperationException($"No predefined prefab was found in the Pooler with the name: {name}");
    }

    /// <summary>
    /// Functionally identical to <see cref="Get{T}(string)"/> but useful if the reference to the object pulled from the pool is not needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    public static void Spawn<T>(string name) where T: class, IPoolable<T>
    {
        if (pooler.originPrefabMap.TryGetValue(name, out var definedPrefab))
        {
            if (pooler.pools.TryGetValue(definedPrefab, out IPool pool))
            {
                pool.Get();
            }
            else throw new InvalidOperationException($"No pool has been mapped to the object {definedPrefab}. There could be an issue with the Pooler as this mapping is done during its initialization.");
        }
        else throw new InvalidOperationException($"No predefined prefab was found in the Pooler with the name: {name}");
    }

    public static void Return<T>(PooledObject<T> obj) where T : class, IPoolable<T>, new()
    {
        if (pooler.pools.TryGetValue(obj.objectKey, out IPool pool))
        {
            pool.Return(obj);
        }
        else
        {
            throw new InvalidOperationException($"No pool registered for objectKey with reference hash of {obj.objectKey.GetHashCode()}");
        }
    }
}
