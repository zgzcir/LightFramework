using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
        RemoveAllButtonListener();
        RemoveAllToggleListener();

        buttons.Clear();
        toggles.Clear();
    }

    public virtual bool OnMessage(UIMessageType messageType, params object[] paramList)
    {
        return true;
    }


    #region events

    public void RemoveAllButtonListener()
    {
        buttons.ForEach(b => b.onClick.RemoveAllListeners());
    }

    public void RemoveAllToggleListener()
    {
        toggles.ForEach(t => t.onValueChanged.RemoveAllListeners());
    }

    public void AddButtonClickListener(Button button, UnityAction unityAction)
    {
        if (button != null)
        {
            if (!buttons.Contains(button))
            {
                buttons.Add(button);
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(unityAction);
            button.onClick.AddListener(PlayButtonSound);
        }
    }

    protected virtual void PlayButtonSound()
    {
    }

    public void AddToggleListener(Toggle toggle, UnityAction<bool> unityAction)
    {
        if (toggle != null)
        {
            if (!toggles.Contains(toggle))
            {
                toggles.Add(toggle);
            }

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(unityAction);
            toggle.onValueChanged.AddListener(PlayToggleSound);
        }
    }

    public virtual void PlayToggleSound(bool isOn)
    {
    }

    #endregion
}