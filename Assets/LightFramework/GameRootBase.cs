using LightFramework.Base;
using LightFramework.Resource;
using LightFramework.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightFramework
{
    public class GameRootBase : MonoSingleton<GameRootBase>
    {
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
            SceneManager.Instance.Init(this);
            LoadConfig();
            RegisterUI();
        }

        protected virtual void Start()
        {
          
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

        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            ResourceManager.Instance.ClearCache();
            Resources.UnloadUnusedAssets();
#endif
        }
    }
}