using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using LightFramework.Base;
using UnityEngine;

namespace LightFramework.Resource
{
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        protected Dictionary<uint, AssetItem> AssetItemDic = new Dictionary<uint, AssetItem>();
        protected Dictionary<uint, AssetBundleItem> AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

        protected ClassObjectPool<AssetBundleItem> AssetBundleItemPool =
            ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(Capacity.AssetBundleItem);

        public bool LoadAssetBundleCofig()
        {
            AssetItemDic.Clear();
#if UNITY_EDITOR
            if (!ResourceManager.Instance.IsLoadFromAssetBundle)
                return true;
#endif
            string path = P.GetFullPath(Application.streamingAssetsPath, NameDefine.LoadProfileAB);
            AssetBundle loadConfigBundle = AssetBundle.LoadFromFile(path);
            TextAsset loadConfigBytes = loadConfigBundle.LoadAsset<TextAsset>(NameDefine.LoadProfileAsset);
            if (loadConfigBytes == null)
            {
                Debug.LogError("AssetBundleLoadConfig is not exits");
                return false;
            }

            MemoryStream ms = new MemoryStream(loadConfigBytes.bytes);
            BinaryFormatter bf = new BinaryFormatter();
            AssetBundleLoadProfile loadProfile = (AssetBundleLoadProfile) bf.Deserialize(ms);
            ms.Close();
            for (int i = 0; i < loadProfile.ABList.Count; i++)
            {
                var aBBase = loadProfile.ABList[i];
                AssetItem item = new AssetItem
                {
                    crc = aBBase.Crc,
                    assetName = aBBase.AssetName,
                    assetBundleName = aBBase.AssetBundleName,
                    assetDependentBundles = aBBase.DependentBundles
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

        public AssetItem LoadAssetItemBundle(uint crc)
        {
            AssetItem item;
            //加载配置时生成
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
                string path = P.GetFullPath(Application.streamingAssetsPath, name);
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

        public void ReleaseAssetBundle(AssetItem assetItem)
        {
            if (assetItem == null)
                return;
//        assetItem.AssetObject = null;
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
            AssetBundleItem item;
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
            if (AssetItemDic.TryGetValue(crc, out AssetItem assetItem))
            {
                return assetItem;
            }

#if UNITY_EDITOR
            assetItem = new AssetItem();
            assetItem.crc = crc;
            AssetItemDic.Add(crc, assetItem);
            return assetItem;
#endif
            return null;
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


        public Object AssetObject;
        public int guid;
        public float lastUseTime;
        protected int refCount;

        public int RefCount
        {
            get => refCount;
            set
            {
                refCount = value;
                if (refCount < 0)
                {
                    Debug.LogError("refcount < 0" + refCount + " ," + AssetObject != null
                        ? AssetObject.name
                        : "name is null");
                }
            }
        }


        public bool IsClear = true;
    }
}