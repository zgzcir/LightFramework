using UnityEngine;

public static class PathDefine
{
    public static readonly string ABBuildConfig = "Assets/Editor/ABConfig.asset";
    public static readonly string BundleTargetPath = Application.streamingAssetsPath;

    public static readonly string XmlPath = Application.dataPath + "/AssetBundleLoadConfig.xml";
    private static readonly string BytesPath = "/GameData/Data/AssetBundleData/AssetBundleLoadConfig.bytes";

    public static readonly string BytesRelativePath =
        "Assets" + BytesPath;
    public static readonly string BytesFullPath =
        Application.dataPath + BytesPath;

    public static readonly string UIPrefabPath = "";

}

public static class NameDefine
{
    public static readonly string LoadConfigBundle = "loadconfig";
    public static readonly string LoadConfigAsset = "AssetBundleLoadConfig";
}

public static class Capacity
{
    public static readonly int AssetBundleItem = 500;
    public static readonly int DoubleLinkedListNode= 500;
    public static readonly int AsyncLoadAssetParam= 50;
    public static readonly int AsyncCallBack= 100;
    public static readonly int ObjectItem = 1000;
}

public static class TimeOut
{
    public static readonly int AsyncLoad = 200000;
 
}
public static class P
{
    public static string GetFullPath(string pathPrefix,string name)
    {
        return pathPrefix + '/' + name;
    }     
}

