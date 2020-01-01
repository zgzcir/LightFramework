using UnityEditor;
using UnityEngine;

// public static class PathDefine
// {
//     public static readonly string ABBuildProfile = "Assets/RLFrame.Editor/Editor/Resource/AssetBundleBuildProfile.asset";
//
//     public static readonly string AssetBundlePath =
//         Application.streamingAssetsPath;
//     
//     public static readonly string XmlPath = Application.dataPath + "/AssetBundleLoadConfig.xml";
//
//     #region bytes
//     private static readonly string BytesPath = "/GameData/Data/AssetBundleData/AssetBundleLoadConfig.bytes";
//     public static readonly string BytesRelativePath =
//         "Assets" + BytesPath;
//     public static readonly string BytesFullPath =
//         Application.dataPath + BytesPath;
//
//     #endregion
//     public static readonly string UIPrefabPath = "Assets/GameData/Prefabs/UGUI";
//     public static readonly string EffectPrefabPath = "Assets/GameData/Prefabs/Effects";
// }

public static class NameDefine
{
    //加载配置二进制文件ab包
    public static  string LoadProfileAB = "loadconfig";
    //加载配置二进制文件asset名
    public static  string LoadProfileAsset = "AssetBundleLoadConfig";
    public static void SetLoadProfileAB(string name)
    {
        LoadProfileAB = name;
    }
    public static void SetLoadProfileAsset(string name)
    {
        LoadProfileAsset = name;
    }
    
}

public static class Capacity
{
    public const int AssetBundleItem = 500;
    public const int DoubleLinkedListNode = 500;
    public const int AsyncLoadAssetParam = 50;
    public const int AsyncCallBack = 100;
    public const int ObjectItem = 1000;

    /// <summary>
    /// 最大asset缓存个数
    /// </summary>
    public const int MaxCacheCount = 500;
} 

public static class TimeOut
{
    public static readonly int AsyncLoad = 100000;
}

public static class P
{
    public static string GetFullPath(string pathPrefix, string name)
    {
        return pathPrefix + '/' + name;
    }
}