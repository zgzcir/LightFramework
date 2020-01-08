using System.Collections.Generic;
using System.Reflection;
using LightFramework.Base;
using LightFramework.Editor.Config;
using UnityEditor;
using UnityEngine;

namespace LightFramework.Config
{
    [System.Serializable]
    public class RegTypeParsersJson
    {
        public List<RegTypeParserJson> parserList;
    }

    [System.Serializable]
    public class RegTypeParserJson
    {
        public string Type;
        public string ParserName;
    }


    public  static class RegTypeParserManager
    {
        private static Dictionary<string, string> regTypeParserDic;
        public static IRegTypeParser GetRegTypeParser(string type)
        {
            regTypeParserDic = new Dictionary<string, string>();
            TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/LightFramework/Editor/Config/RegTypePasers/parsers.json");
            RegTypeParsersJson jsonObject = JsonUtility.FromJson<RegTypeParsersJson>(ta.text);
            foreach (RegTypeParserJson info in jsonObject.parserList)
            {
                // regTypeParserDic.Add(info.Type, info.ParserName);
                if (info.Type.Equals(type))
                {
                    var ob= ReflectionExtensions.CreateInstance(info.ParserName) as IRegTypeParser;
                    return ob;
                }
            }
            return null;
        }
    }
}