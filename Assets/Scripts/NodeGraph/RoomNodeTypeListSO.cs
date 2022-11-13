using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeListSO", menuName = "Scriptable Objects/Dungeon/Room Node Type List")]


public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header
    [Space(10)]
    [Header("�� ��� Ÿ�� ����Ʈ")]
    #endregion
    #region Tooltip
    [Tooltip("�� ����� RoomNodeTypeSO ��ü�� ä������ �մϴ�. - Enum ��� ���˴ϴ�")]
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
