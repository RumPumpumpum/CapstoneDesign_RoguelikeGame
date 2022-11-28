using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS

    [Space(10)]
    [Header("BASIC LEVEL DETAILS")]

    #endregion Header BASIC LEVEL DETAILS

    #region Tooltip

    [Tooltip("레벨의 이름")]

    #endregion Tooltip

    public string levelName;

    #region Header ROOM TEMPLATES FOR LEVEL

    [Space(10)]
    [Header("레벨별 룸 템플릿")]

    #endregion Header ROOM TEMPLATES FOR LEVEL

    #region Tooltip

    [Tooltip("레벨에 포함하려는 룸 템플릿으로 목록을 채운다.")]

    #endregion Tooltip

    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR LEVEL
    [Space(10)]
    [Header("레벨별 룸 노드 그래프")]
    #endregion Header ROOM NODE GRAPHS FOR LEVEL

    #region Tooltip
    [Tooltip("원하는 레벨의 룸 노드 그래프를 모두 넣어주세요")]
    #endregion Tooltip

    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation

#if UNITY_EDITOR

    // Validate scriptable object details enetered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        // 노드 그래프에서 모든 노드 유형에 대해 룸 템플릿이 지정 되었는지 확인
        // 동,서 남,북 복도 및 시작방 유형이 지정되었는지 확인
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        // 모든 룸 템플릿 반복
        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if (roomTemplateSO.roomNodeType.isCorridorEW)
                isEWCorridor = true;

            if (roomTemplateSO.roomNodeType.isCorridorNS)
                isNSCorridor = true;

            if (roomTemplateSO.roomNodeType.isEntrance)
                isEntrance = true;
        }

        if (isEWCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : 지정된 동/서 방향 복도가 없습니다.");
        }

        if (isNSCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : 지정된 남/북 방향 복도가 없습니다.");
        }

        if (isEntrance == false)
        {
            Debug.Log("In " + this.name.ToString() + " : 지정된 시작방이 없습니다.");
        }

        // 모든 노드 그래프 반복
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;

            // 노드 그래프의 모든 노드 반복
            foreach (RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null)
                    continue;

                // 각 룸 노드에 대해 룸 템플릿이 지정 되었는지 확인

                // 복도와 입구가 문제 없으면
                if (roomNodeSO.roomNodeType.isEntrance || 
                    roomNodeSO.roomNodeType.isCorridorEW || 
                    roomNodeSO.roomNodeType.isCorridorNS || 
                    roomNodeSO.roomNodeType.isCorridor || 
                    roomNodeSO.roomNodeType.isNone)
                    continue;

                bool isRoomNodeTypeFound = false;

                // 모든 룸 템플릿 반복
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {

                    if (roomTemplateSO == null)
                        continue;

                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }

                }

                if (!isRoomNodeTypeFound)
                    Debug.Log(this.name.ToString() + " : 룸 템플릿을 넣어주세요, " + "방 타입:" + roomNodeSO.roomNodeType.name.ToString() + " 노드그래프: " + roomNodeGraph.name.ToString());


            }
        }
    }

#endif

    #endregion Validation

}
