using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


[DisallowMultipleComponent]

public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        // 룸 노드 타입 리스트를 불러옴
        LoadRoomNodeTypeList();

    }

    /// <summary>
    /// 룸 노트 타입 리스트를 불러옴
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// 랜덤 던전을 생성, 성공시 ture 실패시 false 반환
    /// </summary>
    /// <param name="currentDungeonLevel"></param>
    /// <returns></returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // 사전에서 룸 템플릿을 불러옴
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        // 던전 생성이 실패했고, 현재 던전 생성 시도 횟수가 최대 시도 횟수보다 적으면 다시 시도
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // 목록에서 룸 노드 그래프를 랜덤으로 가져온다.
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // 던전이 성공적으로 생성 되거나 최대 던전 재생성 횟수를 넘지 않을 동안 반복
            while(!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                // 던전 룸 게임 오브젝트와 던전 룸 사전을 초기화
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // 선택된 룸 노드 그래프에 대하여 랜덤 던전 생성 시도
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }

            // 던전이 성공적으로 구축되었으면 룸 게임오브젝트를 인스턴트화
            if (dungeonBuildSuccessful)
                {
                    InstantiateRoomGameobjects();
                }
        }

        return dungeonBuildSuccessful;
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        // 룸 템플릿 사전을 초기화
        roomTemplateDictionary.Clear();

        // 사전에서 룸 템플릿 리스트를 불러옴
        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // 사전에 이 방 템플릿에 대한 포함되어 있지 않으면 생성
            if(!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            // 사전에 이 방 템플릿이 포함되어 있으면 중복경고
            else
            {
                Debug.Log("중복된 룸 템플릿 키를 발견하였습니다 : ");
            }
        }
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // 입구방 노드 얻기
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if(entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }

        else
        {
            Debug.Log("입구방 노드가 없습니다.");
            return false; // 던전 생성 실패
        }

        // 겹치는 방이 없으면 시작
        bool noRoomOverlaps = true;

        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // 대기열의 모든 노드를 처리했고 겹치는 방이 없으면 true , 겹치는 방이 존재하면 false 반환
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        // 처리해야 할 룸 노드가 있고 겹치는 룸이 없으면
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // 룸 노드 대기열에서 다음 룸 노드를 얻는다
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // 부모노드와 연결 된 자식노드를 대기열에 추가한다
            foreach(RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance) // 입구인 경우
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                // 룸 사전에 룸 추가
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }

            else // 입구가 아닌 경우
            {
                // 부모방을 얻는다
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // 겹치지 않고 방을 배치 할 수 있는지 확인한다
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }

        }

        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // 겹치는 부분이 없다는 것을 증명하기 전 까지 겹친다고 가정
        bool roomOverlaps = true;
        
        // 겹친다면 겹치는 부분이 없을때 까지 다시 시도
        while(roomOverlaps)
        {
            // 연결되지 않은 출입구를 랜덤으로 선택한다
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            // 더이상 시도할 출입구가 없으면 오버랩 판정
            if(unconnectedAvailableParentDoorways.Count == 0)
            {
                return false; // 방이 겹침
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            // 부모의 출입구 방향과 일치하는 룸 노드에 대한 룸 템플릿을 무작위로 가져옴
            // 부모의 출입구가 북쪽이면 자식은 남쪽이니 NScorridor 를 골라야 함
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            // 방 생성
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            // 겹치는 부분 없이 방을 배치
            if(PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                // 방이 겹치지 않으면 false로 설정 후 while문 탈출
                roomOverlaps = false;

                // 배치됐다고 표시
                room.isPositioned = true;

                //사전에 방 추가
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }

            else
            {
                roomOverlaps = true;
            }
        }

        return true;
    }

    /// <summary>
    /// 부모의 출입구 방향을 고려해 룸 노드에 대한 무작위 룸 템플릿을 가져옴
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="doorwayParent"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;

        // 룸 노드가 복도면 부모의 출입구 방향과 맞는 복도를 랜덤으로 고른다
        if(roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomtemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }

        // 아니라면 임의의 룸 템플릿을 선택한다
        else
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomtemplate;
    }

    /// <summary>
    /// 겹치치 않게 방 배치
    /// </summary>
    /// <param name="parentRoom"></param>
    /// <param name="doorwayParent"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // 현재 방의 출입구 위치를 얻음
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);
        
        // 부모 출입구 반대편 방에 출입구가 없으면
        if(doorway == null)
        {
            // 사용할 수 없는 부모 출입구로 표시, 다시 연결을 시도하지 않음
            doorwayParent.isUnavailable = true;

            return false;
        }

        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;
        // 이 조정은 연결 하려는 출입구 위치를 기반으로 함(부모와 어울리도록 배치)
        // ex) 출입구가 북쪽에 있으면 출입구 좌표에 y축으로 -1 를 더하면 자식방 출입구가 된다
        // ParentDoorwayPosition + Adjustment = Child Room Node 

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }

        // 방의 상한과 하한의 계산은 부모 출입구를 조정한 위치를 기반으로 한다
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForOverlap(room);

        // 방끼리 서로 겹치지 않는다면 true 반환
        if (overlappingRoom == null)
        {
            // 출입구가 연결 되어있고 사용할 수 없다고 표시
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }

        else
        {
            // 재시도 하지 않을것이므로 사용할 수 없음만 표시
            doorwayParent.isUnavailable = true;

            return false;
        }
    }

    /// <summary>
    /// 출입구 목록에서 출입구를 얻는다
    /// </summary>
    /// <param name="parentDoorway"></param>
    /// <param name="doorwayList"></param>
    /// <returns></returns>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach(Doorway doorwayToCheck in doorwayList)
        {
            // 부모의 복도 방향이 동쪽이고 확인하고 있는 출입구가 서쪽이면 
            if(parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                // 확인하고 있던 출입구 반환
                return doorwayToCheck;
            }
            
            else if(parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
               return doorwayToCheck;
            }

            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }

            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }

        return null;
    }

    /// <summary>
    /// 상한, 하한 매개번수를 이용해 겹치는 방을 찾고 있는경우 방을 반환, 없는경우 null 반환
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <returns></returns>
    private Room CheckForOverlap(Room roomToTest)
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // 배치 하려는 방이나 아직 배치되지 않은 방은 스킵
            if(room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            // 방이 겹쳤으면
            if(IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// 2개의 방이 서로 겹치는지 체크
    /// </summary>
    /// <param name="room1"></param>
    /// <param name="room2"></param>
    /// <returns></returns>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverLappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);

        bool isOverLappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if(isOverLappingX && isOverLappingY)
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    /// <summary>
    /// 첫번째 방이 두번째 방과 간격이 겹치는지 체크
    /// </summary>
    /// <param name="imin1"></param>
    /// <param name="imax1"></param>
    /// <param name="imin2"></param>
    /// <param name="imax2"></param>
    /// <returns></returns>
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        if(Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    /// <summary>
    /// 룸 타입 리스트에서 룸 타입이 일치하는 무작위 룸 템플릿을 가져온다, 일치하는 것이 없으면 null 반환
    /// </summary>
    /// <param name="roomNodeType"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // 일치하는 룸 탬플릿 추가
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // 일치하는 유형이 없으면 null 반환
        if(matchingRoomTemplateList.Count == 0)
        {
            return null;
        }

        // 목록에서 랜덤 룸 템플릿을 고른 후 반환
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];

    }

    /// <summary>
    /// 연결 되어있지 않은 출입구를 얻는다
    /// </summary>
    /// <param name="roomDoorwayList"></param>
    /// <returns></returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        // 출입구 목록 탐색
        foreach(Doorway doorway in roomDoorwayList)
        {
            if(!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }


    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if(roomNode.parentRoomNodeIDList.Count == 0) // 입구방
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }

        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    /// <summary>
    /// 룸 노드 그래프 목록에서 랜덤한 룸 노드 그래프를 고름
    /// </summary>
    /// <param name="roomNodeGraphList"></param>
    /// <returns></returns>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }

        else
        {
            Debug.Log("목록에 룸 노드 그래프가 없습니다.");
            return null;
        }
    }

    /// <summary>
    /// doorway 리스트를 깊은복사
    /// </summary>
    /// <param name="oldDoorwayList"></param>
    /// <returns></returns>
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach(Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }

    /// <summary>
    /// string List 깊은복사
    /// </summary>
    /// <param name="oldStringList"></param>
    /// <returns></returns>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach(string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    /// <summary>
    /// 던전 룸 게임 오브젝트를 인스턴트화
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        // 모든 던전 룸 탐색
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // 방 위치 계산
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);
            // 방 인스턴트화
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);
            // 인스턴트 프리팹으로 부터 인스턴트 룸 컴포넌트를 얻는다
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;
            // 인스턴트화 된 방 initialise
            instantiatedRoom.Initialise(roomGameobject);

            room.instantiatedRoom = instantiatedRoom;
        }
    }

    /// <summary>
    /// 룸 템플릿 ID로 부터 룸 템플릿을 얻는다
    /// </summary>
    /// <param name="roomTemplateID"></param>
    /// <returns></returns>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if(roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }

        else
        {
            return null;
        }
    }

    /// <summary>
    /// 방 ID로 부터 방 값을 얻는다
    /// </summary>
    /// <param name="roomID"></param>
    /// <returns></returns>
    public Room GetRoomByRoomID(string roomID)
    {
        if(dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }

        else
        {
            return null;
        }
    }

    /// <summary>
    /// 던전 룸 게임 오브젝트와 던전 룸 사전을 초기화
    /// </summary>
    private void ClearDungeon()
    {
        if(dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluePair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluePair.Value;

                if(room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            dungeonBuilderRoomDictionary.Clear();
        }
    }



}
