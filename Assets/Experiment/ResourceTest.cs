﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class ResourceTest : MonoBehaviour
{
    void Start()
    {
//        SerializeTest();
//        DeserializeTest();
//        BinarySerializeTest();
//        BinaryDeserializeTest();
//        ReadTestAssets();
LoadAseetBundle();
    }

//    private void Load()
//    {
//        AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/attack");
//        GameObject go = Instantiate(ab.LoadAsset<GameObject>("attack"));
//        Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/Attack.prefab"));
//    }

    private void LoadAseetBundle()
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/loadconfig");
        MemoryStream ms = new MemoryStream(assetBundle.LoadAsset<TextAsset>("AssetBundleLoadConfig").bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleLoadProfile loadProfile = (AssetBundleLoadProfile) bf.Deserialize(ms);
        ms.Close();

        string path = "Assets/GameData/Prefabs/Attack.prefab";
        uint crc = CRC32.GetCRC32(path);
        ABBase abBase = null;
        for (int i = 0; i < loadProfile.ABList.Count; i++)
        {
            if (loadProfile.ABList[i].Crc == crc)
            {
                abBase = loadProfile.ABList[i];
            }
        }

        for (int i = 0; i < abBase.DependentBundles.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.DependentBundles[i]);
        }

        if (abBase != null)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.AssetBundleName);
            GameObject go = Instantiate(ab.LoadAsset<GameObject>(abBase.AssetName));
        }
    }

//    private void SerializeTest()
//    {
//        XmlSerialize(new TestSerialize()
//        {
//            Id = 1,
//            Name = "test",
//            List = new List<int> {1, 2, 3}
//        });
//        XmlDeserialize();
//    }

    void DeserializeTest()
    {
        print(XmlDeserialize().Id + ";" + XmlDeserialize().Name + ";List->" + XmlDeserialize().List.Count);
    }

//    private void BinarySerializeTest()
//    {
//        BinarySerialize(new TestSerialize()
//        {
//            Id = 2,
//            Name = "binaryTest",
//            List = new List<int> {1, 2, 3}
//        });
//    }

//    private void BinaryDeserializeTest()
//    {
//        TestSerialize ts = BinaryDeserialize();
//        print(ts.Id + ";" + ts.Name + ";List->" + ts.List.Count);
//    }

//    private void ReadTestAssets()
//    {
//        AssetsSerialize assetsSerialize = AssetDatabase.LoadAssetAtPath<AssetsSerialize>("Assets/testAssets.asset");
//        print(assetsSerialize.Id + ";" + assetsSerialize.Name + ";List->" + assetsSerialize.List.Count);
//
//    }

//    private void XmlSerialize(TestSerialize ts)
//    {
//        FileStream fs = new FileStream(Application.dataPath + "/testXML.xml", FileMode.Create, FileAccess.ReadWrite,
//            FileShare.ReadWrite);
//        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
//
//        XmlSerializer xml = new XmlSerializer(ts.GetType());
//        xml.Serialize(sw, ts);
//        sw.Close();
//        fs.Close();
//        AssetDatabase.Refresh();
//    }

    private TestSerialize XmlDeserialize()
    {
        FileStream fs = new FileStream(Application.dataPath + "/testXML.xml", FileMode.Open, FileAccess.ReadWrite,
            FileShare.ReadWrite);
        XmlSerializer xml = new XmlSerializer(typeof(TestSerialize));
        TestSerialize ts = (TestSerialize) xml.Deserialize(fs);
        fs.Close();
        return ts;
    }

//    private void BinarySerialize(TestSerialize ts)
//    {
//        FileStream fs = new FileStream(Application.dataPath + "/testBytes.bytes", FileMode.Create, FileAccess.ReadWrite,
//            FileShare.ReadWrite);
//        BinaryFormatter bf = new BinaryFormatter();
//        bf.Serialize(fs, ts);
//        fs.Close();
//        AssetDatabase.Refresh();
//    }

//    private TestSerialize BinaryDeserialize()
//    {
//        TextAsset ta = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/testBytes.bytes");
//        MemoryStream ms = new MemoryStream(ta.bytes);
//        BinaryFormatter bf = new BinaryFormatter();
//        TestSerialize ts = (TestSerialize) bf.Deserialize(ms);
//        ms.Close();
//        return ts;
//    }
}