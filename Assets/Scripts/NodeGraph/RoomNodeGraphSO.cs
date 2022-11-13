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
    /// 룸노드 리스트에서 룸노드 사전을 불러온다
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
    /// 룸 노드 ID로 부터 룸 노드를 얻는다
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
    // 아래부터는 유니티 에디터에서만 작동 되는 코드
#if UNITY_EDITOR
    // 방 노드를 담을 변수
    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
    //선 위치 변수
    [HideInInspector] public Vector2 linePosition;

    // 편집기에서 값이 변경될 때마다 호출
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
