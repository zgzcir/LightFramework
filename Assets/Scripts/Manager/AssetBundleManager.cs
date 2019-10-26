using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    protected Dictionary<uint, AssetItem> AssetItemDic = new Dictionary<uint, AssetItem>();
    protected Dictionary<uint, AssetBundleItem> AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

    protected ClassObjectPool<AssetBundleItem> AssetBundleItemPool =
        ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(Capacity.AssetBundleItem);

    public bool LoadAssetBundleCofig()
    {
        AssetItemDic.Clear();
        string path = P.GetFullPath(PathDefine.BundleTargetPath, NameDefine.LoadConfigBundle);
        AssetBundle loadConfigBundle = AssetBundle.LoadFromFile(path);
        TextAsset loadConfigBytes = loadConfigBundle.LoadAsset<TextAsset>(NameDefine.LoadConfigAsset);
        if (loadConfigBytes == null)
        {
            Debug.LogError("AssetBundleLoadConfig is not exits");
            return false;
        }

        MemoryStream ms = new MemoryStream(loadConfigBytes.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleLoadConfig loadConfig = (AssetBundleLoadConfig) bf.Deserialize(ms);
        ms.Close();
        for (int i = 0; i < loadConfig.ABList.Count; i++)
        {
            var aBBase = loadConfig.ABList[i];
            AssetItem item = new AssetItem
            {
                crc = aBBase.Crc,
                assetName = aBBase.AssetName,
                assetBundleName = aBBase.AssetBundleName,
                assetDependentBundles = aBBase.AssetDependentBundles
            };
            if (AssetItemDic.ContainsKey(aBBase.Crc))
            {
                Debug.LogError("重复的crc" + item.assetName + "ab包：" + item.assetBundleName);
            }
            else
            {
                AssetItemDic.Add(item.crc, item);
            }
        }

        return true;
    }

    public AssetItem LoadAssetItem(uint crc)
    {
        AssetItem item;
        if (!AssetItemDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError($"{MethodBase.GetCurrentMethod().Name} Error: Can not Find {crc} in ResourceItemDic");
            return null;
        }

        if (item.assetBundle != null)
        {
            return item;
        }

        item.assetBundle = LoadAssetBundle(item.assetBundleName);
        item.assetDependentBundles?.ForEach(name => { LoadAssetBundle(name); });
        return item;
    }

    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if (!AssetBundleItemDic.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string path = P.GetFullPath(PathDefine.BundleTargetPath, name);
            if (File.Exists(path))
            {
                assetBundle = AssetBundle.LoadFromFile(path);
            }

            if (assetBundle == null)
            {
                Debug.LogError($"{MethodBase.GetCurrentMethod().Name} Error: {path} cant load");
            }

            item = AssetBundleItemPool.Spawn();
            item.AssetBundle = assetBundle;
            AssetBundleItemDic.Add(crc, item);
        }

        item.RefCount++;
        return item.AssetBundle;
    }

    public void ReleaseAsset(AssetItem assetItem)
    {
        if (assetItem == null)
            return;
        var dependentBundles = assetItem.assetDependentBundles;
        if (dependentBundles != null && dependentBundles.Count
            > 0)
        {
            for (int i = 0; i < dependentBundles.Count; i++)
            {
                UnLoadAssetBundle(dependentBundles[i]);
            }
        }
        UnLoadAssetBundle(assetItem.assetBundleName);
    }

    private void UnLoadAssetBundle(string name)
    {
        uint crc = CRC32.GetCRC32(name);
        AssetBundleItem item ;
        if (AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(true);
                item.Rest();
                AssetBundleItemPool.Recycle(item);
                AssetBundleItemDic.Remove(crc);
            }
        }
    }

    public AssetItem FindAssetItem(uint crc)
    {
        return AssetItemDic[crc];
    }
}

public class AssetBundleItem
{
    public AssetBundle AssetBundle;
    public int RefCount;

    public void Rest()
    {
        AssetBundle = null;
        RefCount = 0;
    }
}

public class AssetItem
{
    public uint crc;
    public string assetName;
    public string assetBundleName;
    public List<string> assetDependentBundles;
    public AssetBundle assetBundle;
}