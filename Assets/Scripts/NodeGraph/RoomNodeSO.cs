using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id; // �� ����� ID, GUID���� ������ ID
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>(); // �θ� �� ��� ID�� ��������
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>(); // �ڽ� �� ��� ID�� �������� 
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph; // �����̳ʸ� �����ϴ� ����
    public RoomNodeTypeSO roomNodeType; // �� ��� ����
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList; // �� ��� ���� ���

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

        // �� ��� Ÿ�� ����Ʈ �ҷ���
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        // BeginArea�� �̿��� ������ �׸���
        GUILayout.BeginArea(rect, nodeStyle);

        // ���� ������ �����ϴ� ����
        EditorGUI.BeginChangeCheck();

        // �θ��尡 �ְų� �� Ÿ���� Entrance �� ���
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            // �� ������ �������� ���ϰ� ��ٴ�
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // ������ Ÿ���� ������ �� �ִ� �˾��ڽ��� �׸� (�⺻���� ���� ������ �� Ÿ��)
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // �� Ÿ���� �������� ���� ����� ��� �ڽĳ�忡 ���� ��ȿ�� �˻�
            /* ������ ���� ���õǾ����� ������ ���õ��� ���� ���  
             ||������ ���� ���õ��� �ʾ����� ������ ���õ� ���
             || �������� ���� ���õ��� �ʾ����� ������ ���õ� ���*/
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor 
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor 
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {

                // �ڽ�ID�� �����Ѵٸ�
                if (childRoomNodeIDList.Count > 0)
                {
                    // ��Ͽ� �ִ� �ڽ�ID ������ŭ �ݺ�
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // �ڽ� ��� ������
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // �ڽ� ��尡 ���� �Ѵٸ�
                        if (childRoomNode != null)
                        {
                            // �θ� ��忡�� �ڽ�ID ����
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);

                            // �ڽ� ��忡�� �θ�ID ����
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
    /// ���� ������ ǥ���� �� ��� Ÿ������ ���ڿ� �迭�� ä���
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
            // ���콺 �ٿ� �̺�Ʈ
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // ���콺 �� �̺�Ʈ
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // ���콺 �� �̺�Ʈ
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    private void ProcessMouseDownEvent (Event currentEvent)
    {
        // 0 = ���ʹ�ư, 1 = �����ʹ�ư
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
        // ���콺 ������ ��ư�� ������ �� �̻� �巡�� ���� ����
        if (isRightClickDragging)
        {
            isRightClickDragging = false;
        }
    }

    private void ProcessLeftClickUpEvent()
    {
        // ���콺 ���ʹ�ư�� ������ �� �̻� �巡�� ���� ����
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
        // currentEvent.delta -> ������ �̺�Ʈ�� ���� ���콺�� ������
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    private void ProcessLeftMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        // currentEvent.delta -> ������ �̺�Ʈ�� ���� ���콺�� ������
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// �ڽĳ���� ID�� �߰� (���������� �߰� �Ǿ����� true ��ȯ)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        // ��ȿ�� �˻�
        if(IsChildRoomVaild (childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }
    /// <summary>
    /// �ڽĳ�尡 �θ��忡 ��ȿ�ϰ� �߰��ɼ� �ִ°�
    /// </summary>
    /// <param name="childID"></param>
    private bool IsChildRoomVaild(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        
        // �������� �� �ʿ� �ϳ��� �����ؾ� ��
        foreach(RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
        {
            // �� Ÿ���� �������̰� �� �������� �θ� ��带 ������ �ִ� ���(������ ����Ǿ� �ִ� ���)  true ��ȯ
            if(roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        // �ڽĳ�尡 �������̰� �������� �θ� ��带 ������ ���� ���(������ ����Ǿ� �ִ� ���) false ��ȯ
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        // �ڽĳ�尡 None Ÿ���� ���ϰ�� false ��ȯ
        if(roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
        {
            return false;
        }

        // �ڱ� �ڽ��� �ڽĳ��� ������ ��� false ��ȯ
        if(id == childID)
        {
            return false;
        }

        // �� �ڽĳ��ID �� �̹� �θ���ID ��Ͽ� ���� ��� (�ߺ��Ǿ��� ���) false ��ȯ
        if(parentRoomNodeIDList.Contains(childID))
        {
            return false;
        }

        // �� �ڽĳ�尡 �̹� �θ��带 ������ ���� ��� false ��ȯ (��� ��尡 �ϳ��� �θ��� ������)
        if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        // ���������� ���� ���� �Ǿ��� ��� false ��ȯ
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
        {
            return false;
        }

        // �ڽ��� �������� �ƴϰ�, �� ���� �������� �ƴҰ�� false ��ȯ(������� ������ �Ǿ����� �������)
        if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        {
            return false;
        }

        // ������ �ڽĳ��� �߰��� �� ����� �������� �ִ� ���������� ũ�ų� ������ false ��ȯ
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
        {
            return false;
        }

        // �Ա����� �ڽĳ���� ��� false ��ȯ (�Ա����� �׻� �ֻ��� ��忩�� ��)
        if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
        {
            return false;
        }
        
        // �������� �ٸ��濡 ���� �� �� �������� �̹� �ڽĳ�带 ������ ������ false ��ȯ (�������� 2���� �ڽĳ�带 �� �� ����)
        if(!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// �θ����� ID�� �߰� (���������� �߰� �Ǿ����� true ��ȯ)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// �ڽ� ��� ���� (�����ϸ� true ��ȯ)
    /// </summary>
    /// <param name="childID"></param>
    /// <returns></returns>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        // �ڽ� ��尡 ���� �Ѵٸ� �װ��� ����
        if(childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }

        return false;
    }

    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        // �θ� ��尡 ���� �Ѵٸ� �װ��� ����
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