using UnityEngine;
[System.Serializable]

public class Doorway
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;

    #region Header
    [Header("�»���� ��ġ�� �����Ѵ�")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;
    #region
    [Header("������ ���Ա��� Ÿ�� �ʺ�")]
    #endregion
    public int doorwayCopyTileWidth;
    #region
    [Header("������ ���Ա��� Ÿ�� ����")]
    #endregion
    public int doorwayCopyTileHeight;

    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool isUnavailable = false;
}
