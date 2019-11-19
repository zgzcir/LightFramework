using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    [MenuItem("Assets/生成离线数据")]
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
}