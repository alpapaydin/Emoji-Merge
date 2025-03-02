using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    private BaseWinConditionData winCondition;
    private WinProgressData progressData = new WinProgressData();
    
    public void InitializeWinCondition(RandomLevelData levelData)
    {
        winCondition = levelData.winCondition;
        
        progressData = new WinProgressData();
        
        if (winCondition is UnblockCellsWincondition unblockCellsWinCondition)
        {
            unblockCellsWinCondition.CalculateDynamicCellCount(
                GridManager.Instance.GridSize, 
                levelData.unblockCenter
            );
        }
        
        GameManager.Instance.OnGoldEarned += OnGoldEarned;
        OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
        GridManager.Instance.OnCellUnblocked += OnCellUnblocked;
        
        UpdateLevelProgressUI();
        
        UIManager.Instance?.WinCondition?.ShowWinCondition(winCondition);
    }

    private void OnGoldEarned(int amount)
    {
        progressData.goldEarned += amount;
        CheckWinCondition();
    }

    private void OnOrderCompleted(Order order)
    {
        progressData.ordersCompleted++;
        CheckWinCondition();
    }

    private void OnCellUnblocked(Vector2Int position)
    {
        progressData.cellsUnblocked++;
        CheckWinCondition();
    }
    
    private void CheckWinCondition()
    {
        if (winCondition == null) return;

        if (winCondition.IsCompleted(progressData))
        {
            OnWin();
        }
        
        UpdateLevelProgressUI();
    }
    
    private void UpdateLevelProgressUI()
    {
        if (winCondition == null) return;
        
        int current = winCondition.GetCurrentProgress(progressData);
        int required = winCondition.GetRequiredProgress();
        
        UIManager.Instance.Stats.UpdateLevelProgress(current, required);
    }

    private void OnWin()
    {
        GameManager.Instance.OnGoldEarned -= OnGoldEarned;
        OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        GridManager.Instance.OnCellUnblocked -= OnCellUnblocked;
        
        GameManager.Instance.WinGame();
    }
}
