using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ABConfig", menuName = "Create ABConfig", order = 0)]
public class ABBuildConfig : ScriptableObject
{
    public List<string> allPrefabPath = new List<string>();
    public List<FileDirAB> allFileDirAB = new List<FileDirAB>();

//    #region 
//    private readonly string AssetsPrefix="Assets";
//    [SerializeField]
//    private string aBBuildConfigPath="";
//    public string ABBuildConfigPath => AssetsPrefix + aBBuildConfigPath;
//    #endregion
    [System.Serializable]
    public struct FileDirAB
    {
        [FormerlySerializedAs("abName")] public string aBName;
        public string path;
    }
}