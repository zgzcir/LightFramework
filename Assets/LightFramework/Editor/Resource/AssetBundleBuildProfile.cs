using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace LightFramework.Editor.Resource
{
    [CreateAssetMenu(fileName = "AssetBundleBuildProfile", menuName = "Create AssetBundleBuildProfile", order = 0)]
    public class AssetBundleBuildProfile : ScriptableObject
    {
        public List<string> prefabsPath = new List<string>();

        [FormerlySerializedAs("AssetDirectory")]
        public List<AssetDirectoryConfig> AssetDirectories = new List<AssetDirectoryConfig>();

        [FormerlySerializedAs("AssetBundleLoadConfigDirectory")]
        public AssetDirectoryConfig AssetBundleLoadProfileDirectory;

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

    [CustomEditor(typeof(AssetBundleBuildProfile))]
    public class AssetBundleBuildProfileInspector : UnityEditor.Editor
    {
        public List<string> prefabsPath = new List<string>();

        [FormerlySerializedAs("AssetDirectory")]
        public List<AssetBundleBuildProfile.AssetDirectoryConfig> AssetDirectories =
            new List<AssetBundleBuildProfile.AssetDirectoryConfig>();

        [FormerlySerializedAs("AssetBundleLoadConfigDirectory")]
        public AssetBundleBuildProfile.AssetDirectoryConfig AssetBundleLoadProfileDirectory;
    }
}