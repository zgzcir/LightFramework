using UnityEngine;

namespace LightFramework.Resource.OfflineData
{
    public class UIOfflineData : OfflineData
    {
        public Vector2[] AnchorMaxs;
        public Vector2[] AnchorMins;

        public Vector2[] Pivots;
        public Vector2[] SizeDeltas;
        public Vector3[] AnchoredPositions;

        public ParticleSystem[] ParticleSystems;


        public override void ResetProp()
        {
            int count = AllNodes.Length;
            for (int i = 0; i <count; i++)
            {
                RectTransform rectTransform = AllNodes[i] as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.localPosition = NodesPosition[i];
                    rectTransform.localRotation = NodesRotation[i];
                    rectTransform.localScale = NodesScale[i];
                    rectTransform.anchorMax = AnchorMaxs[i];
                    rectTransform.anchorMin = AnchorMins[i];
                    rectTransform.pivot = Pivots[i];
                    rectTransform.sizeDelta = Pivots[i];
                    rectTransform.anchoredPosition = AnchoredPositions[i];
                    
                    if (NodesActive[i])
                    {
                        if (!rectTransform.gameObject.activeSelf)
                        {
                            rectTransform.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (rectTransform.gameObject.activeSelf)
                        {
                            rectTransform.gameObject.SetActive(false);
                        }
                    }
                
                    if (rectTransform.childCount > NodesChildCount[i])
                    {
                        int childCount = rectTransform.childCount;
                        for (int j = NodesChildCount[i]; j < childCount; j++)
                        {
                            GameObject tempObj = rectTransform.GetChild(j).gameObject;
                            if (!ObjectManager.Instance.IsFrameCreat(tempObj))
                            {
                                Destroy(tempObj);
                            }
                        }
                    }
                }
            
           

            } 

            int particleCount = ParticleSystems.Length;
            for (int i = 0; i < particleCount; i++)
            {
                ParticleSystems[i].Clear(true);
                ParticleSystems[i].Play();
            }
        }

        public override void BindData()
        {
            Transform[] allNodes = gameObject.GetComponentsInChildren<Transform>(true);
            AllNodes = allNodes;
            ParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);

            var allNodesCount = allNodes.Length;
            for (int i = 0; i < allNodesCount; i++)
            {
                if (!(allNodes[i] is RectTransform))
                {
                    allNodes[i].gameObject.AddComponent<RectTransform>();
                }
            }

            NodesChildCount = new int[allNodesCount];
            NodesActive = new bool[allNodesCount];
            NodesPosition = new Vector3[allNodesCount];
            NodesScale = new Vector3[allNodesCount];
            NodesRotation = new Quaternion[allNodesCount];

            AnchorMaxs = new Vector2[allNodesCount];
            AnchorMins = new Vector2[allNodesCount];
            Pivots = new Vector2[allNodesCount];
            SizeDeltas = new Vector2[allNodesCount];
            AnchoredPositions = new Vector3[allNodesCount];
            for (int i = 0; i < allNodesCount; i++)
            {
                RectTransform tempNode = allNodes[i] as RectTransform;
                NodesChildCount[i] = tempNode.childCount;
                NodesActive[i] = tempNode.gameObject.activeSelf;
                NodesPosition[i] = tempNode.localPosition;
                NodesRotation[i] = tempNode.localRotation;
                NodesScale[i] = tempNode.localScale;
                AnchorMaxs[i] = tempNode.anchorMax;
                AnchorMins[i] = tempNode.anchorMin;
                Pivots[i] = tempNode.pivot;
                SizeDeltas[i] = tempNode.sizeDelta;
                AnchoredPositions[i] = tempNode.anchoredPosition;
            }
        }
    }
}