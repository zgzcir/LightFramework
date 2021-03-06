﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LightFramework.Resource
{
    [Serializable]
    public class AssetBundleLoadProfile
    {
        [XmlElement("AssetBundle")] public List<ABBase> ABList { get; set; }
 
 
    }
    [Serializable]
    public class ABBase
    {
        [XmlAttribute("Path")] public string Path { get; set; }
        [XmlAttribute("Crc")] public uint Crc { get; set; }
        [XmlAttribute("ABName")] public string AssetBundleName { get; set; }
        [XmlAttribute("AssetName")] public string AssetName { get; set; }
        [XmlElement("DependentBundles")] public List<string> DependentBundles { get; set; }
    }
}