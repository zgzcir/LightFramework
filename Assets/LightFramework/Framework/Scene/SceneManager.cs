using System;
using System.Collections;
using System.Collections.Generic;
using LightFramework.Base;
using LightFramework.Resource;
using LightFramework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager>
{
    // public Action SceneLoadingDoneCallBack;
    // public Action SceneLoadingEnterCallBack;

    public string CurrentMapName { get; set; }
    public static int LoadingProgrees { get; set; } = 0;

    public bool isSceneLoadingDone { get; set; } = false;

    private MonoBehaviour mono;

    public void Init(MonoBehaviour mono)
    {
        this.mono = mono;
    }

    public void LoadScene(string name, UnityAction callBack=null)
    {
        LoadingProgrees = 0;
        UIManager.Instance.PopUpWindow(UIDefinition.Loading, paramList: new object[] {name});
        mono.StartCoroutine(LoadSceneAsyncCor(name, callBack));
    }

    /// <summary>
    /// 设置场景环境
    /// </summary>
    /// <param name="name"></param>
    public void SetSceneSetting(string name)
    {
    }


//todo 
    IEnumerator LoadSceneAsyncCor(string name, UnityAction callBack=null)
    {
        ClearCache();
        isSceneLoadingDone = false;
        AsyncOperation unloadScene =
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneDefinition.Empty, LoadSceneMode.Single);
        while (unloadScene != null && !unloadScene.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        LoadingProgrees = 0;
        int targetProgress = 0;
        AsyncOperation asyncScene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
        if (asyncScene != null && !asyncScene.isDone)
        {
            asyncScene.allowSceneActivation = false;
            while (asyncScene.progress < 0.9f)
            {
                targetProgress = (int) asyncScene.progress * 100;
                yield return new WaitForEndOfFrame();
                if (LoadingProgrees < targetProgress)
                {
                    ++LoadingProgrees;
                    yield return new WaitForEndOfFrame();
                }
            }

            CurrentMapName = name;
            SetSceneSetting(name);
            targetProgress = 100;
            while (LoadingProgrees < targetProgress - 2)
            {
                ++LoadingProgrees;
                yield return new WaitForEndOfFrame();
            }
            LoadingProgrees = 100;
            asyncScene.allowSceneActivation = true;
            isSceneLoadingDone = true;
            callBack?.Invoke();
            UIManager.Instance.CloseWindow(UIDefinition.Loading);
        }
    }

    private void ClearCache()
    {
        ObjectManager.Instance.ClearCache();
        ResourceManager.Instance.ClearCache();
    }
}