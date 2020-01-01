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

    public int[] NodesChildCount;

    public bool[] NodesActive;
 
    public Vector3[] NodesPosition;
    public Vector3[] NodesScale;
    public Quaternion[] NodesRotation;

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
                tempNode.localPosition = NodesPosition[i];
                tempNode.localRotation = NodesRotation[i];
                tempNode.localScale = NodesScale[i];
                
                if (NodesActive[i])
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

                if (tempNode.childCount > NodesChildCount[i])
                {
                    int childCount = tempNode.childCount;
                    for (int j = NodesChildCount[i]; j < childCount; j++)
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
        NodesChildCount = new int[allNodesCount];
        NodesActive = new bool[allNodesCount];
        NodesPosition = new Vector3[allNodesCount];
        NodesScale = new Vector3[allNodesCount];
        NodesRotation = new Quaternion[allNodesCount];
        for (int i = 0; i < allNodesCount; i++)
        {
            Transform tempNode = AllNodes[i];
            NodesChildCount[i] = tempNode.childCount;
            NodesActive[i] = tempNode.gameObject.activeSelf;
            NodesPosition[i] = tempNode.localPosition;
            NodesRotation[i] = tempNode.localRotation;
            NodesScale[i] = tempNode.localScale;
        }
    }


}