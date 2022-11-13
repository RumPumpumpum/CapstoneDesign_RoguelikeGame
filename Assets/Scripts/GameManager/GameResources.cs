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
                // 인스턴스가 null인지 확인하고 gameResource 유형의 개체를 인스턴스에 로드
            }
            return instance;
        }
    }

    #region Header
    [Space(10)]
    [Header("던전")]
    #endregion
    #region Tooltip
    [Tooltip("던전을 RoomNodeTypeListSO 로 채우기")]
    #endregion

    public RoomNodeTypeListSO roomNodeTypeList;
}
