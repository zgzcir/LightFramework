using UnityEditor;
using UnityEngine;

namespace LightFramework.Editor.Resource
{
    public class PathDefineEditor
    {
        public static readonly string BundleTargetPath =
            Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget;
        /// <summary>
        /// ConfigEditor
        /// </summary>
        /// <returns></returns>
        public static readonly string XmlUnityPath = "Assets/GameData/Data/Xml";

        public static string XmlFullPath(string name)
        {
            if (name.EndsWith(".xml"))
                return $"{XmlUnityPath}/{name}";
            return $"{XmlUnityPath}/{name}.xml";
        }

        public static string XmlLocalPath()
        {
            return Application.dataPath.Replace("Assets", "") + XmlUnityPath;
        }

        public static readonly string BinaryUnityPath = "Assets/GameData/Data/Binary";

        public static string BinaryFullPath(string name)
        {
            if (name.EndsWith(".xml"))
                name = name.Replace(".xml", "");
            return $"{BinaryUnityPath}/{name}.bytes";
        }

        public static readonly string ExcelLocalPath = Application.dataPath + "/../Data/Excel";

        public static string ExcelFullPath(string name)
        {
            return $"{ExcelLocalPath}/{name}";
        }

        public static readonly string RegLocalPath = Application.dataPath + "/../Data/Reg";

        public static string RegFullPath(string name)
        {
            return $"{RegLocalPath}/{name}.reg.xml";
        }
    }
}