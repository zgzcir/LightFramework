using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : Window
{
    private MenuPanel panel;

    public override void Awake(params object[] paramList)
    {
        panel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(panel.TestButton, OnClickTest);
  
    }
    
    void OnClickTest()
    {
        Debug.Log("Test");
    }

    public override void OnUpdate()
    {
    }
}