using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    //name path
    private static Dictionary<string, string> m_fileDirsABDic = new Dictionary<string, string>();

    private static List<string> m_filterFilesPath = new List<string>();

    private static Dictionary<string, List<string>> m_filesABDic = new Dictionary<string, List<string>>();

    private static List<string> m_filterVaildConfigs = new List<string>();

    [MenuItem("Tools/打包")]
    public static void Build()
    {
        m_filterVaildConfigs.Clear();
        m_fileDirsABDic.Clear();
        m_filterFilesPath.Clear();
        m_filesABDic.Clear();

        ABBuildConfig abBuildConfig = AssetDatabase.LoadAssetAtPath<ABBuildConfig>(PathDefine.ABBuildConfig);
        abBuildConfig.allFileDirAB.ForEach(dir =>
        {
            if (m_fileDirsABDic.ContainsKey(dir.aBName))
            {
                Debug.LogError("AB包配置名字重复,请检查");
            }
            else
            {
                m_fileDirsABDic.Add(dir.aBName, dir.path);

                m_filterFilesPath.Add(dir.path);
                m_filterVaildConfigs.Add(dir.path);
            }
        });

        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abBuildConfig.allPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "Prefab:" + path, i * 1.0f / allStr.Length);
            m_filterVaildConfigs.Add(path);
            if (!IsContainInFileDirAB(path))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDependices = AssetDatabase.GetDependencies(path);
                List<string> allDependicesPath = new List<string>();
                for (int j = 0; j < allDependices.Length; j++)
                {
                    var dependPath = allDependices[j];
                    if (!IsContainInFileDirAB(dependPath) && !dependPath.EndsWith(".cs"))
                    {
                        m_filterFilesPath.Add(dependPath);
                        allDependicesPath.Add(dependPath);
                    }
                }

                if (m_filesABDic.ContainsKey(go.name))
                {
                    Debug.LogError("存在相同名字Prefab:" + go.name);
                }
                else
                {
                    m_filesABDic.Add(go.name, allDependicesPath);
                }
            }
        }

        #region bytesfile

        if (File.Exists(PathDefine.BytesFullPath))
        {
            File.Delete(PathDefine.BytesFullPath);
        }
        File.Create(PathDefine.BytesFullPath);
        
        #endregion


        foreach (var item in m_fileDirsABDic)
        {
            SetABName(item.Key, item.Value);
        }

        foreach (var item in m_filesABDic)
        {
            SetABName(item.Key, item.Value);
        }

        ClearInvalidAB();

        BuildAssetsBundle();

        ClearABName();


        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    private static void SetABName(string name, List<string> pathes)
    {
        pathes.ForEach(p =>
        {
            AssetImporter assetImporter = AssetImporter.GetAtPath(p);
            if (assetImporter == null)
            {
                Debug.LogError("不存在此路径文件/文件夹:" + p);
            }
            else
            {
                assetImporter.assetBundleName = name;
            }
        });
    }

    private static void ClearABName()
    {
        string[] assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < assetBundleNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(assetBundleNames[i], true);
            EditorUtility.DisplayProgressBar("清除AB包名", "name:" + assetBundleNames[i],
                i * 1.0f / assetBundleNames.Length);
        }
    }

    private static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("不存在此路径文件/文件夹:" + path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    private static void BuildAssetsBundle()
    {
        #region 生成配置表

        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        //path name
        Dictionary<string, string> assetPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            var bundleName = allBundles[i];
            string[] assetsPathInBundle = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            for (int j = 0; j < assetsPathInBundle.Length; j++)
            {
                var assetPath = assetsPathInBundle[j];
                if (assetPath.EndsWith(".cs"))
                {
                    continue;
                }

                if (IsValidPath(assetPath))
                {
                    assetPathDic.Add(assetPath, bundleName);
                }
            }
        }

        foreach (var VARIABLE in assetPathDic)
        {
            Debug.Log(VARIABLE.Key + ":  " + VARIABLE.Value);
        }

        WriteData(assetPathDic);

        #endregion


        BuildPipeline.BuildAssetBundles(PathDefine.BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);
    }

    private static void ClearInvalidAB()
    {
        if (!Directory.Exists(PathDefine.BundleTargetPath))
            Directory.CreateDirectory(PathDefine.BundleTargetPath);

        string[] allbundelsName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo directoryInfo = new DirectoryInfo(PathDefine.BundleTargetPath);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < fileInfos.Length; i++)
        {
            var file = fileInfos[i];
//            if (IsContainABName(file.Name, allbundelsName) || file.Name.EndsWith(".meta"))
            if (IsContainABName(file.Name, allbundelsName))
            {
                continue;
            }

            Debug.Log("此AB包已无效：" + file.Name);
            if (File.Exists(file.FullName))
            {
                File.Delete(file.FullName);
            }
        }
    }

    private static bool IsContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name.Equals(strs[i]))
            {
                return true;
            }
        }

        return false;
    }

    //文件夹 其他依赖
    private static bool IsContainInFileDirAB(string path)
    {
        for (int i = 0; i < m_filterFilesPath.Count; i++)
        {
            if (path == m_filterFilesPath[i] || path.Contains(m_filterFilesPath[i]) &&
                path.Replace(m_filterFilesPath[i], "")[0] == '/')
            {
                return true;
            }
        }

        return false;
    }

    private static void WriteData(Dictionary<string, string> assetPathDic)
    {
        AsseBundleLoadConfig asseBundleLoadConfig = new AsseBundleLoadConfig();
        asseBundleLoadConfig.ABList = new List<ABBase>();
        foreach (var item in assetPathDic)
        {
            ABBase aBBase = new ABBase
            {
                Path = item.Key,
                Crc = CRC32.GetCRC32(item.Key),
                ABName = item.Value,
                AssetName = item.Key.Remove(0, item.Key.LastIndexOf('/') + 1),
                AssetDependentBundles = new List<string>()
            };
            string[] assetDependices = AssetDatabase.GetDependencies(aBBase.Path);
            for (int i = 0; i < assetDependices.Length; i++)
            {
                string path = assetDependices[i];
                if (path == aBBase.Path || path.EndsWith(".cs"))
                    continue;
                string aBName;
                if (assetPathDic.TryGetValue(path, out aBName))
                {
                    if (aBName.Equals(item.Value))
                    {
                        continue;
                    }

                    if (!aBBase.AssetDependentBundles.Contains(aBName))
                    {
                        aBBase.AssetDependentBundles.Add(aBName);
                    }
                }
            }

            asseBundleLoadConfig.ABList.Add(aBBase);
        }

        if (File.Exists(PathDefine.XmlPath)) File.Delete(PathDefine.XmlPath);
        FileStream fs = new FileStream(PathDefine.XmlPath, FileMode.Create, FileAccess.ReadWrite,
            FileShare.ReadWrite);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(AsseBundleLoadConfig));
        xmlSerializer.Serialize(fs, asseBundleLoadConfig);
        fs.Close();

        asseBundleLoadConfig.ABList.ForEach(aBBase => { aBBase.Path = ""; });

        FileStream fs2 = new FileStream(PathDefine.BytesRelativePath,
            FileMode.Open, FileAccess.ReadWrite,
            FileShare.Write);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs2, asseBundleLoadConfig);
        fs2.Close();
    }
    private static bool IsValidPath(string path)
    {
        for (int i = 0; i < m_filterVaildConfigs.Count; i++)
        {
            if (path.Contains(m_filterVaildConfigs[i])) return true;
        }

        return false;
    }
}

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
}