using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    //name path
    public static Dictionary<string, string> fileDirsABDic = new Dictionary<string, string>();

    public static List<string> filterFilesPath = new List<string>();

    public static Dictionary<string, List<string>> filesABDic = new Dictionary<string, List<string>>();

    [MenuItem("Tools/打包")]
    public static void Build()
    {
        fileDirsABDic.Clear();
        filterFilesPath.Clear();
        filesABDic.Clear();

        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(PathDefine.ABConfig);
        abConfig.allFileDirAB.ForEach(dir =>
        {
            if (fileDirsABDic.ContainsKey(dir.abName))
            {
                Debug.LogError("AB配置名字重复,请检查");
            }
            else
            {
                fileDirsABDic.Add(dir.abName, dir.path);
                filterFilesPath.Add(dir.path);
            }
        });


        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.allPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "Prefab:" + path, i * 1.0f / allStr.Length);

            if (!IsContainAllFileAB(path))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDependices = AssetDatabase.GetDependencies(path);
                List<string> allDependicesPath = new List<string>();
                for (int j = 0; j < allDependices.Length; j++)
                {
                    var dependPath = allDependices[j];
                    if (!IsContainAllFileAB(dependPath) && !dependPath.EndsWith(".cs")) ;
                    {
                        filterFilesPath.Add(dependPath);
                        allDependicesPath.Add(dependPath);
                    }
                }

                if (filesABDic.ContainsKey(go.name))
                {
                    Debug.LogError("存在相同名字Prefab:" + go.name);
                }
                else
                {
                    filesABDic.Add(go.name, allDependicesPath);
                }

            }
        }

        foreach (var item in fileDirsABDic)
        {
            SetABName(item.Key, item.Value);
        }

        foreach (var item in filesABDic)
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
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
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

                resPathDic.Add(assetPath, bundleName);
            }
        }

        foreach (var VARIABLE in resPathDic)
        {
            Debug.Log(VARIABLE.Key + ":  " + VARIABLE.Value);
        }

        #endregion

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);
    }

    private static void ClearInvalidAB()
    {
        string[] allbundelsName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo directoryInfo = new DirectoryInfo(PathDefine.BundleTargetPath);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < fileInfos.Length; i++)
        {
            var file = fileInfos[i];
            if (IsContainABName(file.Name, allbundelsName) || file.Name.EndsWith(".meta"))
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
    private static bool IsContainAllFileAB(string path)
    {
        for (int i = 0; i < filterFilesPath.Count; i++)
        {
            if (path == filterFilesPath[i] || path.Contains(filterFilesPath[i]))
            {
                return true;
            }
        }

        return false;
    }
}


public class PathDefine
{
    public static readonly string ABConfig = "Assets/Editor/ABConfig.asset";
    public static readonly string BundleTargetPath = Application.streamingAssetsPath;
}