using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;    // �����Ϳ��� �߻��ϴ� Ư�� ���� �����ϰ� ĸ��
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;    // ���� ������ ������

    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    // ��� �ʺ� 160�ȼ�, ��� ���� 75�ȼ�
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;

    // ��� �е� 25�ȼ�, ��� �׵θ� 12�ȼ�
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // ����� ���� ��
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    // �׸��� ����
    private const float gridLarge = 100f;   // ū ����
    private const float gridSmall = 25f;    // ��������

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Note Graph Editor")]

    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }


    private void OnEnable()
    {
        // �����⿡���� ���� ��������� ����
        Selection.selectionChanged += InspectorSelectionChanged;

        // ��� ���̾ƿ�
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        // ����Ƽ���� ����� �ؽ��� �ε�
        roomNodeStyle.normal.textColor = Color.white;
        // �ؽ�Ʈ ������ �������
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
        // �е��� �׵θ��� ����

        //������ ��� ���̾ƿ�
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // �� ��� Ÿ�� �ҷ���
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // �����⿡���� ���� ��������� ���� ����
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    #region OnDoubleClickAsset
    /// <summary>
    /// ��� �׷����� ����Ŭ���� �� room node graph editor â�� ����
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    #endregion
    [OnOpenAsset(0)] // UnityEditor.callbacks �ʿ�
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
    
        if (roomNodeGraph != null)
        {
            OpenWindow();

            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        if(currentRoomNodeGraph != null)
        {
            // ���ڸ� �׸���
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);
            
            // �巡���� �� ���� �׸���
            DrawDraggedLine();

            // �̺�Ʈ ó��
            ProcessEvents(Event.current);

            DrawRoomConnections();

            // Draw Room Nodes
            DrawRoomNodes();
        }
        // GUI�� �������� ������ �ٽ� �׸�
        if (GUI.changed) 
            Repaint();
    }
    
    /// <summary>
    /// �� ��� ������ ��濡 ���ڸ� �׸�
    /// </summary>
    /// <param name="gridSize"></param>
    /// <param name="gridOpacity"></param>
    /// <param name="girdColor"></param>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        // �׸���� ��Ī�̱⶧���� �巡�� �Ÿ��� ���ݸ� �̵��ϸ� �ȴ�.
        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        for(int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f)
                + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * j, 0f)
                + gridOffset);
        }

        Handles.color = Color.white;
    }


    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            // ������ � �̿�
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }

    }

    private void ProcessEvents(Event currentEvent)
    {
        // �׷��� �巡�� ���� �ʱ�ȭ
        graphDrag = Vector2.zero;

        // ���콺�� �� ��� ���� ���� �ʰų�, �巡�������� ���� ���
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // �� ��尡 null�̰ų�, ���� �� ��忡�� ������ �巡�� �� �ϰ��
        if(currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // 
        else
        {
            // �̿��� ���
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    ///  ���콺�� �� ��� ���� �ִ��� üũ, ���� ������ ���� ��ȯ , ������ null ��ȯ
    /// </summary>
    /// <param name="currentEvent"></param>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for(int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            // �ش� �� ��� �簢�� ������ ���콺 �̺�Ʈ�� ���ԵǾ� �ִ��� Ȯ��
            if(currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }


    /// <summary>
    /// �� ��� �׷��� �̺�Ʈ�� �߻�
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // ���콺 Ŭ�� �̺�Ʈ�� �߻����� ��
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // ���콺 �� �̺�Ʈ�� �߻����� ��
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // ���콺 �巡�� �̺�Ʈ�� �߻����� ��
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// ���콺 Ŭ���� �߻��ϴ� �̺�Ʈ
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // ���콺 ��Ŭ�� �� �� �ڸ��� �׷��� �̺�Ʈ �߻�
        // 0=��Ŭ��, 1=��Ŭ��
        if(currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }

        // ���콺 ��Ŭ�� ��
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Context �޴��� ������
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        // CreateRoomNode�� mousePosition ��ü�� �����Ͽ� ȣ���ϴ� item�� �߰�
        menu.AddItem(new GUIContent("�� ��� ����"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator(""); // �и� ��ȣ
        menu.AddItem(new GUIContent("��� �� ��� ����"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("���õ� �� ����� ���ἱ ����"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("���õ� �� ��� ����"), false, DeleteSelectedRoomNodes);



        menu.ShowAsContext();
    }

    /// <summary>
    /// ���콺 ��ġ�� �� ��带 �����.
    /// </summary>
    /// <param name="mousePositionObject"></param>
    private void CreateRoomNode(object mousePositionObject)
    {
        // ��� �׷����� ������� �� ���� ������� �����
        if(currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        // lambda expression : (�Ķ����) => ��; 
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// ���콺 ��ư�� ������ �� �߻��ϴ� �̺�Ʈ
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // ���콺 ��Ŭ���� ���������� �̺�Ʈ����
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // ���콺�� ��� ���� �ִ��� üũ
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if(roomNode != null)
            {
                // �巡�� ������ ���� �θ��� ������ ��尡 �ڽĳ��
                // �θ��忡 �ڽĳ���� ID�� �߰��ϰ� �����ϸ� true ��ȯ
                if(currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // �ڽ� ��忡 �θ����� ID �߰�
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// ���콺 �巡�� �̺�Ʈ
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // ���콺 ��Ŭ���� ���� ������ �̺�Ʈ����
        if(currentEvent.button == 1)
        {
            // ���콺 ���� �̺�Ʈ�� ó���ϱ� ���� �޼���
            ProcessRightMouseDragEvent(currentEvent);
        }

        // ���콺 ��Ŭ���� ���� ������ �̺�Ʈ����
        else if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// ���콺 ��Ŭ�� �巡�� �̺�Ʈ
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if(currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// ���콺 ��Ŭ�� �巡�� �̺�Ʈ
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        for(int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(dragDelta);
        }

        GUI.changed = true;
    }


    /// <summary>
    /// ���콺 ��ġ�� �� ��带 �����. -�����ε�� �Լ�
    /// </summary>
    /// <param name="mousePositionObject"></param>
    /// <param name="roomNodeType"></param>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // ScriptableObject �� ��� ����
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // ���� �� ��� ����Ʈ ��Ͽ� ���带 �߰�
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // �� ��� ��  ����
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // scriptable object asset database�� �� ��� �߰�
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        // ��� �׷��� ���ΰ�ħ
        currentRoomNodeGraph.OnValidate();
    }
    
    /// <summary>
    /// ���õ� �� ��带 �����Ѵ�.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        // ���Լ��� Queue
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        // ��� ��� Ȯ��
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            // �� ��尡 ���� �Ǿ��ִ°� && �Ա����� ���� �Ұ���
            if(roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                // ���� ��⿭�� �� ��� �߰� 
                roomNodeDeletionQueue.Enqueue(roomNode);

                // �ڽ� �� ����� ID�� Ž��
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // �ڽ� ���� ������
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    
                    if(childRoomNode != null)
                    {
                        // �ڽ� �� ��忡�� �θ�ID ����
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // �θ� �� ����� ID�� Ž��
                foreach (string parentRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // �θ� ���� ������
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // �θ� �� ��忡�� �ڽ�ID ����
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

            }
        }

        // ���� ��⿭�� ���� ����
        while (roomNodeDeletionQueue.Count > 0)
        {
            // ť�κ��� �� ��� ������, ���� ť���� �ش� ��� ����
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // �������� �� ��� ����
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // �� ��� ����Ʈ���� �� ��� ����
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // ������ �� ����� ���� ����
            DestroyImmediate(roomNodeToDelete, true);

            // ������ ������� ����
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// ���õ� �� ��峢���� ���ἱ�� �����Ѵ�.
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // ��� ��� Ž��
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            // ���õ� �� ����� �ڽ� ��尡 ���� �� ���
            if(roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for(int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // �ڽ� ��� ������
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // �ڽ� ��尡 ���� �Ǿ� �ִٸ�
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // �θ� ��忡�� �ڽ�ID ����
                        roomNode.RemoveChildRoomNodeIDFromRoomNode (childRoomNode.id);

                        // �ڽ� ��忡�� �θ�ID ����
                        roomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);

                    }
                }
            }
        }
    }

    /// <summary>
    /// ��� �� ����� ������ �����Ѵ�
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true; 
            }
        }
    }

    /// <summary>
    /// ��� �� ��� ����
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach(RoomNodeSO  roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }

        GUI.changed = true;
    }

    /// <summary>
    ///  ��� ���ἱ �׸���
    /// </summary>
    /// <param name="delta"></param>
    private void DragConnectingLine(Vector2 delta)
    {
        // ������ ���� ��ġ ���� ������Ʈ
        currentRoomNodeGraph.linePosition += delta;
    }


    /// <summary>
    /// ��� ���ἱ ����
    /// </summary>
    private void ClearLineDrag()
    {
        // �巡�׸� ���̻� ���� ������
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        // Vector2 �� ��ġ�� zero�� �����.
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }


    /// <summary>
    /// �θ���� �ڽĳ�� ���̿� ���ἱ�� �׸���
    /// </summary>
    private void DrawRoomConnections()
    {
        // ��� ���� �ݺ�
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.childRoomNodeIDList.Count > 0)
            {
                // �ڽ� ���� �ݺ�
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // ���� �ڽ� ���� Ű�� ������ �����ϴ��� Ȯ��
                    if(currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        // ���� �� ���� �ڽ� ���� ���̿� �� ����
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// �θ���� �ڽĳ�� ���̿� ���ἱ�� �׸���
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // ���� �������� ������
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // �߰��� ���
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // ���������� ������ ������ ���� ���� ���
        Vector2 direction = endPosition - startPosition;

        // �߰������� ����ȭ�� �������� ���
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        // ȭ��ǥ�� �Ӹ����� ���
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        // ȭ��ǥ �׸���
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        // �� �׸���
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// ������ â�� �� ��带 �׸�
    /// </summary>
    private void DrawRoomNodes()
    {
        // ��� �� ��� Ž��
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }

            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }

        GUI.changed = true;
    }
        
    /// <summary>
    /// �����⿡�� �� ��� �׷��� ������ ����� ���
    /// </summary>
    private void InspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

        if(roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }

}
