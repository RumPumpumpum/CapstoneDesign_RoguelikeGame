using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid; // ����ID

    #region �� ������ ���
    [Space(10)]
    [Header("�� ������")]

    #endregion �� ������ ���

    #region Tooltip

    [Tooltip("���� ���� ������Ʈ ������")]

    #endregion Tooltip

    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab; // SO �� ����ǰ� �������� ����Ǵ� ��� guid �����

    #region �� ���� ���

    [Space(10)]
    [Header("�� ����")]

    #endregion �� ���� ���

    #region Tooltip

    [Tooltip("�� ����� ������ �� ��� �׷������� ���� �� ��忡 �ش�ȴ�.(������ ����)" +
        "�� ��� �׷������� Corridor ������ ������ �� ���ø����� CorridorNS�� CorridorEW�� �����Ѵ�.")]

    #endregion Tooltip

    public RoomNodeTypeSO roomNodeType;

    #region Tooltip

    [Tooltip("�� Ÿ�ϸ� ������ �ѷ��δ� ���簢���� �ִٸ� ���� ������ �ش� ���簢���� ���� �ϴ� �𼭸��� ��Ÿ����." +
        "�̰��� Ÿ�ϸʿ� ���� �����Ǿ�� �Ѵ�(Ÿ�ϸ� �׸��� ��ġ�� ��� ���� ��ǥ �귯�� ������ ���)")]
    #endregion Tooltip

    public Vector2Int lowerBounds;

    #region Tooltip

    [Tooltip("�� Ÿ�ϸ� ������ �ѷ��δ� ���簢���� �ִٸ� ���� ������ �ش� ���簢���� ������ ��� �𼭸��� ��Ÿ����." +
        "�̰��� Ÿ�ϸʿ� ���� �����Ǿ�� �Ѵ�(Ÿ�ϸ� �׸��� ��ġ�� ��� ���� ��ǥ �귯�� ������ ���)")]
    #endregion Tooltip

    public Vector2Int upperBounds;

    #region Tooltip

    [Tooltip("�ϳ��� �濡 ���� �ٸ� ������ 4���� ���Ա��� �־�� �Ѵ�." +
        "�� ���Ա����� ũ�Ⱑ �ϰ��� 3���� Ÿ���� �����־�� �Ѵ�")]

    #endregion Tooltip

    [SerializeField] public List<Doorway> doorwayList;

    #region Tooltip

    [Tooltip("Ÿ�ϸ� ��ǥ���� ������ġ(���� ���� ������ �̿�)�� �迭�� �߰��ؾ� ��")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;

    /// <summary>
    /// �� ���ø� ���� �Ա� ����� ��ȯ
    /// </summary>
    /// <returns></returns>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region ��ȿ�� �˻�
#if UNITY_EDITOR
    // SO fields ��ȿ�� �˻�
    private void OnValidate()
    {
        // guid�� ����ְų� ���� �������� ���� �����հ� ���� ���� ���(�������� ����) guid �缳��
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }

        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);

        //���� ������ �˻�
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);


    }

#endif
    #endregion ��ȿ�� �˻�

}
