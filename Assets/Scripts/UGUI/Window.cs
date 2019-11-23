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

    /// <summary>
    /// 替换Sprite
    /// </summary>
    /// <param name="path"></param>
    /// <param name="image"></param>
    /// <param name="isSetNativeSize"></param>
    public bool ChangeImageSprite(string path, Image image, bool isSetNativeSize = false)
    {
        if (image == null) return false;
        Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
        if (sp != null)
        {
            if (image.sprite != null)
            {
                image.sprite = null;
            }

            image.sprite = sp;
            if (isSetNativeSize)
            {
                image.SetNativeSize();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 异步替换Sprite
    /// </summary>
    /// <param name="path"></param>
    /// <param name="image"></param>
    /// <param name="isSetNativeSize"></param>
    public void ChangeImageSpriteAsync(string path, Image image, bool isSetNativeSize = false)
    {
        if (image == null) return;
        ResourceManager.Instance.AsyncLoadResource(path, (s, o, list) =>
        {
            if (o != null)
            {
                Sprite sp = o as Sprite;
                Image image = list[0] as Image;
                bool isSetNativeSize = (bool) list[1];
                if (image.sprite != null)
                {
                    image.sprite = null;
                }

                image.sprite = sp;
                if (isSetNativeSize)
                {
                    image.SetNativeSize();
                }
            }
        }, LoadResPriority.RES_MIDDLE, image, isSetNativeSize);
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