using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id; // 룸 노드의 ID, GUID에서 생성한 ID
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>(); // 부모 룸 노드 ID를 유지관리
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>(); // 자식 룸 노드 ID를 유지관리 
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph; // 컨테이너를 보유하는 변수
    public RoomNodeTypeSO roomNodeType; // 룸 노드 유형
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList; // 룸 노드 유형 목록

    #region Editor Code

#if UNITY_EDITOR

    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isRightClickDragging = false;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        // 방 노드 타입 리스트 불러옴
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        // BeginArea를 이용해 노드상자 그리기
        GUILayout.BeginArea(rect, nodeStyle);

        // 선택 변경을 감지하는 영역
        EditorGUI.BeginChangeCheck();

        // 부모노드가 있거나 방 타입이 Entrance 인 경우
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // 방 종류를 변경하지 못하게 잠근다
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // 누르면 타입을 변경할 수 있는 팝업박스를 그림 (기본값은 현재 설정된 방 타입)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // 방 타입이 선택으로 인해 변경될 경우 자식노드에 대한 유효성 검사
            /* 복도가 원래 선택되었지만 지금은 선택되지 않은 경우  
             ||복도가 원래 선택되지 않았지만 지금은 선택된 경우
             || 보스방이 원래 선택되지 않았지만 지금은 선택된 경우*/
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor 
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor 
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {

                // 자식ID가 존재한다면
                if (childRoomNodeIDList.Count > 0)
                {
                    // 목록에 있는 자식ID 갯수만큼 반복
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // 자식 노드 가져옴
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // 자식 노드가 존재 한다면
                        if (childRoomNode != null)
                        {
                            // 부모 노드에서 자식ID 제거
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // 자식 노드에서 부모ID 제거
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);

                        }
                    }
                }
            }
        }
    

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this);

        GUILayout.EndArea();
    }

    /// <summary>
    /// 선택 가능한 표시할 룸 노드 타입으로 문자열 배열을 채운다
    /// </summary>
    /// <returns></returns>
    public string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];

        for(int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if(roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }

        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // 마우스 다운 이벤트
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // 마우스 업 이벤트
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // 마우스 업 이벤트
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseDownEvent (Event currentEvent)
    {
        // 0 = 왼쪽버튼, 1 = 오른쪽버튼
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }

        if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    private void ProcessLeftClickDownEvent()
    {
        if(isSelected == true)
        {
            isSelected = false;
        }
        else
        {
            isSelected = true;
        }
    }

    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    private void ProcessRightClickUpEvent()
    {
        // 마우스 오른쪽 버튼을 놓으면 더 이상 드래그 되지 않음
        if (isRightClickDragging)
        {
            isRightClickDragging = false;
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        // 마우스 왼쪽버튼을 놓으면 더 이상 드래그 되지 않음
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent);
        }

        else if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        isRightClickDragging = true;
        // currentEvent.delta -> 마지막 이벤트와 비교한 마우스의 움직임
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        // currentEvent.delta -> 마지막 이벤트와 비교한 마우스의 움직임
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// 자식노드의 ID를 추가 (성공적으로 추가 되었으면 true 반환)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // 유효성 검사
        if(IsChildRoomVaild (childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }
    /// <summary>
    /// 자식노드가 부모노드에 유효하게 추가될수 있는가
    /// </summary>
    /// <param name="childID"></param>
    private bool IsChildRoomVaild(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        
        // 보스방은 한 맵에 하나만 존재해야 함
        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            // 방 타입이 보스룸이고 이 보스룸이 부모 노드를 가지고 있는 경우(선으로 연결되어 있는 경우)  true 반환
            if(roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        // 자식노드가 보스방이고 보스방이 부모 노드를 가지고 있을 경우(선으로 연결되어 있는 경우) false 반환
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        // 자식노드가 None 타입의 방일경우 false 반환
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
        {
            return false;
        }

        // 자기 자신을 자식노드로 선택할 경우 false 반환
        if(id == childID)
        {
            return false;
        }

        // 이 자식노드ID 가 이미 부모노드ID 목록에 있을 경우 (중복되었을 경우) false 반환
        if(parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // 이 자식노드가 이미 부모노드를 가지고 있을 경우 false 반환 (모든 노드가 하나의 부모만을 가진다)
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        // 복도끼리는 서로 연결 되었을 경우 false 반환
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false;
        }

        // 자식이 복도방이 아니고, 이 방이 복도방이 아닐경우 false 반환(복도방과 연결이 되어있지 않을경우)
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false;
        }

        // 복도를 자식노드로 추가할 때 연결된 복도수가 최대 복도수보다 크거나 같으면 false 반환
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }

        // 입구방이 자식노드일 경우 false 반환 (입구방은 항상 최상위 노드여야 함)
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
        {
            return false;
        }
        
        // 복도방을 다른방에 연결 할 때 복도방이 이미 자식노드를 가지고 있으면 false 반환 (복도방은 2개의 자식노드를 둘 수 없음)
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 부모노드의 ID를 추가 (성공적으로 추가 되었으면 true 반환)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// 자식 노드 삭제 (성공하면 true 반환)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // 자식 노드가 존재 한다면 그것을 삭제
        if(childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }

        return false;
    }

    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // 부모 노드가 존재 한다면 그것을 삭제
        if(parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }

        return false;
    }

#endif

    #endregion Editor Code
}