using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
                // �ν��Ͻ��� null���� Ȯ���ϰ� gameResource ������ ��ü�� �ν��Ͻ��� �ε�
            }
            return instance;
        }
    }

    #region Header
    [Space(10)]
    [Header("����")]
    #endregion
    #region Tooltip
    [Tooltip("������ RoomNodeTypeListSO �� ä���")]
    #endregion

    public RoomNodeTypeListSO roomNodeTypeList;

    #region MATERIALS

    [Space(10)]
    [Header("MATERIALS")]

    #endregion

    #region Tooltip

    [Tooltip("Dimmed Material")]

    #endregion

    public Material dimmedMaterial;
}
