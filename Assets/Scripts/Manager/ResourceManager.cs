using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public enum LoadResPriority
{
    RES_HIGH = 0,
    RES_MIDDLE,
    RES_LOW,
    RES_NUM
}

public class ObjectItem
{
    public uint Crc;

    public AssetItem PrimitiveAssetItem;
    public GameObject CloneObj;

    public bool isClear = true;

    public long Guid;
    public bool isAlreadyRelease = false;

    #region async

    public bool IsSetSceneParent = false;
    public OnAsyncFinish outerCallBack;
    public object[] paramList;

    #endregion

    public OfflineData OfflineData;

    public void Reset()
    {
        Crc = 0;
        CloneObj = null;
        isClear = true;
        Guid = 0;
        PrimitiveAssetItem = null;
        isAlreadyRelease = false;
        IsSetSceneParent = false;
        outerCallBack = null;
        paramList = null;
        OfflineData = null;
    }
}


public class AsyncLoadAssetParam
{
    /// <summary>
    /// 一段时间内多个请求异步加载的回调包
    /// </summary>
    public List<AsyncCallBackPack> CallBackPacks = new List<AsyncCallBackPack>();

    public uint Crc;
    public string Path;
    public LoadResPriority Priority = LoadResPriority.RES_LOW;
    public bool isSprite = false;

    public void Reset()
    {
        CallBackPacks.Clear();
        Crc = 0;
        Path = "";
        isSprite = false;
        Priority = LoadResPriority.RES_LOW;
    }
}

public class AsyncCallBackPack
{
    public OnAsyncFinish Finish;
    public object[] paramList;

//->finalFinish
    public OnAsyncPrimitiveAssetFinish PrimitiveAssetFinishInner;
    public ObjectItem ObjectItem;

    public void Reset()
    {
        Finish = null;
        PrimitiveAssetFinishInner = null;
        ObjectItem = null;
        paramList = null;
    }
}


public class ResourceManager : Singleton<ResourceManager>
{
    protected MonoBehaviour startMono;

    //异步加载队列  优先级划分
    protected List<AsyncLoadAssetParam>[] loadingAssetLists =
        new List<AsyncLoadAssetParam>[(int) LoadResPriority.RES_NUM];

    /// <summary>
    /// 按crc聚合异步加载的参数
    /// </summary>
    protected Dictionary<uint, AsyncLoadAssetParam>
        asyncLoadAssetParamDic = new Dictionary<uint, AsyncLoadAssetParam>();

    protected ClassObjectPool<AsyncLoadAssetParam> asyncLoadResParamPackPool =
        new ClassObjectPool<AsyncLoadAssetParam>(Capacity.AsyncLoadAssetParam);

    protected ClassObjectPool<AsyncCallBackPack> asyncCallBackPackPool =
        new ClassObjectPool<AsyncCallBackPack>(Capacity.AsyncCallBack);

    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int) LoadResPriority.RES_NUM; i++)
        {
            loadingAssetLists[i] = new List<AsyncLoadAssetParam>();
        }

        startMono = mono;
        startMono.StartCoroutine(AsyncLoadCycleCoroutine());
    }

    /// <summary>
    /// 创建全局唯一标识符
    /// </summary>
    /// <returns></returns>
    public long CreateGuid()
    {
        return guid++;
    }

    protected long guid = 0;

    /// <summary>
    ///跳场景
    /// </summary>
    public void ClearCache()
    {
        List<AssetItem> items = new List<AssetItem>();
        foreach (var item in AssetDic.Values)
        {
            if (item.IsClear) items.Add(item);
        }

        items.ForEach(i => { DestroyAssetItem(i, true); });
        items.Clear();
    }

    /// <summary>
    /// 取消异步加载asset,先清空回收回调列表，在从列表和字典里移除后回收
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool CancelAsyncLoad(ObjectItem objectItem)
    {
        AsyncLoadAssetParam asyncParam = null;
        if (asyncLoadAssetParamDic.TryGetValue(objectItem.Crc, out asyncParam) &&
            loadingAssetLists[(int) asyncParam.Priority].Contains(asyncParam))
        {
            for (int i = asyncParam.CallBackPacks.Count; i > 0; i--)
            {
                AsyncCallBackPack asyncCallbackPack = asyncParam.CallBackPacks[i];
                //判定objecitem取消
                if (asyncCallbackPack != null && asyncCallbackPack.ObjectItem == objectItem)
                {
                    asyncCallbackPack.Reset();
                    asyncCallBackPackPool.Recycle(asyncCallbackPack);
                    asyncParam.CallBackPacks.Remove(asyncCallbackPack);
                }
            }

            if (asyncParam.CallBackPacks.Count <= 0)
            {
                loadingAssetLists[(int) asyncParam.Priority].Remove(asyncParam);
                asyncLoadAssetParamDic.Remove(objectItem.Crc);
                asyncParam.Reset();
                asyncLoadResParamPackPool.Recycle(asyncParam);
                return true;
            }
        }

        return false;
    }

    public int IncreaseResourceRef(ObjectItem objectItem, int refCount = 1)
    {
        return objectItem == null ? IncreaseResourceRef(objectItem.Crc, refCount) : 0;
    }

    public int IncreaseResourceRef(uint crc, int refCount = 1)
    {
        AssetItem assetItem = null;
        if (!AssetDic.TryGetValue(crc, out assetItem))
            return 0;
        assetItem.RefCount += refCount;
        assetItem.lastUseTime = Time.realtimeSinceStartup;
        return assetItem.RefCount;
    }

    public int DecreaseResourceRef(ObjectItem objectItem, int refCount = 1)
    {
        return objectItem == null ? DecreaseResourceRef(objectItem.Crc, refCount) : 0;
    }

    public int DecreaseResourceRef(uint crc, int refCount = 1)
    {
        AssetItem assetItem = null;
        if (!AssetDic.TryGetValue(crc, out assetItem))
            return 0;
        assetItem.RefCount -= refCount;
        return assetItem.RefCount;
    }

    public void PreLoadRes(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        uint crc = CRC32.GetCRC32(path);
        AssetItem item = GetCacheAssetItem(crc, 0);
        if (item != null)
        {
//            item.IsClear = false;
            return;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!IsLoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindAssetItem(crc);
            if (item.AssetObject != null)
                obj = item.AssetObject;
            else
                obj = LoadAssetByEditor<Object>(path);
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadAssetItemBundle(crc);
            if (item != null && item.assetBundle != null)
            {
                if (item.AssetObject != null)
                    obj = item.AssetObject;
                else
                    obj = item.assetBundle.LoadAsset<Object>(item.assetName);
            }
        }

        CacheResource(path, ref item, crc, obj);
        //跳场景
        item.IsClear = false;
        ReleaseResource(path);
    }

    public bool IsLoadFromAssetBundle = true;

    protected CMapList<AssetItem> unRefAseetItems = new CMapList<AssetItem>();

    public Dictionary<uint, AssetItem> AssetDic { get; set; } = new Dictionary<uint, AssetItem>();

    public T LoadResource<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        uint crc = CRC32.GetCRC32(path);
        AssetItem item = GetCacheAssetItem(crc);
        if (item != null)
        {
            return item.AssetObject as T;
        }

        T obj = null;
#if UNITY_EDITOR
        if (!IsLoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindAssetItem(crc);
            if (item.AssetObject != null)
                obj = item.AssetObject as T;
            else
                obj = LoadAssetByEditor<T>(path);
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadAssetItemBundle(crc);
            if (item != null && item.assetBundle != null)
            {
                if (item.AssetObject != null)
                    obj = item.AssetObject as T;
                else
                    obj = item.assetBundle.LoadAsset<T>(item.assetName);
            }
        }

        CacheResource(path, ref item, crc, obj);
        return obj;
    }

    //给ObjectManager的接口
    public ObjectItem LoadPrimitiveAssetItem(string path, ObjectItem objectItem)
    {
        if (objectItem == null)
            return null;
        uint crc = objectItem.Crc == 0 ? CRC32.GetCRC32(path) : objectItem.Crc;

        AssetItem assetItem = GetCacheAssetItem(crc);
        if (assetItem != null)
        {
            objectItem.PrimitiveAssetItem = assetItem;
            return objectItem;
        }

        Object obj = null;
#if UNITY_EDITOR
        if (!IsLoadFromAssetBundle)
        {
            assetItem = AssetBundleManager.Instance.FindAssetItem(crc);
            if (assetItem.AssetObject != null)
                obj = assetItem.AssetObject;
            else
                obj = LoadAssetByEditor<Object>(path);
        }
#endif
        if (obj == null)
        {
            assetItem = AssetBundleManager.Instance.LoadAssetItemBundle(crc);
            if (assetItem != null && assetItem.assetBundle != null)
            {
                if (assetItem.AssetObject != null)
                    obj = assetItem.AssetObject as Object;
                else
                    obj = assetItem.assetBundle.LoadAsset<Object>(assetItem.assetName);
            }
        }

        assetItem.IsClear = objectItem.isClear;
        CacheResource(path, ref assetItem, crc, obj);
        objectItem.PrimitiveAssetItem = assetItem;

        return objectItem;
    }

    /// <summary>
    /// 供ObjectManager使用
    /// </summary>
    /// <param name="objectItem"></param>
    /// <param name="isDestroyCache"></param>
    /// <returns></returns>
    public bool ReleaseObjectResource(ObjectItem objectItem, bool isDestroyCache = false)
    {
        if (objectItem == null) return false;
        //todo unnecessary 
        Object.Destroy(objectItem.CloneObj);
        objectItem.PrimitiveAssetItem.RefCount--;
        if (isDestroyCache)
            DestroyAssetItem(objectItem.PrimitiveAssetItem, isDestroyCache);
        return true;
    }

    //out direct
    public bool ReleaseResource(Object obj, bool isDestroyCache = false)
    {
        if (obj == null) return false;
        AssetItem item = null;
        foreach (var res in AssetDic.Values)
        {
            if (res.guid == obj.GetInstanceID())
            {
                item = res;
            }
        }

        if (item == null)
        {
            Debug.LogError("AssetDic not exits " + obj.name + "，可能进行了多次释放");
            return false;
        }

        item.RefCount--;
        if (isDestroyCache)
            DestroyAssetItem(item, isDestroyCache);
        return true;
    }


    public bool ReleaseResource(string path, bool isDestroyCache = false)
    {
        if (string.IsNullOrEmpty(path)) return false;
        uint crc = CRC32.GetCRC32(path);
        AssetItem item = null;
        if (!AssetDic.TryGetValue(crc, out item))
        {
            Debug.LogError("AssetDic not exits " + path + "，可能进行了多次释放");
            return false;
        }

        item.RefCount--;
        DestroyAssetItem(item, isDestroyCache);
        return true;
    }

    private void CacheResource(string path, ref AssetItem item, uint crc, Object obj, int addRefCount = 1)
    {
        WashOut();
        if (item == null)
        {
            Debug.LogError("Resource item is null,path:" + path);
        }

        if (obj == null)
        {
            Debug.LogError("Resource Load Fail,path:" + path);
        }

        item.AssetObject = obj;
        item.guid = obj.GetInstanceID();
        item.lastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addRefCount;
        AssetItem oldItem;
        if (AssetDic.TryGetValue(item.crc, out oldItem))
        {
            AssetDic[crc] = item;
        }
        else
        {
            AssetDic.Add(crc, item);
        }
    }

    private void WashOut()
    {
//        {
//            if (unRefAseetItems.Size() <= 0)
//                break;
//            AssetItem item = unRefAseetItems.Back();
//            DestroyAssetItem(item);
//            unRefAseetItems.Pop();
//        }
    }

//字典去除 卸载bundle
    private void DestroyAssetItem(AssetItem item, bool destroyCache = false)
    {
//        if (!destroyCache)
//        {
////            unRefAseetItems.InsertToHead(item);
//            return;
//        }

        if (item == null || item.RefCount > 0)
            return;
        if (!AssetDic.Remove(item.crc))
            return;
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
//        return;
#endif
        //清除对象池中其他同crc的object
        ObjectManager.Instance.ClearPoolObject(item.crc);
        AssetBundleManager.Instance.ReleaseAsset(item);
//        item.AssetObject = null;
    }
#if UNITY_EDITOR
    protected T LoadAssetByEditor<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
    private AssetItem GetCacheAssetItem(uint crc, int refCount = 1)
    {
        if (AssetDic.TryGetValue(crc, out var item))
        {
            item.RefCount += refCount;
            item.lastUseTime = Time.realtimeSinceStartup;
//            if (item.RefCount <= 1)
//            {
//                unRefAseetItems.Remove(item);
//            }
        }

        return item;
    }

    public void AsyncLoadResource(string path, OnAsyncFinish cb, LoadResPriority priority, params object[] paramList
    )
    {
        AsyncLoadResource(path, cb, priority, 0, paramList);
    }

    public void AsyncLoadResource(string path, OnAsyncFinish cb, LoadResPriority priority, uint crc = 0,
        params object[] paramList)
    {
        if (crc == 0)
            crc = CRC32.GetCRC32(path);

        AssetItem item = GetCacheAssetItem(crc);
        if (item != null)
        {
            cb?.Invoke(path, item.AssetObject, paramList);
            return;
        }

        AsyncLoadAssetParam asyncLoadAssetParam = null;
        if (!asyncLoadAssetParamDic.TryGetValue(crc, out asyncLoadAssetParam))
        {
            asyncLoadAssetParam = asyncLoadResParamPackPool.Spawn();
            asyncLoadAssetParam.Crc = crc;
            asyncLoadAssetParam.Path = path;
            asyncLoadAssetParam.Priority = priority;
            asyncLoadAssetParamDic.Add(crc, asyncLoadAssetParam);
            loadingAssetLists[(int) priority].Add(asyncLoadAssetParam);
        }

        AsyncCallBackPack callBackPack = asyncCallBackPackPool.Spawn();
        callBackPack.Finish = cb;
        callBackPack.paramList = paramList;
        asyncLoadAssetParam.CallBackPacks.Add(callBackPack);
    }

    IEnumerator AsyncLoadCycleCoroutine()
    {
        List<AsyncCallBackPack> asyncCallBackPacks = null;
        long lastYieldTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool isHaveYield = false;
            for (int i = 0; i < (int) LoadResPriority.RES_NUM; i++)
            {
                List<AsyncLoadAssetParam> loadingList = loadingAssetLists[i];
                if (loadingList.Count <= 0)
                    continue;
                AsyncLoadAssetParam asyncLoadAssetParam = loadingList[0];
                loadingList.RemoveAt(0);
                asyncCallBackPacks = asyncLoadAssetParam.CallBackPacks;

                Object obj = null;
                AssetItem assetItem = null;
#if UNITY_EDITOR
                if (!IsLoadFromAssetBundle)
                {
                    obj = LoadAssetByEditor<Object>(asyncLoadAssetParam.Path);
                    yield return new WaitForSeconds(0.5f);
                    assetItem = AssetBundleManager.Instance.FindAssetItem(asyncLoadAssetParam.Crc);
                }
#endif
                if (obj == null)
                {
                    assetItem = AssetBundleManager.Instance.LoadAssetItemBundle(asyncLoadAssetParam.Crc);
                    if (assetItem != null && assetItem.assetBundle != null)
                    {
                        AssetBundleRequest request = null;
                        if (asyncLoadAssetParam.isSprite)
                        {
                            request = assetItem.assetBundle.LoadAssetAsync<Sprite>(assetItem.assetName);
                        }
                        else
                        {
                            request = assetItem.assetBundle.LoadAssetAsync(assetItem.assetName);
                        }

                        yield return request;
                        if (request.isDone)
                            obj = request.asset;
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }

                CacheResource(asyncLoadAssetParam.Path, ref assetItem, asyncLoadAssetParam.Crc, obj,
                    asyncCallBackPacks.Count);
                for (int j = 0; j < asyncCallBackPacks.Count; j++)
                {
                    AsyncCallBackPack callBackPack = asyncCallBackPacks[i];
                    if (callBackPack != null || callBackPack.ObjectItem != null)
                    {
                        callBackPack.ObjectItem
                            .PrimitiveAssetItem = assetItem;
                        callBackPack.PrimitiveAssetFinishInner?.Invoke(asyncLoadAssetParam.Path,
                            callBackPack.ObjectItem, callBackPack.ObjectItem.paramList);
                        //**
                        callBackPack.PrimitiveAssetFinishInner = null;
                        callBackPack.ObjectItem = null;
                    }

                    callBackPack?.Finish?.Invoke(asyncLoadAssetParam.Path, obj, callBackPack.paramList);
                    callBackPack?.Reset();
                    asyncCallBackPackPool.Recycle(callBackPack);
                }

                asyncCallBackPacks.Clear();
                asyncLoadAssetParamDic.Remove(asyncLoadAssetParam.Crc);
                asyncLoadAssetParam.Reset();
                asyncLoadResParamPackPool.Recycle(asyncLoadAssetParam);
                if (System.DateTime.Now.Ticks - lastYieldTime > TimeOut.AsyncLoad)
                {
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;
                    isHaveYield = true;
                }
            }

            if (!isHaveYield || System.DateTime.Now.Ticks - lastYieldTime > TimeOut.AsyncLoad)
            {
                yield return null;
                lastYieldTime = System.DateTime.Now.Ticks;
            }
        }
    }

    public void AsyncLoadResource(string path, ObjectItem objectItem, OnAsyncPrimitiveAssetFinish innerCallBack,
        LoadResPriority priority)
    {
        AssetItem assetItem = GetCacheAssetItem(objectItem.Crc);
        if (assetItem != null)
        {
            objectItem.PrimitiveAssetItem = assetItem;
            innerCallBack?.Invoke(path, objectItem);
            return;
        }

        AsyncLoadAssetParam asyncLoadAssetParam = null;
        if (!asyncLoadAssetParamDic.TryGetValue(objectItem.Crc, out asyncLoadAssetParam))
        {
            asyncLoadAssetParam = asyncLoadResParamPackPool.Spawn();
            asyncLoadAssetParam.Crc = objectItem.Crc;
            asyncLoadAssetParam.Path = path;
            asyncLoadAssetParam.Priority = priority;
            asyncLoadAssetParamDic.Add(objectItem.Crc, asyncLoadAssetParam);
            loadingAssetLists[(int) priority].Add(asyncLoadAssetParam);
        }

        AsyncCallBackPack callBackPack = asyncCallBackPackPool.Spawn();
        callBackPack.PrimitiveAssetFinishInner = innerCallBack;
        callBackPack.ObjectItem = objectItem;
        asyncLoadAssetParam.CallBackPacks.Add(callBackPack);
    }
}

public delegate void OnAsyncFinish(string path, Object obj, params object[] paramList);

public delegate void OnAsyncPrimitiveAssetFinish(string path, ObjectItem objectItem, params object[] paramList);

#region list

public class DoubleLinkedListNode<T> where T : class
{
    public DoubleLinkedListNode<T> prev;

    public DoubleLinkedListNode<T> next;

    public T t = null;
}

public class DoubleLinkedList<T> where T : class
{
    public DoubleLinkedListNode<T> Head;
    public DoubleLinkedListNode<T> Tail;


    protected ClassObjectPool<DoubleLinkedListNode<T>> DoubleLinkNodePool =
        ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(Capacity.DoubleLinkedListNode);

    protected int count = 0;
    public int Count => count;

    public DoubleLinkedListNode<T> AddToHeader(T t)
    {
        DoubleLinkedListNode<T> pList = DoubleLinkNodePool.Spawn();
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToHeader(pList);
    }

    public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }

        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }

        count++;
        return pNode;
    }

    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        DoubleLinkedListNode<T> pList = DoubleLinkNodePool.Spawn();
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToTail(pList);
    }

    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null)
        {
            return null;
        }

        pNode.next = null;
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }

        count++;
        return pNode;
    }

    public void Remove(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null) return;

//        if (pNode.prev != null)
//        {
//            if (pNode == Tail)
//            {
//                Tail = pNode.prev;
//                Tail.next = null;
//            }
//            else if (pNode.next != null)
//            {
//                pNode.prev.next = pNode.next;
//                pNode.next.prev = pNode.prev;
//            }
//        }
//        else
//        {
//            Head = pNode.next;
//            pNode.next.prev = null;
//        }

        if (pNode == Head)
        {
            Head = pNode.next;
        }

        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }

        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }

        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }

        pNode.next = pNode.prev = null;
        pNode.t = null;
        DoubleLinkNodePool.Recycle(pNode);
        count--;
    }

    public void MoveToHead(DoubleLinkedListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
        {
            return;
        }

        if (pNode.prev == null && pNode.next == null)
        {
            return;
        }

        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }

        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }

        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }

        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;
        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

public class CMapList<T> where T : class, new()
{
    private DoubleLinkedList<T> doubleLinkedList = new DoubleLinkedList<T>();
    private Dictionary<T, DoubleLinkedListNode<T>> findMap = new Dictionary<T, DoubleLinkedListNode<T>>();

    ~CMapList()
    {
        Clear();
    }

    public void Clear()
    {
        while (doubleLinkedList.Tail != null)
        {
            Remove(doubleLinkedList.Tail.t);
        }
    }

    public void InsertToHead(T t)
    {
        if (findMap.TryGetValue(t, out var node))
        {
            doubleLinkedList.AddToHeader(node);
            return;
        }

        doubleLinkedList.AddToHeader(t);
        findMap.Add(t, doubleLinkedList.Head);
    }

    public void Pop()
    {
        if (doubleLinkedList.Tail != null)
        {
            Remove(doubleLinkedList.Tail.t);
        }
    }

    public void Remove(T t)
    {
        if (!findMap.TryGetValue(t, out var node))
        {
            return;
        }

        doubleLinkedList.Remove(node);
        findMap.Remove(t);
    }

    public T Back()
    {
        return doubleLinkedList.Tail.t;
    }

    public int Size()
    {
        return findMap.Count;
    }

    public bool Find(T t)
    {
        if (!findMap.TryGetValue(t, out var node))
            return false;
        return true;
    }

    public bool Refresh(T t)
    {
        if (!findMap.TryGetValue(t, out var node))
            return false;
        doubleLinkedList.MoveToHead(node);
        return true;
    }
}

#endregion