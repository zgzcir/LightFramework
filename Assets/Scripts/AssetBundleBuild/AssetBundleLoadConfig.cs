﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class AssetBundleLoadConfig
{
    [XmlElement("ABList")] public List<ABBase> ABList { get; set; }


}
[Serializable]
public class ABBase
{
    [XmlAttribute("Path")] public string Path { get; set; }
    [XmlAttribute("Crc")] public uint Crc { get; set; }
    [XmlAttribute("ABName")] public string AssetBundleName { get; set; }
    [XmlAttribute("AssetName")] public string AssetName { get; set; }
    [XmlElement("AssetDependentBundles")] public List<string> AssetDependentBundles { get; set; }
}