using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWindow : Window
{
    private LoadingPanel panel;
    private string loadingSceneName;

    public override void Awake(params object[] paramList)
    {
        panel = GameObject.GetComponent<LoadingPanel>();
        loadingSceneName = (string) paramList[0];
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (panel == null) return;
        panel.Slider.value = MapManager.LoadingProgrees / 100.0f;
        panel.LodingProgress.text = $"{MapManager.LoadingProgrees}%";
        if (MapManager.LoadingProgrees>=100)
        {
            OpenLoadingSceneUI();
        }
    }

    public void OpenLoadingSceneUI()
    {
        if (loadingSceneName == SceneDefinition.Menu)
        {
            UIManager.Instance.PopUpWindow(UIDefinition.Menu);
        }
        UIManager.Instance.CloseWindow(UIDefinition
            .Loading);
    }
}