using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Set of hooks used to interact with the lifetime of a pooled object. You can trust these hooks exist and are initialized from the moment your Monobehaviour exists
/// </summary>
public sealed class LifetimeHooks : MonoBehaviour
{
    private PooledObject<GameObjectPoolWrapper> pooledObject;
    public UnityEvent onLeavingPool = new UnityEvent();
    public UnityEvent onReleaseToPool = new UnityEvent();

    public void Initialize(PooledObject<GameObjectPoolWrapper> obj)
    {
        pooledObject = obj;
        obj.leavingPool.AddListener(LeftPool);
    }

    public void Release()
    {
        onReleaseToPool.Invoke();
        pooledObject.Dispose();
    }

    private void LeftPool()
    {
        onLeavingPool.Invoke();
    }

    public void TimedRelease(float timeInSeconds)
    {
        StartCoroutine(TempTimer(timeInSeconds));
    }

    //TODO replace this with an omnicat timer later
    IEnumerator TempTimer(float time)
    {
        yield return new WaitForSeconds(time);
        onReleaseToPool.Invoke();
        pooledObject.Dispose();
    }
}
