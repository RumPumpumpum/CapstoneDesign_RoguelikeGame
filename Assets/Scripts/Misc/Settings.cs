using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region DUNGEON BUILD SETTING

    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;

    #endregion

    #region ROOM SETTINGS

    // 한 방에 연결 가능한 복도 최대 개수
    public const int maxChildCorridors = 3;
    #endregion
}
