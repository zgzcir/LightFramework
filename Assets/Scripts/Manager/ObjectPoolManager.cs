using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    protected Dictionary<Type, object> classPoolDic = new Dictionary<Type, object>();

    public ClassObjectPool<T> GetOrCreateClassPool<T>(int capacity) where T : class, new()
    {
        Type type = typeof(T);
        object outObj;
        if (!classPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(capacity);
            classPoolDic.Add(typeof(T), newPool);
            outObj = newPool;
        }
        return outObj as ClassObjectPool<T>;
    }

}