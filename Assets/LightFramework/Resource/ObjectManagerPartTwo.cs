using UnityEngine;

namespace LightFramework.Resource
{
    public partial class ObjectManager
    {
        /// <summary>
        ///使用offlinedata节省GetComponent开销来获取组件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public OfflineData.OfflineData FindOfflineData(GameObject gameObject)
        {
            OfflineData.OfflineData offlineData = null;
            if (ObjectItemsInstanceTempDic.TryGetValue(gameObject.GetInstanceID(), out ObjectItem objectItem))
            {
                offlineData = objectItem.OfflineData;
            }

            return offlineData;
        }
    }
}