using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using LightFramework.Config;
using UnityEngine;

[System.Serializable]
public class PokemonConfigData : ConfigDataBase
{
#if UNITY_EDITOR
    
    public override void Construction()
    {
        Pokemons = new List<PokemonData>()
        {
            new PokemonData()
            {
                Id = 1001,
                Name = "憨批龟龟",
                PrefabPath = "Assets/GameData/Prefabs/Attack.prefab",
                Level = 10,
                Rare = 3,
                Height = 8
            },
            new PokemonData()
            {
                Id = 7,
                Name = "杰尼龟",
                PrefabPath = "Assets/GameData/Prefabs/Attack.prefab",
                Level = 10,
                Rare = 3,
                Height = 8
            }
        };
        EvolvedPokemons=
            new List<PokemonData>()
            {
                new PokemonData()
                {
                    Id = 1001,
                    Name = "水箭龟龟",
                    PrefabPath = "Assets/GameData/Prefabs/Attack.prefab",
                    Level = 10,
                    Rare = 3,
                    Height = 8
                },
                new PokemonData()
                {
                    Id = 7,
                    Name = "杰尼龟",
                    PrefabPath = "Assets/GameData/Prefabs/Attack.prefab",
                    Level = 10,
                    Rare = 3,
                    Height = 8
                }
            };
    }
#endif

    public override void Init()
    {
        PokemonDic.Clear();
        foreach (var pokemon in Pokemons)
        {
            if (PokemonDic.ContainsKey(pokemon.Id))
            {
                Debug.LogError($"Repeated {pokemon.Id} ,{pokemon.Name}");
            }
            else
            {
                PokemonDic.Add(pokemon.Id, pokemon);
            }
        }

        base.Init();
    }

    public PokemonData FindPokemonById(int id)
    {
        return PokemonDic[id];
    }

    [XmlIgnore] public Dictionary<int, PokemonData> PokemonDic = new Dictionary<int, PokemonData>();
    [XmlElement("Pokemon")] public List<PokemonData> Pokemons { get; set; }
    [XmlElement("EvolvedPokemon")] public List<PokemonData> EvolvedPokemons { get; set; }
}

[System.Serializable]
public class PokemonData
{
    [XmlAttribute("Id")] public int Id { get; set; }
    [XmlAttribute("Name")] public string Name { get; set; }
    [XmlAttribute("PrefabPath")] public string PrefabPath { get; set; }
    [XmlAttribute("Level")] public int Level { get; set; }
    [XmlAttribute("Rare")] public int Rare { get; set; }
    [XmlAttribute("Height")] public float Height { get; set; }
}