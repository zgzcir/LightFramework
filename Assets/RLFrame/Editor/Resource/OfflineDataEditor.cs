using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.Drawing.Vml;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    [MenuItem("Tools/离线数据/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] gameObjects = Selection.gameObjects;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Adding OfflineData", $"正在修改{gameObjects[i].name}....",
                i / 1.0f * gameObjects.Length);
            CreateOfflineData(gameObjects[i]);
        }

        EditorUtility.ClearProgressBar();
    }

    public static void CreateOfflineData(GameObject gameObject)
    {
        OfflineData offlineData = gameObject.GetComponent<OfflineData>();
        if (offlineData == null)
        {
            offlineData = gameObject.AddComponent<OfflineData>();
        }

        offlineData.BindData();
        EditorUtility.SetDirty(gameObject);
        Debug.Log($"修改了{gameObject.name} Prefab");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/离线数据/生成UI离线数据")]
    public static void AssetCreateUIOfflineData()
    {
        GameObject[] gameObjects = Selection.gameObjects;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Adding OfflineData", $"正在修改{gameObjects[i].name}....",
                i / 1.0f * gameObjects.Length);
            CreateOfflineData(gameObjects[i]);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/离线数据/生成所有UI Prefab离线数据")]
    public static void CreateAllUIData()
    {
        string path = ProfileAccessor.GetFrameProfile().UIPrefabPath;
        string[] strs = AssetDatabase.FindAssets("t:Prefab", new[] {path});
        for (int i = 0; i < strs.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(strs[i]);
            EditorUtility.DisplayProgressBar("Adding OfflineData", $"正在扫描{path}---->{prefabPath}修改中...",
                i / 1.0f * strs.Length);
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (gameObject == null) continue;
            CreateUIData(gameObject);
        }

        Debug.Log("UI离线数据全部生成完毕");
        EditorUtility.ClearProgressBar();
    }

    public static void CreateUIData(GameObject gameObject)
    {
        gameObject.layer = LayerMask.NameToLayer("UI");

        UIOfflineData uiOfflineData = gameObject.GetComponent<UIOfflineData>();
        if (uiOfflineData == null)
        {
            uiOfflineData = gameObject.AddComponent<UIOfflineData>();
        }

        uiOfflineData.BindData();
        EditorUtility.SetDirty(gameObject);
        Debug.Log($"修改了UI{gameObject.name}Prefab");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("Tools/离线数据/生成Effect离线数据")]
    public static void AssetCreateEffectOfflineData()
    {
        GameObject[] gameObjects = Selection.gameObjects;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Adding OfflineData", $"正在修改{gameObjects[i].name}....",
                i / 1.0f * gameObjects.Length);
            CreateEffectData(gameObjects[i]);
        }

        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("Tools/离线数据/生成所有Effect Prefab离线数据")]
    public static void CreateAllEffectData()
    {
        string path = ProfileAccessor.GetFrameProfile().EffectPrefabPath;
        string[] strs = AssetDatabase.FindAssets("t:Prefab", new[] {path});
        for (int i = 0; i < strs.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(strs[i]);
            EditorUtility.DisplayProgressBar("Adding OfflineData", $"正在扫描{path}---->{prefabPath}修改中...",
                i / 1.0f * strs.Length);
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (gameObject == null) continue;
            CreateEffectData(gameObject);
        }

        Debug.Log("Effect离线数据全部生成完毕");
        EditorUtility.ClearProgressBar();
    }
    public static void CreateEffectData(GameObject gameObject)
    {
        EffectOfflineData effectOfflineData = gameObject.GetComponent<EffectOfflineData>();
        if (effectOfflineData == null)
        {
            effectOfflineData = gameObject.AddComponent<EffectOfflineData>();
        }
        effectOfflineData.BindData();
        EditorUtility.SetDirty(gameObject);
        Debug.Log($"修改了Effect{gameObject.name}Prefab");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}