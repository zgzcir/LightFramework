using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABConfig",menuName = "Create ABConfig",order = 0)]
public class ABConfig : ScriptableObject
{
    public List<string> allPrefabPath = new List<string>();
    public List<FileDirAB> allFileDirAB = new List<FileDirAB>();

    [System.Serializable]
    public struct FileDirAB
    {
        public string abName;
        public string path;
    }
}
