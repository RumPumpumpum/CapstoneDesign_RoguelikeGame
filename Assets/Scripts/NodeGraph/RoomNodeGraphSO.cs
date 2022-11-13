using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake()
    {
        LoadRoomNodeDictionary();
    }
    
    /// <summary>
    /// ���� ����Ʈ���� ���� ������ �ҷ��´�
    /// </summary>
    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach(RoomNodeSO node in roomNodeList)
        {
            roomNodeDictionary[node.id] = node;
        }
    }

    /// <summary>
    /// �� ��� ID�� ���� �� ��带 ��´�
    /// </summary>
    /// <param name="roomNodeID"></param>
    /// <returns></returns>
    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

    #region Editor Code
    // �Ʒ����ʹ� ����Ƽ �����Ϳ����� �۵� �Ǵ� �ڵ�
#if UNITY_EDITOR
    // �� ��带 ���� ����
    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
    //�� ��ġ ����
    [HideInInspector] public Vector2 linePosition;

    // �����⿡�� ���� ����� ������ ȣ��
    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }

    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
    {
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }
#endif
    #endregion Editor Code
}
