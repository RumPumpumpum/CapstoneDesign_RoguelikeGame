using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]

public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;
    #region
    [Header("�����⿡ ǥ���Ϸ��� �� ���� �÷��� ����")]
    #endregion
    public bool displayInNodeGraphEditor = true;
    #region
    [Header("�� ���� ����")]
    #endregion
    public bool isCorridor; // ������ ������ �˰����� ����
    public bool isCorridorNS;
    public bool isCorridorEW;
    public bool isEntrance; // �Ա�) ù��° ��带 ������ �� �⺻������ ����
    public bool isBossRoom; // ������
    public bool isNone; // ����Ʈ

    // ����Ƽ �����Ϳ����� ����� #if #endif
    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// ��ũ��Ʈ�� �ε�ǰų� ���� ����� �� ����Ƽ�� ȣ���ϴ� ������ ���� ���.
    /// �ν������� ��ȭ�� �����ϴ� �� ���, Scriptable ��ü�� ���� ������Ʈ�ϸ� �� �޼��� ȣ��
    /// </summary>
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
