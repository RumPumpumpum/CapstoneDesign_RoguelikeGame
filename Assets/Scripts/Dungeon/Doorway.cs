using UnityEngine;
[System.Serializable]

public class Doorway
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;

    #region Header
    [Header("좌상단의 위치를 복사한다")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;
    #region
    [Header("복사할 출입구의 타일 너비")]
    #endregion
    public int doorwayCopyTileWidth;
    #region
    [Header("복사할 출입구의 타일 높이")]
    #endregion
    public int doorwayCopyTileHeight;

    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool isUnavailable = false;
}
