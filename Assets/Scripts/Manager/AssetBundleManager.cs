using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    protected Dictionary<uint, ResourceItem> ResourceItemDic = new Dictionary<uint, ResourceItem>();

    public bool LoadAssetBundleCofig()
    {
        ResourceItemDic.Clear();
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
            ResourceItem item = new ResourceItem
            {
                crc = aBBase.Crc,
                assetName = aBBase.AssetName,
                assetBundleName = aBBase.AssetBundleName,
                assetDependentBundles = aBBase.AssetDependentBundles
            };
            if (ResourceItemDic.ContainsKey(aBBase.Crc))
            {
                Debug.LogError("重复的crc" + item.assetName + "ab包：" + item.assetBundleName);
            }
            else
            {
                ResourceItemDic.Add(item.crc, item);
            }
        }

        return true;
    }

    public ResourceItem LoadAssetBundleResourceItem(uint crc)
    {
        ResourceItem item;
        if (!ResourceItemDic.TryGetValue(crc, out item) || item == null)
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

        return assetBundle;

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
public class ResourceItem
{
    public uint crc;
    public string assetName;
    public string assetBundleName;
    public List<string> assetDependentBundles;
    public AssetBundle assetBundle;
}