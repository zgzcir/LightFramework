using UnityEngine;
using UnityEngine.Serialization;

public class OfflineData : MonoBehaviour
{
    public Rigidbody Rigidbody;
    public Collider Collider;

    /// <summary>
    /// 自身及子节点
    /// </summary>
    public Transform[] AllNodes;

    public int[] EachNodeChildCount;

    public bool[] EachNodeActive;

    public Vector3[] EachNodePosition;
    public Vector3[] EachNodeScale;
    public Quaternion[] EachNodeRotation;

    /// <summary>
    /// 还原属性
    /// </summary>
    public virtual void ResetProp()
    {
        int allNodesConut = AllNodes.Length;
        for (int i = 0; i < allNodesConut; i++)
        {
            Transform tempNode = AllNodes[i];
            if (tempNode != null)
            {
                tempNode.localPosition = EachNodePosition[i];
                tempNode.localRotation = EachNodeRotation[i];
                tempNode.localScale = EachNodeScale[i];
                if (EachNodeActive[i])
                {
                    if (!tempNode.gameObject.activeSelf)
                    {
                        tempNode.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (tempNode.gameObject.activeSelf)
                    {
                        tempNode.gameObject.SetActive(false);
                    }
                }

                if (tempNode.childCount > EachNodeChildCount[i])
                {
                    int childCount = tempNode.childCount;
                    for (int j = EachNodeChildCount[i]; j < childCount; j++)
                    {
                        GameObject tempObj = tempNode.GetChild(j).gameObject;
                        if (!ObjectManager.Instance.IsFrameCreat(tempObj))
                        {
                            Destroy(tempObj);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 编辑器下保存初始数据
    /// </summary>
    public virtual void BindData()
    {
        Collider = gameObject.GetComponentInChildren<Collider>(true);
        Rigidbody = gameObject.GetComponentInChildren<Rigidbody>(true);
        AllNodes = gameObject.GetComponentsInChildren<Transform>(true);
        int allNodesCount = AllNodes.Length;
        EachNodeChildCount = new int[allNodesCount];
        EachNodeActive = new bool[allNodesCount];
        EachNodePosition = new Vector3[allNodesCount];
        EachNodeScale = new Vector3[allNodesCount];
        EachNodeRotation = new Quaternion[allNodesCount];
        for (int i = 0; i < allNodesCount; i++)
        {
            Transform tempNode = AllNodes[i];
            EachNodeChildCount[i] = tempNode.childCount;
            EachNodeActive[i] = tempNode.gameObject.activeSelf;
            EachNodePosition[i] = tempNode.localPosition;
            EachNodeRotation[i] = tempNode.localRotation;
            EachNodeScale[i] = tempNode.localScale;
        }
    }
}