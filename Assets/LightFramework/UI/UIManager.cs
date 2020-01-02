using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightFramework.Base;
using LightFramework.Resource;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightFramework.UI
{
    public enum UIMessageType
    {
        None,
    }

    public class UIManager : Singleton<UIManager>
    {
        private RectTransform uiRoot;
        private RectTransform wndRoot;
        private Camera uiCamera;
        private EventSystem eventSystem;
        private float canvasRate;


        /// <summary>
        /// ui名字-类型注册字典
        /// </summary>
        private Dictionary<string, Type> RegesiterDic = new Dictionary<string, System.Type>();

        private Dictionary<string, Window> OpenedWindowDic = new Dictionary<string, Window>();


        public void Init(RectTransform uiRoot, RectTransform wndRoot, Camera uiCamera, EventSystem eventSystem)
        {
            this.uiRoot = uiRoot;
            this.wndRoot = wndRoot;
            this.uiCamera = uiCamera;
            this.eventSystem = eventSystem;

            canvasRate = Screen.height / uiCamera.orthographicSize * 2.0f;
        }

        /// <summary>
        /// 整体隐藏 不触发事件
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowOrHideUI(bool isShow)
        {
            if (uiRoot != null)
            {
                uiRoot.gameObject.SetActive(isShow);
            }
        }

        /// <summary>
        /// 设置默认选择对象
        /// </summary>
        /// <param name="gameObject"></param>
        public void SetDefaultSelect(GameObject gameObject)
        {
            if (eventSystem == null)
            {
                eventSystem = EventSystem.current;
            }

            eventSystem.firstSelectedGameObject = gameObject;
        }


        public void Update()
        {
            var windowList = OpenedWindowDic.Values.ToList();
            for (int i = 0; i < windowList.Count; i++)
            {
                windowList[i].OnUpdate();
            }
        }

        /// <summary>
        /// 发送消息给窗口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messageType"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public bool SendMessageToWindow(string name, UIMessageType messageType, params object[] paramList)
        {
            Window window = FindWndByName<Window>(name);
            if (window != null)
            {
                return window.OnMessage(messageType, paramList);
            }

            return false;
        }

        /// <summary>
        /// 注册窗口
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        public void Register<T>(string name) where T : Window
        {
            RegesiterDic[name] = typeof(T);
        }

        public T FindWndByName<T>(string name) where T : Window
        {
            if (OpenedWindowDic.TryGetValue(name, out Window window))
            {
                return window as T;
            }

            return null;
        }

        public Window PopUpWindow(string name, bool isTop = true, params object[] paramList)
        {
            Window window = FindWndByName<Window>(name);
            if (window == null)
            {
                System.Type tp = null;
                if (RegesiterDic.TryGetValue(name, out tp))
                {
                    window = System.Activator.CreateInstance(tp) as Window;
                }
                else
                {
                    Debug.LogError($"找不到{name}窗口对应的脚本");
                    return null;
                }

                //todo optimazation
                GameObject windowObject =
                    ObjectManager.Instance.InstantiateObject(UIDefinition.UIPrefabPathPrefix + name, false, false);
                if (windowObject == null)
                {
                    Debug.LogError($"创建窗口失败:{name}");
                    return null;
                }

                if (!OpenedWindowDic.ContainsKey(name))
                {
                    OpenedWindowDic.Add(name, window);
                }

                window.GameObject = windowObject;
                window.Transform = windowObject.transform;

                window.Awake(paramList);
                window.Transform.SetParent(wndRoot,false);
                window.Name = name;
                ShowWindow(window, isTop, paramList);
            }
            else
            {
                ShowWindow(window, isTop, paramList);
            }

            return window;
        }


        public void CloseWindow(string name, bool isDestroy = false)
        {
            Window window = FindWndByName<Window>(name);
            CloseWindow(window, isDestroy);
        }


        public void CloseWindow(Window window, bool isDestroy = false)
        {
            if (window != null)
            {
                window.OnDisable();
                window.OnClose();
                if (OpenedWindowDic.ContainsKey(window.Name))
                {
                    OpenedWindowDic.Remove(window.Name);
                }

                if (isDestroy)
                {
                    ObjectManager.Instance.ReleaseObject(window.GameObject, 0, true);
                }
                else
                {
                    ObjectManager.Instance.ReleaseObject(window.GameObject, recyleParent: false);
                }

                window.GameObject = null;
                window.Transform = null;
            }
        }

        public void CloseAllWindow()
        {
            OpenedWindowDic.Values.ToList().ForEach(w => { CloseWindow(w); });
        }

        /// <summary>
        /// 关闭所有窗口，打开唯一的窗口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isTop"></param>
        /// <param name="paramList"></param>
        public void UniqueSwitchByName(string name, bool isTop, params object[] paramList)
        {
            CloseAllWindow();
            PopUpWindow(name, isTop, paramList);
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="name"></param>
        public void HideWindow(string name)
        {
            Window window = FindWndByName<Window>(name);
            HideWindow(window);
        }

        public void HideWindow(Window window)
        {
            if (window != null)
            {
                window.GameObject.SetActive(false);
                window.OnDisable();
            }
        }

        public void ShowWindow(string name, bool isTop, params object[] paramList)
        {
            Window window = FindWndByName<Window>(name);
            ShowWindow(window, isTop, paramList);
        }

        public void ShowWindow(Window window, bool isTop, params object[] paramList)
        {
            if (window != null)
            {
                if (window.GameObject != null && !window.GameObject.activeSelf)
                {
                    window.GameObject.SetActive(true);
                    if (isTop)
                    {
                        window.Transform.SetAsLastSibling();
                    }

                    window.OnShow(paramList);
                }
            }
        }
    }
}