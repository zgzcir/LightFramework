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

    public long guid;
    public bool isAlredayRelease = false;

    public void Reset()
    {
        Crc = 0;
        CloneObj = null;
        isClear = true;
        guid = 0;
        PrimitiveAssetItem = null;
        isAlredayRelease = false;
    }
}


public class AsyncLoadAssetParam
{
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
    public OnAsyncObjecFinish callBack;
    public object[] paramList;

    public void Reset()
    {
        callBack = null;
        paramList = null;
    }
}

public delegate void OnAsyncObjecFinish(string path, Object obj, params object[] paramList);

public class ResourceManager : Singleton<ResourceManager>
{
    protected MonoBehaviour startMono;

    //异步加载队列 ?2
    protected List<AsyncLoadAssetParam>[] loadingAssetLists =
        new List<AsyncLoadAssetParam>[(int) LoadResPriority.RES_NUM];

    protected Dictionary<uint, AsyncLoadAssetParam> loadingAssetDic = new Dictionary<uint, AsyncLoadAssetParam>();

    protected ClassObjectPool<AsyncLoadAssetParam> asyncLoadResParamPool =
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
        startMono.StartCoroutine(AsyncLoadCor());
    }

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

    public bool IsLoadFromAssetBundle = false;

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

    //减少引用计数
    public bool ReleaseResource(Object obj, bool isDestroyPrimitiveCache = false)
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
        if (isDestroyPrimitiveCache)
            DestroyAssetItem(item, isDestroyPrimitiveCache);
        return true;
    }

    public bool ReleaseResource(ObjectItem objectItem, bool isDestroyPrimitiveCache = false)
    {
        if (objectItem == null) return false;
        AssetItem assetItem = null;
        foreach (var res in AssetDic.Values)
        {
            if (res.guid == objectItem.CloneObj.GetInstanceID())
            {
                assetItem = res;
            }
        }
        if (assetItem == null)
        {
            Debug.LogError("AssetDic not exits " + objectItem.CloneObj.name + "，可能进行了多次释放");
            return false;
        }
        Object.Destroy(objectItem.CloneObj);
        assetItem.RefCount--;
        if (isDestroyPrimitiveCache)
            DestroyAssetItem(assetItem, isDestroyPrimitiveCache);
        return true;


        return ReleaseResource(objectItem.PrimitiveAssetItem.AssetObject, isDestroyPrimitiveCache);
    }

    public bool ReleaseResource(string path, bool isDestroyPrimitiveCache = false)
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
        DestroyAssetItem(item, isDestroyPrimitiveCache);
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
        if (!destroyCache)
        {
//            unRefAseetItems.InsertToHead(item);
            return;
        }

        if (item == null || item.RefCount > 0)
            return;
        if (!AssetDic.Remove(item.crc))
            return;
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
//        return;
#endif
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

    public void AsyncLoadResource(string path, OnAsyncObjecFinish cb, LoadResPriority priority, uint crc = 0,
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

        AsyncLoadAssetParam para = null;
        if (!loadingAssetDic.TryGetValue(crc, out para))
        {
            para = asyncLoadResParamPool.Spawn();
            para.Crc = crc;
            para.Path = path;
            para.Priority = priority;
            loadingAssetDic.Add(crc, para);
            loadingAssetLists[(int) priority].Add(para);
        }

        AsyncCallBackPack callBackPack = asyncCallBackPackPool.Spawn();
        callBackPack.callBack = cb;
        callBackPack.paramList = paramList;
        para.CallBackPacks.Add(callBackPack);
    }

    IEnumerator AsyncLoadCor()
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
                AssetItem item = null;
#if UNITY_EDITOR
                if (!IsLoadFromAssetBundle)
                {
                    obj = LoadAssetByEditor<Object>(asyncLoadAssetParam.Path);
                    yield return new WaitForSeconds(0.5f);
                    item = AssetBundleManager.Instance.FindAssetItem(asyncLoadAssetParam.Crc);
                }
#endif
                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadAssetItemBundle(asyncLoadAssetParam.Crc);
                    if (item != null && item.assetBundle != null)
                    {
                        AssetBundleRequest request = null;
                        if (asyncLoadAssetParam.isSprite)
                        {
                            request = item.assetBundle.LoadAssetAsync<Sprite>(item.assetName);
                        }
                        else
                        {
                            request = item.assetBundle.LoadAssetAsync(item.assetName);
                        }

                        yield return request;
                        if (request.isDone)
                            obj = request.asset;
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }

                CacheResource(asyncLoadAssetParam.Path, ref item, asyncLoadAssetParam.Crc, obj,
                    asyncCallBackPacks.Count);
                for (int j = 0; j < asyncCallBackPacks.Count; j++)
                {
                    AsyncCallBackPack pack = asyncCallBackPacks[i];
                    pack?.callBack?.Invoke(asyncLoadAssetParam.Path, obj, pack.paramList);
                    pack?.Reset();
                    asyncCallBackPackPool.Recycle(pack);
                }

                asyncCallBackPacks.Clear();
                loadingAssetDic.Remove(asyncLoadAssetParam.Crc);
                asyncLoadAssetParam.Reset();
                asyncLoadResParamPool.Recycle(asyncLoadAssetParam);
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
}

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