using System.Collections.Generic;
using UnityEngine;
using static SerializeOption;

public class ConfigManager : Singleton<ConfigManager>
{
    /// <summary>
    /// 配置表
    /// </summary>
    private readonly Dictionary<string, ConfigDataBase> ConfigDataDic = new Dictionary<string, ConfigDataBase>();

    public T LoadConfigData<T>(string binaryPath) where T : ConfigDataBase
    {
        if (string.IsNullOrEmpty(binaryPath)) return null;

        if (ConfigDataDic.ContainsKey(binaryPath))
        {
            Debug.LogError($"Loaded config {binaryPath} repeatedly");
            return ConfigDataDic[binaryPath] as T;
        }
        T data = null;
        data = BinaryDeserializeRuntime<T>(binaryPath);
#if UNITY_EDITOR
        if (data == null)
        {
            Debug.Log($"Temporary load from xml,Please ensure the binary file{binaryPath} is generated");
            string xmlPath = binaryPath.Replace("Binary", "Xml").Replace(".bytes", "xml");
            data = XmlDeserializeEditor<T>(xmlPath);
        }
#endif

        data?.Init();
        ConfigDataDic.Add(binaryPath, data);
        return data;
    }

    public T FindConfigData<T>(string path) where T: ConfigDataBase
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (ConfigDataDic.TryGetValue(path, out ConfigDataBase data))
        {
            return (T)data;
        }
        else
        {
            data = LoadConfigData<T>(path);
        }
        return (T)data;
    }
}
public class CFG
{
    public const string TableBuff = "Assets/GameData/Data/Binary/BuffConfigData.bytes";
    public const string TablePokemon = "Assets/GameData/Data/Binary/PokemonConfigData.bytes";

}