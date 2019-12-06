using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T instance;
    public static T Instance => instance;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T) this;
        }
        else
        {
            Debug.LogError($"Duplicates Instance {this.GetType()}appear");
        }
    }
}  