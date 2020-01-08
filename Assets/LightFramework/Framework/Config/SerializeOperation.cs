using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using LightFramework.Resource;
using UnityEngine;
using Object = System.Object;

namespace LightFramework.Config
{
    public class SerializeOperation
    {
        #region Xml

        public static bool XmlSerialize(string savePath, object obj)
        {
            try
            {
                using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {
//                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
//                    namespaces.Add(String.Empty, String.Empty);
                        XmlSerializer xs = new XmlSerializer(obj.GetType());
//                    xs.Serialize(sw, obj, namespaces);
                        xs.Serialize(sw, obj);
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"XmlSerialize Error, {obj.GetType()},{e}");
                throw;
            }
        }


        public static T XmlDeserializeEditor<T>(string path) where T : class
        {
            T t = null;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    t = (T) xmlSerializer.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"XmlDeserialize Error :{e} with {path}");
                throw;
            }

            return t;
        }
        public static System.Object XmlDeserializeEditor(string path,Type type)
        {
            Object obj = null;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(type);
                    obj =  xmlSerializer.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"XmlDeserialize Error :{e} with {path}");
                throw;
            }

            return obj;
        }


        public T XmlDeserializeRuntime<T>(string path) where T : class
        {
            T t = default(T);
            TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"XmlDeserialize Error : can not load from {path}");
                return null;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(textAsset.bytes))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    t = (T) xmlSerializer.Deserialize(ms);
                }

                ResourceManager.Instance.ReleaseResource(path, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"XmlDeserialize Error :{e} ");
                throw;
            }

            return t;
        }

        #endregion
        #region bytes

        public static bool BinarySerialize(string savePath, object obj)
        {
            try
            {
                using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fs, obj);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"BinarySerialize Error, {obj.GetType()},{e}");
                throw;
            }
        }

        public static T BinaryDeserializeRuntime<T>(string path) where T : class
        {
            T t = default(T);
            TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"BinaryDeserializeRuntime Error : can not load from {path}");
                return null;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(textAsset.bytes))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    t = (T) binaryFormatter.Deserialize(ms);
                }

                ResourceManager.Instance.ReleaseResource(path, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"BinaryDeserializeRuntime Error :{e} ");
                throw;
            }

            return t;
        }

        #endregion
    }
}