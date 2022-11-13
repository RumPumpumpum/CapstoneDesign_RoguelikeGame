using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeListSO", menuName = "Scriptable Objects/Dungeon/Room Node Type List")]


public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header
    [Space(10)]
    [Header("룸 노드 타입 리스트")]
    #endregion
    #region Tooltip
    [Tooltip("이 목록은 RoomNodeTypeSO 개체로 채워져야 합니다. - Enum 대신 사용됩니다")]
    #endregion
    public List<RoomNodeTypeSO> list;
    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.VaildateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion
}
