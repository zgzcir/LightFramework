using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ResourceLoad : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        AssetBundle ab=    AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/attack");
        GameObject go = Instantiate(ab.LoadAsset<GameObject>("attack"));
        Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/Attack.prefab"));
        
    }
    
    
    
}