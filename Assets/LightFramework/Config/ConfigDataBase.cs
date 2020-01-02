namespace LightFramework.Config
{
    [System.Serializable]
    public class ConfigDataBase
    {
#if UNITY_EDITOR
        public virtual void Construction()
        {
        
        }
#endif

        public virtual void Init()
        {
        
        }
    }
}