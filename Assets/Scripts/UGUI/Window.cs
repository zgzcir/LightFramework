using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window
{
    public string Name { get; set; }
    public GameObject GameObject { get; set; }
    public Transform Transform { get; set; }

    protected List<Button> buttons;
    protected List<Toggle> toggles;

    public virtual void Awake(params object[] paramList)
    {
        
    }

    public virtual void OnShow(params object[] paramList)
    {
    }

    public virtual void OnDisable()
    {
                
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnClose()
    {
    }

    public virtual bool OnMessage(UIMessageType messageType, params object[] paramList)
    {
        return true;
    }
}