using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;    // 에디터에서 발생하는 특정 일을 감지하고 캡쳐
using UnityEditor;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;    // 수평선 수직선 오프셋

    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    // 노드 너비 160픽셀, 노드 높이 75픽셀
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;

    // 노드 패딩 25픽셀, 노드 테두리 12픽셀
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    // 연결된 선의 값
    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    // 그리드 간격
    private const float gridLarge = 100f;   // 큰 격자
    private const float gridSmall = 25f;    // 작은격자

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Note Graph Editor")]

    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }


    private void OnEnable()
    {
        // 편집기에서의 선택 변경사항을 구독
        Selection.selectionChanged += InspectorSelectionChanged;

        // 노드 레이아웃
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        // 유니티에서 내장된 텍스쳐 로드
        roomNodeStyle.normal.textColor = Color.white;
        // 텍스트 색상을 흰색으로
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
        // 패딩과 테두리값 정의

        //선택한 노드 레이아웃
        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.white;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // 룸 노드 타입 불러옴
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    private void OnDisable()
    {
        // 편집기에서의 선택 변경사항을 구독 해제
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    #region OnDoubleClickAsset
    /// <summary>
    /// 노드 그래프를 더블클릭할 때 room node graph editor 창을 연다
    /// </summary>
    /// <param name="instanceID"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    #endregion
    [OnOpenAsset(0)] // UnityEditor.callbacks 필요
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
            // 격자를 그린다
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);
            
            // 드래그할 때 선을 그린다
            DrawDraggedLine();

            // 이벤트 처리
            ProcessEvents(Event.current);

            DrawRoomConnections();

            // Draw Room Nodes
            DrawRoomNodes();
        }
        // GUI의 변경점이 있으면 다시 그림
        if (GUI.changed) 
            Repaint();
    }
    
    /// <summary>
    /// 룸 노드 에디터 배경에 격자를 그림
    /// </summary>
    /// <param name="gridSize"></param>
    /// <param name="gridOpacity"></param>
    /// <param name="girdColor"></param>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        // 그리드는 대칭이기때문에 드래그 거리의 절반만 이동하면 된다.
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
            // 베지어 곡선 이용
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }

    }

    private void ProcessEvents(Event currentEvent)
    {
        // 그래프 드래그 변수 초기화
        graphDrag = Vector2.zero;

        // 마우스가 룸 노드 위에 있지 않거나, 드래그중이지 않을 경우
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        // 룸 노드가 null이거나, 현재 룸 노드에서 라인을 드래그 중 일경우
        if(currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        // 
        else
        {
            // 이외의 경우
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    ///  마우스가 룸 노드 위에 있는지 체크, 위에 있으면 룸노드 반환 , 없으면 null 반환
    /// </summary>
    /// <param name="currentEvent"></param>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for(int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            // 해당 룸 노드 사각형 영역에 마우스 이벤트가 포함되어 있는지 확인
            if(currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }


    /// <summary>
    /// 룸 노드 그래프 이벤트가 발생
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            // 마우스 클릭 이벤트가 발생했을 때
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            // 마우스 업 이벤트가 발생했을 때
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            // 마우스 드래그 이벤트가 발생했을 때
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 마우스 클릭시 발생하는 이벤트
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        // 마우스 우클릭 시 그 자리에 그래프 이벤트 발생
        // 0=좌클릭, 1=우클릭
        if(currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }

        // 마우스 좌클릭 시
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Context 메뉴를 보여줌
    /// </summary>
    /// <param name="mousePosition"></param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        // CreateRoomNode를 mousePosition 객체를 전달하여 호출하는 item을 추가
        menu.AddItem(new GUIContent("룸 노드 생성"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator(""); // 분리 기호
        menu.AddItem(new GUIContent("모든 룸 노드 선택"), false, SelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("선택된 룸 노드의 연결선 제거"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("선택된 룸 노드 제거"), false, DeleteSelectedRoomNodes);



        menu.ShowAsContext();
    }

    /// <summary>
    /// 마우스 위치에 룸 노드를 만든다.
    /// </summary>
    /// <param name="mousePositionObject"></param>
    private void CreateRoomNode(object mousePositionObject)
    {
        // 노드 그래프가 비어있을 때 먼저 입장방을 만든다
        if(currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        // lambda expression : (파라미터) => 식; 
        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
    }

    /// <summary>
    /// 마우스 버튼을 놓았을 때 발생하는 이벤트
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        // 마우스 우클릭을 누른상태의 이벤트인지
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            // 마우스가 노드 위에 있는지 체크
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

            if(roomNode != null)
            {
                // 드래그 시작한 곳이 부모노드 목적지 노드가 자식노드
                // 부모노드에 자식노드의 ID를 추가하고 성공하면 true 반환
                if(currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    // 자식 노드에 부모노드의 ID 추가
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// 마우스 드래그 이벤트
    /// </summary>
    /// <param name="currentEvent"></param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        // 마우스 우클릭을 누른 상태의 이벤트인지
        if(currentEvent.button == 1)
        {
            // 마우스 추적 이벤트를 처리하기 위한 메서드
            ProcessRightMouseDragEvent(currentEvent);
        }

        // 마우스 좌클릭을 누른 상태의 이벤트인지
        else if(currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// 마우스 우클릭 드래그 이벤트
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
    /// 마우스 좌클릭 드래그 이벤트
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
    /// 마우스 위치에 룸 노드를 만든다. -오버로드된 함수
    /// </summary>
    /// <param name="mousePositionObject"></param>
    /// <param name="roomNodeType"></param>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        // ScriptableObject 룸 노드 생성
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        // 현재 방 노드 리스트 목록에 방노드를 추가
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // 룸 노드 값  설정
        roomNode.Initialise(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

        // scriptable object asset database에 방 노드 추가
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        // 노드 그래프 새로고침
        currentRoomNodeGraph.OnValidate();
    }
    
    /// <summary>
    /// 선택된 룸 노드를 제거한다.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        // 선입선출 Queue
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        // 모든 노드 확인
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            // 룸 노드가 선택 되어있는가 && 입구방은 삭제 불가능
            if(roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                // 삭제 대기열에 룸 노드 추가 
                roomNodeDeletionQueue.Enqueue(roomNode);

                // 자식 룸 노드의 ID를 탐색
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 자식 룸노드 가져옴
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    
                    if(childRoomNode != null)
                    {
                        // 자식 룸 노드에서 부모ID 제거
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                // 부모 룸 노드의 ID를 탐색
                foreach (string parentRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 부모 룸노드 가져옴
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);

                    if (parentRoomNode != null)
                    {
                        // 부모 룸 노드에서 자식ID 제거
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

            }
        }

        // 삭제 대기열의 노드들 삭제
        while (roomNodeDeletionQueue.Count > 0)
        {
            // 큐로부터 룸 노드 가져옴, 이후 큐에서 해당 노드 제거
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();

            // 사전에서 룸 노드 제거
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);

            // 룸 노드 리스트에서 룸 노드 제거
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);

            // 삭제할 룸 노드의 에셋 제거
            DestroyImmediate(roomNodeToDelete, true);

            // 에셋의 변경사항 저장
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// 선택된 룸 노드끼리의 연결선을 제거한다.
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        // 모든 노드 탐색
        foreach(RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            // 선택된 룸 노드의 자식 노드가 존재 할 경우
            if(roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for(int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    // 자식 노드 가져옴
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    // 자식 노드가 선택 되어 있다면
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        // 부모 노드에서 자식ID 제거
                        roomNode.RemoveChildRoomNodeIDFromRoomNode (childRoomNode.id);

                        // 자식 노드에서 부모ID 제거
                        roomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);

                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 룸 노드의 선택을 해제한다
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
    /// 모든 룸 노드 선택
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
    ///  노드 연결선 그리기
    /// </summary>
    /// <param name="delta"></param>
    private void DragConnectingLine(Vector2 delta)
    {
        // 생성한 선의 위치 변수 업데이트
        currentRoomNodeGraph.linePosition += delta;
    }


    /// <summary>
    /// 노드 연결선 제거
    /// </summary>
    private void ClearLineDrag()
    {
        // 드래그를 더이상 하지 않으면
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        // Vector2 의 위치를 zero로 만든다.
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }


    /// <summary>
    /// 부모노드와 자식노드 사이에 연결선을 그린다
    /// </summary>
    private void DrawRoomConnections()
    {
        // 모든 룸노드 반복
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if(roomNode.childRoomNodeIDList.Count > 0)
            {
                // 자식 룸노드 반복
                foreach(string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    // 현재 자식 룸노드 키가 사전에 존재하는지 확인
                    if(currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        // 현재 방 노드와 자식 방노드 사이에 선 연결
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 부모노드와 자식노드 사이에 연결선을 그린다
    /// </summary>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        // 선의 시작점과 도착점
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        // 중간점 계산
        Vector2 midPosition = (endPosition + startPosition) / 2f;

        // 시작점부터 도착점 까지의 방향 벡터 계산
        Vector2 direction = endPosition - startPosition;

        // 중간점에서 정규화된 법선벡터 계산
        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        // 화살표의 머리방향 계산
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        // 화살표 그리기
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

        // 선 그리기
        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// 에디터 창에 룸 노드를 그림
    /// </summary>
    private void DrawRoomNodes()
    {
        // 모든 룸 노드 탐색
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
    /// 편집기에서 룸 노드 그래프 선택이 변경될 경우
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
