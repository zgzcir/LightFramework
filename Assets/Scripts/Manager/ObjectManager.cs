using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectManager : Singleton<ObjectManager>
{
    protected Dictionary<Type, object> classPoolDic = new Dictionary<Type, object>();

    //实例对象池
    protected Dictionary<uint, List<ObjectItem>> objectItemsInstancePoolDic = new Dictionary<uint, List<ObjectItem>>();

    //类对象池
    protected ClassObjectPool<ObjectItem> objectItemNativePool;

    protected Dictionary<int, ObjectItem> ObjectItemsInstanceTempDic = new Dictionary<int, ObjectItem>();

    public Transform RecylePoolTrans;
    public Transform SceneTrans;

    public void Init(Transform transRecylePool, Transform sceneTrans)
    {
        objectItemNativePool = GetOrCreateClassPool<ObjectItem>(Capacity.ObjectItem);
        RecylePoolTrans = transRecylePool;
        SceneTrans = sceneTrans;
    }
    protected ObjectItem GetCacheObjectItemFromDic(uint crc)
    {
        List<ObjectItem> objectItems = null;
        if (objectItemsInstancePoolDic.TryGetValue(crc, out objectItems) && objectItems.Count > 0)
        {
            //resourcemanager 引用计数
            var item = objectItems[0];
            objectItems.RemoveAt(0);
            GameObject gameObject = item.CloneObj;
            if (!ReferenceEquals(gameObject, null))
            {
                item.isAlredayRelease = false;
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

    public GameObject InstantiateObject(string path, bool isClear = true, bool SetSceneTrans = false)
    {
        uint crc = CRC32.GetCRC32(path);
        ObjectItem objectItem = GetCacheObjectItemFromDic(crc);
        if (ReferenceEquals(objectItem, null))
        {
            objectItem = objectItemNativePool.Spawn();
            objectItem.Crc = crc;
            objectItem.isClear = isClear;
            objectItem = ResourceManager.Instance.LoadPrimitiveAssetItem(path, objectItem);

            var PrimitiveObject = objectItem.PrimitiveAssetItem.AssetObject;
            if (PrimitiveObject != null)
            {
                objectItem.CloneObj = GameObject.Instantiate(PrimitiveObject) as GameObject;
            }
        }

        if (SetSceneTrans)
        {
            objectItem.CloneObj.transform.SetParent(SceneTrans, false);
        }

        int guid = objectItem.CloneObj.GetInstanceID();
        if (ObjectItemsInstanceTempDic.ContainsKey(guid) == false)
        {
            ObjectItemsInstanceTempDic.Add(guid, objectItem);
        }

        return objectItem.CloneObj;
    }

    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool isDestroyCache = false,
        bool recyleParent = true)
    {
        if (obj == null) return;
        ObjectItem objectItem = null;
        int guid = obj.GetInstanceID();
        if (!ObjectItemsInstanceTempDic.TryGetValue(guid, out objectItem))
        {
            Debug.LogError(obj.name + "创建与回收的id不一致或对象非框架加载");
            return;
        }

        if (objectItem.isAlredayRelease)
        {
            Debug.LogError(obj.name + "该对象已放入对象池");
            return;
        }
#if UNITY_EDITOR
        obj.name += "(Recyle)";
#endif
        if (maxCacheCount == 0)
        {
            ObjectItemsInstanceTempDic.Remove(guid);
            ResourceManager.Instance.ReleaseResource(objectItem, isDestroyCache);
            objectItem.Reset();
            objectItemNativePool.Recycle(objectItem);
        }
        else //回收到对象池
        {
            if (!objectItemsInstancePoolDic.TryGetValue(objectItem.Crc, out List<ObjectItem> objectItems))
            {
                objectItems = new List<ObjectItem>(); //TODO
                objectItemsInstancePoolDic.Add(objectItem.Crc, objectItems);
            }
            if (objectItem.CloneObj)
            {
                if (recyleParent)
                {
                    objectItem.CloneObj.transform.SetParent(RecylePoolTrans);
                }
                else
                {
                    objectItem.CloneObj.SetActive(false);
                }
            }

            if (maxCacheCount < 0 || objectItems.Count < maxCacheCount)
            {
                objectItems.Add(objectItem);
                objectItem.isAlredayRelease = true;
                //resourcemanager 引用计数
            }
            else
            {
                ObjectItemsInstanceTempDic.Remove(guid);
                ResourceManager.Instance.ReleaseResource(objectItem, isDestroyCache);
                objectItem.Reset();
                objectItemNativePool.Recycle(objectItem);
            }
        }
    }

    #region pool

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

    #endregion
}