using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AssetBundleBuildConfig", menuName = "Create AssetBundleBuildConfig", order = 0)]
public class AssetBundleBuildConfig : ScriptableObject
{
    public List<string> prefabsPath = new List<string>();
    public List<AssetDirectoryConfig> AssetDirectoryPath = new List<AssetDirectoryConfig>();

//    #region  
//    private readonly string AssetsPrefix="Assets";
//    [SerializeField]
//    private string aBBuildConfigPath="";
//    public string ABBuildConfigPath => AssetsPrefix + aBBuildConfigPath;
//    #endregion
    [System.Serializable]
    public struct AssetDirectoryConfig
    {
         public string assetBundleName;
        public string path;
    }
}
