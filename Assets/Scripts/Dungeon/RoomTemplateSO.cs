using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid; // 고유ID

    #region 방 프리팹 헤더
    [Space(10)]
    [Header("방 프리팹")]

    #endregion 방 프리팹 헤더

    #region Tooltip

    [Tooltip("방을 위한 오브젝트 프리팹")]

    #endregion Tooltip

    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab; // SO 가 복사되고 프리팹이 변경되는 경우 guid 재생성

    #region 방 구성 헤더

    [Space(10)]
    [Header("방 구성")]

    #endregion 방 구성 헤더

    #region Tooltip

    [Tooltip("룸 노드의 종류는 룸 노드 그래프에서 만든 룸 노드에 해당된다.(복도는 예외)" +
        "룸 노드 그래프에는 Corridor 종류만 있지만 룸 템플릿에는 CorridorNS와 CorridorEW가 존재한다.")]

    #endregion Tooltip

    public RoomNodeTypeSO roomNodeType;

    #region Tooltip

    [Tooltip("방 타일맵 주위를 둘러싸는 직사각형이 있다면 방의 하한은 해당 직사각형의 왼쪽 하단 모서리를 나타낸다." +
        "이것은 타일맵에 대해 결정되어야 한다(타일맵 그리드 위치를 얻기 위해 좌표 브러시 포인터 사용)")]
    #endregion Tooltip

    public Vector2Int lowerBounds;

    #region Tooltip

    [Tooltip("방 타일맵 주위를 둘러싸는 직사각형이 있다면 방의 상한은 해당 직사각형의 오른쪽 상단 모서리를 나타낸다." +
        "이것은 타일맵에 대해 결정되어야 한다(타일맵 그리드 위치를 얻기 위해 좌표 브러시 포인터 사용)")]
    #endregion Tooltip

    public Vector2Int upperBounds;

    #region Tooltip

    [Tooltip("하나의 방에 서로 다른 방향의 4개의 출입구가 있어야 한다." +
        "각 출입구에는 크기가 일관된 3개의 타일이 열려있어야 한다")]

    #endregion Tooltip

    [SerializeField] public List<Doorway> doorwayList;

    #region Tooltip

    [Tooltip("타일맵 좌표에서 스폰위치(적과 상자 스폰에 이용)를 배열에 추가해야 함")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;

    /// <summary>
    /// 방 템플릿 에서 입구 목록을 반환
    /// </summary>
    /// <returns></returns>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region 유효성 검사
#if UNITY_EDITOR
    // SO fields 유효성 검사
    private void OnValidate()
    {
        // guid가 비어있거나 예전 프리팹이 현재 프리팹과 같지 않은 경우(프리팹이 변경) guid 재설정
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        //스폰 포지션 검사
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);


    }

#endif
    #endregion 유효성 검사

}
