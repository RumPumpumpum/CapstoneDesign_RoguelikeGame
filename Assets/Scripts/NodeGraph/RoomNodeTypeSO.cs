using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]

public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;
    #region
    [Header("편집기에 표시하려고 할 때만 플래그 지정")]
    #endregion
    public bool displayInNodeGraphEditor = true;
    #region
    [Header("방 유형 선택")]
    #endregion
    public bool isCorridor; // 복도의 방향은 알고리즘이 결정
    public bool isCorridorNS;
    public bool isCorridorEW;
    public bool isEntrance; // 입구) 첫번째 노드를 생성할 때 기본적으로 생성
    public bool isBossRoom; // 보스방
    public bool isNone; // 디폴트

    // 유니티 에디터에서만 실행됨 #if #endif
    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// 스크립트가 로드되거나 값이 변경될 때 유니티가 호출하는 편집기 전용 기능.
    /// 인스펙터의 변화를 감지하는 데 사용, Scriptable 개체의 값을 업데이트하면 이 메서드 호출
    /// </summary>
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
