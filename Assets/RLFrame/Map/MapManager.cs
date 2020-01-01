using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : Singleton<MapManager>
{
    public Action SceneLoadingDoneCallBack;
    public Action SceneLoadingEnterCallBack;

    public string CurrentMapName { get; set; }
    public static int LoadingProgrees { get; set; } = 0;

    public bool isSceneLoadingDone { get; set; } = false;

    private MonoBehaviour mono;

    public void Init(MonoBehaviour mono)
    {
        this.mono = mono;
    }

    public void LoadScene(string name)
    {
        LoadingProgrees = 0;
        UIManager.Instance.PopUpWindow(UIDefinition.Loading, paramList: new object[] {name});
        mono.StartCoroutine(LoadSceneAsyncCor(name));
    }

    /// <summary>
    /// 设置场景环境
    /// </summary>
    /// <param name="name"></param>
    public void SetSceneSetting(string name)
    {
    }


//todo 
    IEnumerator LoadSceneAsyncCor(string name)
    {
        SceneLoadingEnterCallBack?.Invoke();
        ClearCache();
        isSceneLoadingDone = false;
        AsyncOperation unloadScene = SceneManager.LoadSceneAsync(SceneDefinition.Empty, LoadSceneMode.Single);
        while (unloadScene != null && !unloadScene.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        LoadingProgrees = 0;
        int targetProgress = 0;
        AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
        if(asyncScene != null && !asyncScene.isDone)
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
            SceneLoadingDoneCallBack?.Invoke();
        }
    }
    private void ClearCache()
    {
        ObjectManager.Instance.ClearCache();
        ResourceManager.Instance.ClearCache();
    }
}