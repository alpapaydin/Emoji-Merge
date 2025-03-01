using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    private BaseWinConditionData winCondition;
    private int goldEarned = 0;
    private int ordersCompleted = 0;

    public void InitializeWinCondition(RandomLevelData levelData)
    {
        winCondition = levelData.winCondition;
        GameManager.Instance.OnGoldEarned += OnGoldEarned;
        OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
        goldEarned = 0;
        ordersCompleted = 0;
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
        }
    }

    private void OnWin()
    {
        GameManager.Instance.OnGoldEarned -= OnGoldEarned;
        OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        GameManager.Instance.WinGame();
    }
}
