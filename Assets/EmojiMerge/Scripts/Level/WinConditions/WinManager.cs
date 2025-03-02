using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    private BaseWinConditionData winCondition;
    private int goldEarned = 0;
    private int ordersCompleted = 0;
    private int cellsUnblocked = 0;

    public void InitializeWinCondition(RandomLevelData levelData)
    {
        winCondition = levelData.winCondition;
        GameManager.Instance.OnGoldEarned += OnGoldEarned;
        OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
        GridManager.Instance.OnCellUnblocked += OnCellUnblocked;
        
        InitializeSpecificWinCondition(levelData);
    }

    private void OnGoldEarned(int amount)
    {
        goldEarned += amount;
        CheckWinCondition();
    }

    private void OnOrderCompleted(Order order)
    {
        ordersCompleted++;
        CheckWinCondition();
    }

    private void OnCellUnblocked(Vector2Int position)
    {
        cellsUnblocked++;
        CheckWinCondition();
    }

    private void InitializeSpecificWinCondition(RandomLevelData levelData)
    {
        winCondition = levelData.winCondition;
        if (winCondition is GetGoldWinCondition goldWinCondition)
        {
            goldEarned = 0;
        }
        else if (winCondition is CompleteOrdersWinCondition ordersWinCondition)
        {
            ordersCompleted = 0;
        }
        else if (winCondition is UnblockCellsWincondition unblockCellsWinCondition)
        {
            cellsUnblocked = 0;
            if (unblockCellsWinCondition.cellsToUnblock == 0)
            {
                unblockCellsWinCondition.cellsToUnblock = GridManager.Instance.GridSize.x * GridManager.Instance.GridSize.y - levelData.unblockCenter.x * levelData.unblockCenter.y;
                print(GridManager.Instance.GridSize);
            }
        }
    }

    private void CheckWinCondition()
    {
        if (winCondition != null)
        {
            if (winCondition is GetGoldWinCondition goldWinCondition)
            {
                if (goldEarned >= goldWinCondition.goldNeeded)
                {
                    OnWin();
                }
            }
            else if (winCondition is CompleteOrdersWinCondition ordersWinCondition)
            {
                if (ordersCompleted >= ordersWinCondition.ordersToComplete)
                {
                    OnWin();
                }
            }
            else if (winCondition is UnblockCellsWincondition unblockCellsWinCondition)
            {
                if (cellsUnblocked >= unblockCellsWinCondition.cellsToUnblock)
                {
                    print(unblockCellsWinCondition.cellsToUnblock);
                    OnWin();
                }
            }
        }
    }

    private void OnWin()
    {
        GameManager.Instance.OnGoldEarned -= OnGoldEarned;
        OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        GridManager.Instance.OnCellUnblocked -= OnCellUnblocked;
        GameManager.Instance.WinGame();
    }
}
