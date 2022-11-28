using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Dungeon Level

    [Space(10)]
    [Header("���� ����")]

    #endregion Dungeon Level

    #region Tooltip

    [Tooltip("���� ���� ������Ʈ�� �־��ּ���")]

    #endregion Tooltip

    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip

    [Tooltip("�׽�Ʈ�� ���� ������ ���������� �־��ּ��� [�⺻ ���� = 0]")]

    #endregion Tooltip

    [SerializeField] private int currentDungeonLevelListIndex = 0;

    [HideInInspector] public GameState gameState;

    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    private void Update()
    {
        HandleGameState();

        // �׽�Ʈ �� �� ���, ������ ������ �����
        if(Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }
    }

    /// <summary>
    /// ���� ���¸� �ٷ��
    /// </summary>
    public void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:

                // ������ �����ϸ� �� ����
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                break;
        }

    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        bool dungeonBuiltSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

        if(!dungeonBuiltSuccessfully)
        {
            Debug.LogError("������ ��� ��� �׷����� ������ ���� �� �����ϴ�");
        }
    }

    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }

#endif

    #endregion Validation
}
