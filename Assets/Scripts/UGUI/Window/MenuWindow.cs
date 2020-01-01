using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class MenuWindow : Window
{
    private MenuPanel panel;

    public override void Awake(params object[] paramList)
    {
        panel = GameObject.GetComponent<MenuPanel>();
        AddButtonClickListener(panel.TestButton, OnClickTest);
//        TestChangeImageSpriteAsync("Assets/GameData/imgs/ID_369.bmp",panel.img1,LoadResPriority.RES_LOW);
        TestChangeImageSpriteAsync1("Assets/GameData/imgs/-7s28Q5-9epgXrZ7iT3cS1hx-w6.png", panel.img1,
            LoadResPriority.RES_LOW);
        TestChangeImageSpriteAsync2("Assets/GameData/imgs/Snipaste_2019-12-07_12-59-13.png", panel.img2,
                                                 LoadResPriority.RES_HIGH);
        TestChangeImageSpriteAsync2("Assets/GameData/imgs/Snipaste_2019-12-08_16-40-01.png", panel.img3,
            LoadResPriority.RES_HIGH);
        LoadBuffConfiogData();
    }

    #region test


    void LoadBuffConfiogData()
    {
     var buff=   ConfigManager.Instance.FindConfigData<BuffConfigData>(CFG.TableBuff);
     Debug.Log(buff);
    }

    public void TestChangeImageSpriteAsync1(string path, Image image, LoadResPriority priority,
             bool isSetNativeSize = false)
         {
             if (image == null) return;
             ResourceManager.Instance.AsyncLoadResource(path, (s, o, list) =>
             {
                 if (o != null)
                 {
                     Sprite sp = o as Sprite;
                     Image img = list[0] as Image;
                     bool isSetNative = (bool) list[1];
                     if (img.sprite != null)
                     {
                         img.sprite = null;
                     }
     
                     img.sprite = sp;
                     if (isSetNative)
                     {
                         img.SetNativeSize();
                     }
                 }
             }, priority, true, image, isSetNativeSize);
         }
    
    public void TestChangeImageSpriteAsync2(string path, Image image, LoadResPriority priority,
        bool isSetNativeSize = false)
    {
        if (image == null) return;
        ResourceManager.Instance.AsyncLoadResource(path, (s, o, list) =>
        {
            if (o != null)
            {
              
      
                Sprite sp = o as Sprite;
                Image img = list[0] as Image;
                bool isSetNative = (bool) list[1];
                if (img.sprite != null)
                {
                    img.sprite = null;
                }

                img.sprite = sp;
                if (isSetNative)
                {
                    img.SetNativeSize();
                }
            }
        }, priority, true, image, isSetNativeSize);
    }
    #endregion

    void OnClickTest()
    {
        Debug.Log("Test");
    }

    public override void OnUpdate()
    {
        Debug.Log("update");
        
    }
}