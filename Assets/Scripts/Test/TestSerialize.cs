using System.Xml.Serialization;
using System.Collections.Generic;

[System.Serializable]
public class TestSerialize
{
    [XmlAttribute("Id")] public int Id { get; set; }
    [XmlAttribute("Name")] public string Name { get; set; }
    [XmlElement("List")]public List<int > List { get; set; }
}