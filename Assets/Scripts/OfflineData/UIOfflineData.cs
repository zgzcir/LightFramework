using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        base.ResetProp();
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

        EachNodeChildCount = new int[allNodesCount];
        EachNodeActive = new bool[allNodesCount];
        EachNodePosition = new Vector3[allNodesCount];
        EachNodeScale = new Vector3[allNodesCount];
        EachNodeRotation = new Quaternion[allNodesCount];

        AnchorMaxs = new Vector2[allNodesCount];
        AnchorMins = new Vector2[allNodesCount];
        Pivots = new Vector2[allNodesCount];
        SizeDeltas = new Vector2[allNodesCount];
        AnchoredPositions = new Vector3[allNodesCount];
        for (int i = 0; i < allNodesCount; i++)
        {
            RectTransform tempNode = allNodes[i] as RectTransform;
            EachNodeChildCount[i] = tempNode.childCount;
            EachNodeActive[i] = tempNode.gameObject.activeSelf;
            EachNodePosition[i] = tempNode.localPosition;
            EachNodeRotation[i] = tempNode.localRotation;
            EachNodeScale[i] = tempNode.localScale;
            AnchorMaxs[i] = tempNode.anchorMax;
            AnchorMins[i] = tempNode.anchorMin;
            Pivots[i] = tempNode.pivot;
            SizeDeltas[i] = tempNode.sizeDelta;
            AnchoredPositions[i] = tempNode.anchoredPosition;
        }
    }
}