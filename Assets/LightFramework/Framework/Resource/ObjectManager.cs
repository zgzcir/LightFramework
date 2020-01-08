using System;
using System.Collections.Generic;
using System.Linq;
using LightFramework.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LightFramework.Resource
{
    public partial class ObjectManager : Singleton<ObjectManager>
    {
        protected Dictionary<Type, object> classPoolDic = new Dictionary<Type, object>();
 
        //实例对象池 主池 crc
        protected Dictionary<uint, List<ObjectItem>> objectItemsInstancePoolDic = new Dictionary<uint, List<ObjectItem>>();
    
        //类对象池
        protected ClassObjectPool<ObjectItem> objectItemNativePool;
     
        /// <summary>
        /// 通过cloneobj guid索引 管理所有实例化过的object
        /// </summary>
        protected Dictionary<long, ObjectItem> ObjectItemsInstanceTempDic = new Dictionary<long, ObjectItem>();

        /// <summary>
        /// 根据guid储存正在加载的object
        /// </summary>
        protected Dictionary<long, ObjectItem> asyncLoadingObjectDic = new Dictionary<long, ObjectItem>();

        public Transform RecylePoolTrans;
        public Transform SceneTrans;

        public void Init(Transform transRecylePool, Transform sceneTrans)
        {
            objectItemNativePool = GetOrCreateClassPool<ObjectItem>(Capacity.ObjectItem);
            RecylePoolTrans = transRecylePool;
            SceneTrans = sceneTrans;
        }

        /// <summary>
        /// 清空对象池，保留primitive
        /// </summary>
        public void ClearCache()
        {
            List<uint> crcs = new List<uint>();
            foreach (uint crc in objectItemsInstancePoolDic.Keys)
            {
                var objectItems = objectItemsInstancePoolDic[crc];
                objectItems.ToList().ForEach(o =>
                {
                    if (o.CloneObj != null && o.isClear)
                    {
                        Object.Destroy(o.CloneObj);
                        objectItems.Remove(o);
                        ObjectItemsInstanceTempDic.Remove(o.Guid);
                        o.Reset();
                        objectItemNativePool.Recycle(o);
                    }
                });
                if (objectItems.Count <= 0)
                {
                    crcs.Add(crc);
                }
            }
            for (int i = 0; i < crcs.Count; i++)
            {
                uint crc = crcs[i];
                if (objectItemsInstancePoolDic.ContainsKey(crc))
                {
                    objectItemsInstancePoolDic.Remove(crc);
                }
            }

            crcs.Clear();
        }

        /// <summary>
        /// 清除某资源在对象池中所有对象,保留primitive|供resourcemanager在清除缓存时使用
        /// </summary>
        /// <param name="crc"></param>
        public void ClearPoolObject(uint crc)
        {
            if (!objectItemsInstancePoolDic.TryGetValue(crc, out List<ObjectItem> objectItems))
                return;
            objectItems.ToList().ForEach(o =>
            {
                if (o.isClear)
                {
                    objectItems.Remove(o);
                    long guid = o.Guid;
                    //todo unnecessary 
                    Object.Destroy(o.CloneObj);
                    ObjectItemsInstanceTempDic.Remove(guid);
                    o.Reset();
                    objectItemNativePool.Recycle(o);
                }
            });
            if (objectItems.Count <= 0)
            {
                objectItemsInstancePoolDic.Remove(crc);
            }
        }

        protected ObjectItem GetCacheObjectItemFromPoolDic(uint crc)
        {
            List<ObjectItem> objectItems = null;
            if (objectItemsInstancePoolDic.TryGetValue(crc, out objectItems) && objectItems.Count > 0)
            {
                ResourceManager.Instance.IncreaseResourceRef(crc);

                var item = objectItems[0];
                objectItems.RemoveAt(0);
                GameObject gameObject = item.CloneObj;
                if (gameObject != null)
                {
                    if (item.OfflineData != null)
                    {
                        item.OfflineData.ResetProp();
                    }

                    item.isAlreadyRelease = false;
#if UNITY_EDITOR
                    if (gameObject.name.EndsWith("(Recycle)"))
                    {
                        gameObject.name = gameObject.name.Replace("(Recycle)", "");
                    }
#endif
                }

                return item;
            }

            return null;
        }

        /// <summary>
        /// 取消异步加载object
        /// </summary>
        /// <param name="guid"></param>
        public void CancelAsyncLoad(long guid)
        {
            if (asyncLoadingObjectDic.TryGetValue(guid, out ObjectItem objectItem) &&
                ResourceManager.Instance.CancelAsyncLoad(objectItem))
            {
                asyncLoadingObjectDic.Remove(guid);
                objectItem.Reset();
                objectItemNativePool.Recycle(objectItem);
            }
        }


        /// <summary>
        /// 是否该脚本创建
        /// </summary>
        /// <returns></returns>
        public bool IsFrameCreat(GameObject gameObject)
        {
            ObjectItem objectItem = ObjectItemsInstanceTempDic[gameObject.GetInstanceID()];
            return objectItem != null;
        }


        public void PreLoadObject(string path, int count = 1, bool isClear = false)
        {
            List<GameObject> tempGameObjcets = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject gameObject = InstantiateObject(path, false, isClear);
                tempGameObjcets.Add(gameObject);
            }

            tempGameObjcets.ForEach(g => { ReleaseObject(g); });
            tempGameObjcets.Clear();
        }

        public GameObject InstantiateObject(string path, bool isShowDefaultTrans = false, bool isClear = true)
        {
            uint crc = CRC32.GetCRC32(path);
            ObjectItem objectItem = GetCacheObjectItemFromPoolDic(crc);
            if (ReferenceEquals(objectItem, null))
            {
                objectItem = objectItemNativePool.Spawn();
                objectItem.Crc = crc;
                objectItem.isClear = isClear;
                objectItem = ResourceManager.Instance.LoadPrimitiveAssetItem(path, objectItem);

                var PrimitiveAssetObject = objectItem.PrimitiveAssetItem.AssetObject;
                if (PrimitiveAssetObject != null)
                {
                    objectItem.CloneObj = GameObject.Instantiate(PrimitiveAssetObject) as GameObject;
                    objectItem.OfflineData = objectItem.CloneObj.GetComponent<OfflineData.OfflineData>();
                }
            }

            if (isShowDefaultTrans)
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

        public void CompleteReleaseObject(GameObject obj)
        {
            ReleaseObject(obj,0,true);
        }
        /// <summary>
        /// 缓存数不包括预加载的
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="maxCacheCount"></param>
        /// <param name="isDestroyPrimitiveCache"></param>
        /// <param name="recyleParent"></param>
        public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool isDestroyPrimitiveCache = false,
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

            if (objectItem.isAlreadyRelease)
            {
                Debug.LogError(obj.name + "该对象已放入对象池");
                return;
            }
#if UNITY_EDITOR
            obj.name += "(Recycle)";
#endif
            if (maxCacheCount == 0)
            {
                ObjectItemsInstanceTempDic.Remove(guid);
                ResourceManager.Instance.ReleaseObjectResource(objectItem, isDestroyPrimitiveCache);
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
                //无限缓存
                if (maxCacheCount < 0 || objectItems.Count < maxCacheCount)
                {
                    objectItems.Add(objectItem);
                    objectItem.isAlreadyRelease = true;
                    ResourceManager.Instance.DecreaseResourceRef(objectItem);
                }
                else
                {
                    ObjectItemsInstanceTempDic.Remove(guid);
                    ResourceManager.Instance.ReleaseObjectResource(objectItem, isDestroyPrimitiveCache);
                    objectItem.Reset();
                    objectItemNativePool.Recycle(objectItem);
                }
            }
        }

        #region async
        /// <summary>
        /// 是否正在异步加载
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool IsInAsyncLoading(long guid)
        {
            return asyncLoadingObjectDic.ContainsKey(guid);
        }

        public long AsyncInstantiateObject(string path, OnAsyncFinish outerCallBack, LoadResPriority priority,
            bool isSetSceneTrans = false, bool isClear = true, params object[] paramList)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            uint crc = CRC32.GetCRC32(path);
            ObjectItem objectItem = GetCacheObjectItemFromPoolDic(crc);
            if (objectItem != null)
            {
                if (isSetSceneTrans)
                {
                    objectItem.CloneObj.transform.SetParent(SceneTrans, false);
                }
                outerCallBack?.Invoke(path, objectItem.CloneObj, paramList);
                return objectItem.Guid;
            }

            objectItem = objectItemNativePool.Spawn();
            if (objectItem == null)
            {
                Debug.LogError("null");
            }

            long guid = ResourceManager.Instance.CreateGuid();
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
                    objectItem.OfflineData = item.CloneObj.GetComponent<OfflineData.OfflineData>();
                }

//加载完成移除
                if (asyncLoadingObjectDic.ContainsKey(item.Guid))
                    asyncLoadingObjectDic.Remove(item.Guid);
                if (item.CloneObj != null && item.IsSetSceneParent)
                {
                    item.CloneObj.transform.SetParent(SceneTrans);
                }

                if (item.outerCallBack != null)
                {
                    if (item.CloneObj != null)
                    {
                        int _guid = item.CloneObj.GetInstanceID();
                        if (!ObjectItemsInstanceTempDic.ContainsKey(_guid))
                        {
                            ObjectItemsInstanceTempDic.Add(_guid, item);
                        }
                    }

                    item.outerCallBack?.Invoke(_path, item.CloneObj, plist);
                }
            }, priority);
            return guid;
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
}