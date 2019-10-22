using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ResourceTest : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
       SerializeTest();
      DeserializeTest();
    }

    private void Load()
    {
        AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/attack");
        GameObject go = Instantiate(ab.LoadAsset<GameObject>("attack"));
        Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/Attack.prefab"));
    }

    private void SerializeTest()
    {
        XmlSerialize(new TestSerialize()
        {
            Id = 1,
            Name = "test",
            List = new List<int> {1, 2, 3}
        });
        XmlDeserialize();
    }

void    DeserializeTest()
    {
        print(XmlDeserialize().Id+";"+XmlDeserialize().Name+";List->"+XmlDeserialize().List.Capacity);
    }
    private void XmlSerialize(TestSerialize ts)
    {
        FileStream fs = new FileStream(Application.dataPath + "/test.xml", FileMode.Create, FileAccess.ReadWrite,
            FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
        
        XmlSerializer xml = new XmlSerializer(ts.GetType());
        xml.Serialize(sw, ts);
        sw.Close();
        fs.Close();
    }

    private TestSerialize XmlDeserialize()
    {
        FileStream fs=new FileStream(Application.dataPath+"/test.xml",FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite);
        XmlSerializer xml=new XmlSerializer(typeof(TestSerialize));
        TestSerialize ts =(TestSerialize)xml.Deserialize(fs);
        fs.Close();
        return ts;
    }
}