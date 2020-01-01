using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameRoot : MonoSingleton<GameRoot>
{
    public AudioSource AudioSource;
    public AudioClip AudioClip;

    public GameObject InstanceGameObject;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleCofig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecylePool"), transform.Find("SceneTrans"));
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform,
            transform.Find("UIRoot/WndRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
            transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
        MapManager.Instance.Init(this);
        LoadConfig();
        RegisterUI();
    }
    private void Start()
    {
        #region etc

//        ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Cube.prefab", true);
//        AudioClip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
//        ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
//        var obj = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true, false);
//        ObjectManager.Instance.ReleaseObject(obj);
//        obj = null;
//     var clip2 = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");

//        ObjectManager.Instance.PreLoadObject("Assets/GameData/Prefabs/Attack.prefab", 10);
//        GameObject gameObject = ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab");
//        ObjectManager.Instance.ReleaseObject(gameObject,0);
        //        AudioClip clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
//        ResourceManager.Instance.ReleaseResource(clip);

        // MapManager.Instance.LoadScene(SceneDefinition.Menu);
        // AudioSource.clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/senlin.mp3");
        // AudioSource.Play();
        //        ObjectManager.Instance.Instantiate ObjectAsync("Assets/GameData/Prefabs/Attack.prefab",
//            (path, obj, paramList) => { InstanceGameObject = obj as GameObject; }, LoadResPriority.RES_HIGH,true,true);

//InstanceGameObject=  ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab");

        //        InstanceGameObject =
//            ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true, true);


//        ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", (path, obj, par) =>
//            {
//                AudioSource.clip = obj as AudioClip;
//                AudioSource.Play();
//                AudioClip = obj as AudioClip;
//            },
//            LoadResPriority.RES_HIGH);
//        ResourceManager.Instance.PreLoadRes("Assets/GameData/Sounds/menusound.mp3");

        #endregion
    }

    void RegisterUI()
    {
        // UIManager.Instance.Register<MenuWindow>(UIDefinition.Menu);
        // UIManager.Instance.Register<LoadingWindow>(UIDefinition.Loading);
    }
    void LoadConfig()
    {
        //   ConfigManager.Instance.LoadConfigData<BuffConfigData>(CFG.TableBuff);
        // ConfigManager.Instance.LoadConfigData<PokemonConfigData>(CFG.TablePokemon);
    }

    private void Update()
    {
//         UIManager.Instance.Update();
//         if (Input.GetKeyDown(KeyCode.A))
//         {
//             //      ResourceManager.Instance.ReleaseResource(AudioClip);
// //            AudioClip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
// //            AudioSource.clip = AudioClip;
// //            AudioSource.Play();
//             ObjectManager.Instance.ReleaseObject(InstanceGameObject);
//             InstanceGameObject = null;
//         }
//
//         if (Input.GetKeyDown(KeyCode.F))
//         {
// //            AudioClip = null;
// //         AudioSource.clip = null;
// //            ResourceManager.Instance.ReleaseResource(AudioClip);
//             InstanceGameObject =
//                 ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab", true);
//         }
//
//         if (Input.GetKeyDown(KeyCode.S))
//         {
//             ObjectManager.Instance.ReleaseObject(InstanceGameObject, 0, true);
//             InstanceGameObject = null;
//         }
//
//
//         if (Input.GetKeyDown(KeyCode.P))
//         {
//             ObjectManager.Instance.PreLoadObject("Assets/GameData/Prefabs/Attack.prefab", 1);
//         }
//
//         if (Input.GetKeyDown(KeyCode.Z))
//         {
//             ObjectManager.Instance.ReleaseObject(InstanceGameObject, 0, true);
//         }
//
//         if (Input.GetKeyDown(KeyCode.O))
//         {
//             ResourceManager.Instance.ReleaseResource(AudioClip, true);
//         }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
#endif
    }
}