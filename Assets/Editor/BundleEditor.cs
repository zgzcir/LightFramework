using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    [MenuItem("Tools/打包")]
    public static void Build()
    {
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
        Debug.Log("Build AssetBundle Done");
    }
}