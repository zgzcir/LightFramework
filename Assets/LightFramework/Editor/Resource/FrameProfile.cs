using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace LightFramework.Editor.Resource
{
    [CreateAssetMenu(fileName = "FrameProfile", menuName = "Create FrameProfile", order = 1)]
    public class FrameProfile : ScriptableObject
    {
#if UNITY_EDITOR
        private string dataPath;

        private void OnEnable()
        {
            dataPath = Application.dataPath.Replace("Assets", "");
        }

        #region resource

        [Header("AssetBundle")]
        public string ABBuildProfilePath = "Assets/RLFrame.Editor/Editor/Resource/AssetBundleBuildProfile.asset";

        [SerializeField] private string aBLoadXmlPath = "Assets/AssetBundleLoadProfile.xml";

        [SerializeField]
        private string aBLoadBytesPath = "Assets/GameData/Data/AssetBundleData/AssetBundleLoadProfile.bytes";

        public string ABLoadXmlPath => dataPath + aBLoadXmlPath;
        public string ABLoadBytesPath => dataPath + aBLoadBytesPath;

        [SerializeField, Tooltip("assetbundle构建位置"), Space(3)]
        private string bundleTargetPath = "../Assetbundle/";

        public string BundleTargetPath =>
            dataPath + "Assets/" + bundleTargetPath + "/" + EditorUserBuildSettings.activeBuildTarget;

        [SerializeField, Tooltip("打包进项目的assetbundle")]
        private string bundleBuildInPath = "Assets/StreamingAssets";

        public string BundleBuildInPath => dataPath + bundleBuildInPath;

        #endregion

        [Header("OfflineData")] [Space(10)]

        #region offlinedata

        public string UIPrefabPath = "Assets/GameData/Prefabs/UGUI";

        public string EffectPrefabPath = "Assets/GameData/Prefabs/Effects";

        #endregion

        [Header("Build")] [Space(10)]

        #region build

        public string AppName = "Game";

        [FormerlySerializedAs("appPathAndroid")] [SerializeField]
        private string appPathAndroidPath = "../Build/Android";

        [FormerlySerializedAs("appPathIOS")] [SerializeField]
        private string appPathIOSPath = "../Build/IOS";

        [FormerlySerializedAs("appPathWindows")] [SerializeField]
        private string appPathWindowsPath = "../Build/Windows";

        public string AppPathAndroid => dataPath + appPathAndroidPath + "/";
        public string AppPathIOS => dataPath + appPathIOSPath + "/";
        public string AppPathWindows => dataPath + appPathWindowsPath + "/";

        #endregion

        #region config

        #region xml

        [Header("Config")] [Space(10)] [SerializeField]
        private string configXmlPath = "Assets/GameData/Data/Xml";

        public string ConfigXmlFullPath(string name)
        {
            if (name.EndsWith(".xml"))
                return $"{configXmlPath}/{name}";
            return $"{configXmlPath}/{name}.xml";
        }

        public string ConfigXmlLocalPath => dataPath + configXmlPath;

        #endregion

        #region binary

        [SerializeField] private string configBinaryPath = "Assets/GameData/Data/Binary";

        public string ConfigBinaryFullPath(string name)
        {
            if (name.EndsWith(".xml"))
                name = name.Replace(".xml", "");
            return $"{configBinaryPath}/{name}.bytes";
        }

        public string ConfigBinaryPath => dataPath + configBinaryPath;

        #endregion


        #region Excel

        [SerializeField] [Space(3)] private string excelPath = "../Data/Excel";

        public string ExcelLocalPath => Application.dataPath + "/" + excelPath;

        public string ExcelFullPath(string name)
        {
            return $"{ExcelLocalPath}/{name}";
        }

        [SerializeField] private string regPath = "../Data/Reg";
        public string RegLocalPath => Application.dataPath + "/" + regPath;

        public string RegFullPath(string name)
        {
            return $"{RegLocalPath}/{name}.reg.xml";
        }

        #endregion

        #endregion

#endif
    }

    public static class ProfileAccessor
    {
        private const string frameProfilePath = "Assets/RLFrame/Editor/Resource/AssetBundleBuildProfile.asset";

        public static FrameProfile GetFrameProfile()
        {
            return AssetDatabase.LoadAssetAtPath<FrameProfile>(frameProfilePath);
        }
    }

// [CustomEditor(typeof(FrameProfile))]
// public class FrameProfileInspector : Editor
// {
//     private void OnEnable()
//     {
//         UIPrefabPath = serializedObject.FindProperty("UIPrefabPath");
//         EffectPrefabPath = serializedObject.FindProperty("EffectPrefabPath");
//     }
//
//     public override void OnInspectorGUI()
//     {
//         serializedObject.Update();
//         EditorGUILayout.PropertyField(UIPrefabPath,new GUIContent("UI离线数据"));
//         GUILayout.Space(5);
//         serializedObject.ApplyModifiedProperties();
//     }
//
//     #region offlinedata
//
//     public SerializedProperty UIPrefabPath;
//
//     public SerializedProperty EffectPrefabPath;
//
//     #endregion
// }
}