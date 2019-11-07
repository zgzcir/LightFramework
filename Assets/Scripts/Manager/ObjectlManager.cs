using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectlManager : Singleton<ObjectlManager>
{
    protected Dictionary<Type, object> classPoolDic = new Dictionary<Type, object>();

    //实例对象池
    protected Dictionary<uint, List<ObjectItem>> objectItemsDic = new Dictionary<uint, List<ObjectItem>>();

    //类对象池
    protected ClassObjectPool<ObjectItem> objectItemNativePool =
        ObjectlManager.Instance.GetOrCreateClassPool<ObjectItem>(Capacity.ObjectItem);
    
    
    public Transform RecylePoolTrans;
    public Transform SceneTrans;
    
    public void Init(Transform transRecylePool,Transform sceneTrans)
    {
       RecylePoolTrans = transRecylePool;
       SceneTrans = sceneTrans;
    }

    protected ObjectItem GetObjectItemFromDic(uint crc)
    {
        List<ObjectItem> objectItems = null;
        if (objectItemsDic.TryGetValue(crc, out objectItems) && objectItems.Count > 0)
        {
            var item = objectItems[0];
            objectItems.RemoveAt(0);
            GameObject gameObject = item.CloneObj;
            if (!ReferenceEquals(gameObject, null))
            {
#if UNITY_EDITOR
                if (gameObject.name.EndsWith("(Recyle)"))
                {
                    gameObject.name.Replace("(Recyle)", "");
                }
#endif
            }

            return item;
        }

        return null;
    }

    public GameObject InstantiateObject(string path, bool isClear = true,bool SetSceneTrans=false)
    {
        uint crc = CRC32.GetCRC32(path);
        ObjectItem objectItem = GetObjectItemFromDic(crc);
        if (ReferenceEquals(objectItem, null))
        {
            objectItem = objectItemNativePool.Spawn();
            objectItem.Crc = crc;
            objectItem.isClear = isClear;
            objectItem = ResourceManager.Instance.LoadPrimitiveAssetItem(path,objectItem);
            
            var PrimitiveObject = objectItem.PrimitiveAssetItem.AssetObject;
            if (PrimitiveObject != null)
            {
                objectItem.CloneObj = GameObject.Instantiate(PrimitiveObject) as GameObject;
            }
        }
        if (SetSceneTrans)
        {
            objectItem.CloneObj.transform.SetParent(SceneTrans,false);
        }
        return objectItem.CloneObj;
    }
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