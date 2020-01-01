using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class BuffConfigData : ConfigDataBase
{
#if UNITY_EDITOR
    public override void Construction()
    {
        Buffs = new List<BuffData>()
        {
            new BuffData()
            {
                Id = 101,
                Name = "闪雷",
                IconPath = "Oui/Liu/Lua/Thunder.buff",
                Description = "雷属性持续伤害，体力逐渐流失",
                Duration = 6.5f,
                Type = BuffType.闪雷,
                Scripts = new List<string>()
                {
                    "测试测试测试",
                    "测试测试测试"
                },
                FXs = new List<BuffFX>()
                {
                    new BuffFX()
                    {
                        Id = 201,
                        Name = "雷暴光环",
                        Path = "Oui/Liu/Lua/Thunder.fx"
                    },
                    new BuffFX()
                    {
                        Id = 202,
                        Name = "雷暴光环",
                        Path = "Oui/Liu/Lua/Thunder.fx"
                    },
                    new BuffFX()
                    {
                        Id = 203,
                        Name = "雷暴光环",
                        Path = "Oui/Liu/Lua/Thunder.fx"
                    }
                }
            }
        };
        DeBuffs = new List<BuffData>()
        {
            new BuffData()
            {
                Id = 102,
                Name = "闪雷",
                IconPath = "Oui/Liu/Lua/Thunder.buff",
                Description = "雷属性持续伤害，体力逐渐流失",
                Duration = 6.5f,
                Type = BuffType.闪雷,
                Scripts = new List<string>()
                {
                    "测试测试测试"
                },
                FXs = new List<BuffFX>()
                {
                    new BuffFX()
                    {
                        Id = 204,
                        Name = "雷暴光环",
                        Path = "Oui/Liu/Lua/Thunder.fx"
                    },
                    new BuffFX()
                    {
                        Id = 205,
                        Name = "雷暴光环",
                        Path = "Oui/Liu/Lua/Thunder.fx"
                    },
                    new BuffFX()
                    {
                        Id = 206,
                        Name = "雷暴光环",
                        Path = "Oui/Liu/Lua/Thunder.fx"
                    }
                }
            }
        };
    }
#endif
    public override void Init()
    {
        BuffDic.Clear();
        foreach (var buff in Buffs)
        {
            BuffDic.Add(buff.Id, buff);
        }
    }

    public BuffData FindBuffById(int id)
    {
        return BuffDic[id];
    }

    [XmlIgnore] public Dictionary<int, BuffData> BuffDic = new Dictionary<int, BuffData>();

    [XmlElement("Buff")] public List<BuffData> Buffs { get; set; }
    [XmlElement("DeBuff")] public List<BuffData> DeBuffs { get; set; }
}

[System.Serializable]
public class BuffData
{
    [XmlAttribute("Id")] public int Id { get; set; }
    [XmlAttribute("Name")] public string Name { get; set; }
    [XmlAttribute("IconPath")] public string IconPath { get; set; }
    [XmlAttribute("Duration")] public float Duration { get; set; }
    [XmlAttribute("Description")] public string Description { get; set; }
    [XmlAttribute("Type")] public BuffType Type { get; set; }
    [XmlElement("Script")] public List<string> Scripts { get; set; }
    [XmlElement("FX")] public List<BuffFX> FXs { get; set; }
}
[System.Serializable]

public class BuffFX
{
    [XmlAttribute("Id")] public int Id { get; set; }
    [XmlAttribute("Name")] public string Name { get; set; }
    [XmlAttribute("Path")] public string Path { get; set; }
}

public enum BuffType
{
    诅咒,
    流血,
    石化,
    闪雷
}