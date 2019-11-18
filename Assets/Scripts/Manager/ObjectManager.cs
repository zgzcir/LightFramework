using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
            ResourceManager.Instance.IncreaseResourceRef(crc);

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


    /// <summary>
    /// 预加载
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="count">个数</param>
    /// <param name="isClear">跳转场景清除</param>
    public void PreLoadObject(string path,int count,bool isClear=false)
    {
        List<GameObject> tempGameObjcets=new List<GameObject>();
        for (int i = 0; i <count; i++)
        {
            GameObject gameObject = InstantiateObject(path, false, isClear);
            tempGameObjcets.Add(gameObject);
        }
        tempGameObjcets.ForEach(g =>
        {
            ReleaseObject(g);
        });
        tempGameObjcets.Clear();
    }
    public GameObject InstantiateObject(string path, bool isSetSceneTrans = false, bool isClear = true)
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

        if (isSetSceneTrans)
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

    public void ReleaseObject(GameObject obj,int maxCacheCount = -1, bool isDestroyPrimitiveCache = false, 
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
            ResourceManager.Instance.ReleaseResource(objectItem, isDestroyPrimitiveCache);
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
                ResourceManager.Instance.DecreaseResourceRef(objectItem);
            }
            else
            {
                ObjectItemsInstanceTempDic.Remove(guid);
                ResourceManager.Instance.ReleaseResource(objectItem, isDestroyPrimitiveCache);
                objectItem.Reset();
                objectItemNativePool.Recycle(objectItem);
            }
        }
    }

    #region async

    public void InstantiateObjectAsync(string path, OnAsyncFinish outerCallBack, LoadResPriority priority,
        bool isSetSceneTrans = false, bool isClear = true, params object[] paramList)
    {
        if (string.IsNullOrEmpty(path)) return;
        uint crc = CRC32.GetCRC32(path);
        ObjectItem objectItem = GetCacheObjectItemFromDic(crc);
        if (objectItem != null)
        {
            if (isSetSceneTrans)
            {
                objectItem.CloneObj.transform.SetParent(SceneTrans, false);
            }

            outerCallBack?.Invoke(path, objectItem.CloneObj, paramList);
            return;
        }

        objectItem = objectItemNativePool.Spawn();
        if (objectItem == null)
        {
            Debug.LogError("null");
        }

        objectItem.Crc = crc;
        objectItem.IsSetSceneParent = isSetSceneTrans;
        objectItem.isClear = isClear;
        objectItem.outerCallBack = outerCallBack;
        objectItem.paramList = paramList;
        //调用ResourceManager异步加载接口

        ResourceManager.Instance.AsyncLoadResource(path, objectItem, (_path, item, plist) =>
        {
            if (item == null) return;
            if (item.PrimitiveAssetItem.AssetObject == null)
            {
#if UNITY_EDITOR
                Debug.LogError("异步资源加载为空" + _path);
#endif
            }
            else
            {
                //实例化
                item.CloneObj = Object.Instantiate(item.PrimitiveAssetItem.AssetObject) as GameObject;
            }

            if (item.CloneObj != null && item.IsSetSceneParent)
            {
                item.CloneObj.transform.SetParent(SceneTrans);
            }

            if (item.outerCallBack != null)
            {
                int guid = item.CloneObj.GetInstanceID();
                if (!ObjectItemsInstanceTempDic.ContainsKey(guid))
                {
                    ObjectItemsInstanceTempDic.Add(guid, item);
                }

                item.outerCallBack?.Invoke(_path, item.CloneObj, plist);
            }
        }, priority);
    }

    #endregion

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