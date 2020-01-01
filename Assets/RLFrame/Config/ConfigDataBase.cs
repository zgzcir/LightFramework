using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfigDataBase
{
#if UNITY_EDITOR
    public virtual void Construction()
    {
        
    }
#endif

    public virtual void Init()
    {
        
    }
}