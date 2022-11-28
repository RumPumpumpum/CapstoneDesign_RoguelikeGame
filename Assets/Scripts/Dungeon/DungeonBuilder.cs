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

        // �� ��� Ÿ�� ����Ʈ�� �ҷ���
        LoadRoomNodeTypeList();

    }

    /// <summary>
    /// �� ��Ʈ Ÿ�� ����Ʈ�� �ҷ���
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// ���� ������ ����, ������ ture ���н� false ��ȯ
    /// </summary>
    /// <param name="currentDungeonLevel"></param>
    /// <returns></returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // �������� �� ���ø��� �ҷ���
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        // ���� ������ �����߰�, ���� ���� ���� �õ� Ƚ���� �ִ� �õ� Ƚ������ ������ �ٽ� �õ�
        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // ��Ͽ��� �� ��� �׷����� �������� �����´�.
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // ������ ���������� ���� �ǰų� �ִ� ���� ����� Ƚ���� ���� ���� ���� �ݺ�
            while(!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                // ���� �� ���� ������Ʈ�� ���� �� ������ �ʱ�ȭ
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // ���õ� �� ��� �׷����� ���Ͽ� ���� ���� ���� �õ�
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }

            // ������ ���������� ����Ǿ����� �� ���ӿ�����Ʈ�� �ν���Ʈȭ
            if (dungeonBuildSuccessful)
                {
                    InstantiateRoomGameobjects();
                }
        }

        return dungeonBuildSuccessful;
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        // �� ���ø� ������ �ʱ�ȭ
        roomTemplateDictionary.Clear();

        // �������� �� ���ø� ����Ʈ�� �ҷ���
        foreach(RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // ������ �� �� ���ø��� ���� ���ԵǾ� ���� ������ ����
            if(!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            // ������ �� �� ���ø��� ���ԵǾ� ������ �ߺ����
            else
            {
                Debug.Log("�ߺ��� �� ���ø� Ű�� �߰��Ͽ����ϴ� : ");
            }
        }
    }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // �Ա��� ��� ���
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if(entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }

        else
        {
            Debug.Log("�Ա��� ��尡 �����ϴ�.");
            return false; // ���� ���� ����
        }

        // ��ġ�� ���� ������ ����
        bool noRoomOverlaps = true;

        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // ��⿭�� ��� ��带 ó���߰� ��ġ�� ���� ������ true , ��ġ�� ���� �����ϸ� false ��ȯ
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
        // ó���ؾ� �� �� ��尡 �ְ� ��ġ�� ���� ������
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // �� ��� ��⿭���� ���� �� ��带 ��´�
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // �θ���� ���� �� �ڽĳ�带 ��⿭�� �߰��Ѵ�
            foreach(RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance) // �Ա��� ���
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                // �� ������ �� �߰�
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }

            else // �Ա��� �ƴ� ���
            {
                // �θ���� ��´�
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // ��ġ�� �ʰ� ���� ��ġ �� �� �ִ��� Ȯ���Ѵ�
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }

        }

        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // ��ġ�� �κ��� ���ٴ� ���� �����ϱ� �� ���� ��ģ�ٰ� ����
        bool roomOverlaps = true;
        
        // ��ģ�ٸ� ��ġ�� �κ��� ������ ���� �ٽ� �õ�
        while(roomOverlaps)
        {
            // ������� ���� ���Ա��� �������� �����Ѵ�
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            // ���̻� �õ��� ���Ա��� ������ ������ ����
            if(unconnectedAvailableParentDoorways.Count == 0)
            {
                return false; // ���� ��ħ
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            // �θ��� ���Ա� ����� ��ġ�ϴ� �� ��忡 ���� �� ���ø��� �������� ������
            // �θ��� ���Ա��� �����̸� �ڽ��� �����̴� NScorridor �� ���� ��
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            // �� ����
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            // ��ġ�� �κ� ���� ���� ��ġ
            if(PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                // ���� ��ġ�� ������ false�� ���� �� while�� Ż��
                roomOverlaps = false;

                // ��ġ�ƴٰ� ǥ��
                room.isPositioned = true;

                //������ �� �߰�
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
    /// �θ��� ���Ա� ������ ����� �� ��忡 ���� ������ �� ���ø��� ������
    /// </summary>
    /// <param name="roomNode"></param>
    /// <param name="doorwayParent"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomtemplate = null;

        // �� ��尡 ������ �θ��� ���Ա� ����� �´� ������ �������� ����
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

        // �ƴ϶�� ������ �� ���ø��� �����Ѵ�
        else
        {
            roomtemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomtemplate;
    }

    /// <summary>
    /// ��ġġ �ʰ� �� ��ġ
    /// </summary>
    /// <param name="parentRoom"></param>
    /// <param name="doorwayParent"></param>
    /// <param name="room"></param>
    /// <returns></returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        // ���� ���� ���Ա� ��ġ�� ����
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);
        
        // �θ� ���Ա� �ݴ��� �濡 ���Ա��� ������
        if(doorway == null)
        {
            // ����� �� ���� �θ� ���Ա��� ǥ��, �ٽ� ������ �õ����� ����
            doorwayParent.isUnavailable = true;

            return false;
        }

        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;
        // �� ������ ���� �Ϸ��� ���Ա� ��ġ�� ������� ��(�θ�� ��︮���� ��ġ)
        // ex) ���Ա��� ���ʿ� ������ ���Ա� ��ǥ�� y������ -1 �� ���ϸ� �ڽĹ� ���Ա��� �ȴ�
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

        // ���� ���Ѱ� ������ ����� �θ� ���Ա��� ������ ��ġ�� ������� �Ѵ�
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForOverlap(room);

        // �波�� ���� ��ġ�� �ʴ´ٸ� true ��ȯ
        if (overlappingRoom == null)
        {
            // ���Ա��� ���� �Ǿ��ְ� ����� �� ���ٰ� ǥ��
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            return true;
        }

        else
        {
            // ��õ� ���� �������̹Ƿ� ����� �� ������ ǥ��
            doorwayParent.isUnavailable = true;

            return false;
        }
    }

    /// <summary>
    /// ���Ա� ��Ͽ��� ���Ա��� ��´�
    /// </summary>
    /// <param name="parentDoorway"></param>
    /// <param name="doorwayList"></param>
    /// <returns></returns>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach(Doorway doorwayToCheck in doorwayList)
        {
            // �θ��� ���� ������ �����̰� Ȯ���ϰ� �ִ� ���Ա��� �����̸� 
            if(parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                // Ȯ���ϰ� �ִ� ���Ա� ��ȯ
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
    /// ����, ���� �Ű������� �̿��� ��ġ�� ���� ã�� �ִ°�� ���� ��ȯ, ���°�� null ��ȯ
    /// </summary>
    /// <param name="roomToTest"></param>
    /// <returns></returns>
    private Room CheckForOverlap(Room roomToTest)
    {
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // ��ġ �Ϸ��� ���̳� ���� ��ġ���� ���� ���� ��ŵ
            if(room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }

            // ���� ��������
            if(IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }

    /// <summary>
    /// 2���� ���� ���� ��ġ���� üũ
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
    /// ù��° ���� �ι�° ��� ������ ��ġ���� üũ
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
    /// �� Ÿ�� ����Ʈ���� �� Ÿ���� ��ġ�ϴ� ������ �� ���ø��� �����´�, ��ġ�ϴ� ���� ������ null ��ȯ
    /// </summary>
    /// <param name="roomNodeType"></param>
    /// <returns></returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // ��ġ�ϴ� �� ���ø� �߰�
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // ��ġ�ϴ� ������ ������ null ��ȯ
        if(matchingRoomTemplateList.Count == 0)
        {
            return null;
        }

        // ��Ͽ��� ���� �� ���ø��� �� �� ��ȯ
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];

    }

    /// <summary>
    /// ���� �Ǿ����� ���� ���Ա��� ��´�
    /// </summary>
    /// <param name="roomDoorwayList"></param>
    /// <returns></returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        // ���Ա� ��� Ž��
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

        if(roomNode.parentRoomNodeIDList.Count == 0) // �Ա���
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
    /// �� ��� �׷��� ��Ͽ��� ������ �� ��� �׷����� ��
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
            Debug.Log("��Ͽ� �� ��� �׷����� �����ϴ�.");
            return null;
        }
    }

    /// <summary>
    /// doorway ����Ʈ�� ��������
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
    /// string List ��������
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
    /// ���� �� ���� ������Ʈ�� �ν���Ʈȭ
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        // ��� ���� �� Ž��
        foreach(KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // �� ��ġ ���
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);
            // �� �ν���Ʈȭ
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);
            // �ν���Ʈ ���������� ���� �ν���Ʈ �� ������Ʈ�� ��´�
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;
            // �ν���Ʈȭ �� �� initialise
            instantiatedRoom.Initialise(roomGameobject);

            room.instantiatedRoom = instantiatedRoom;
        }
    }

    /// <summary>
    /// �� ���ø� ID�� ���� �� ���ø��� ��´�
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
    /// �� ID�� ���� �� ���� ��´�
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
    /// ���� �� ���� ������Ʈ�� ���� �� ������ �ʱ�ȭ
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
