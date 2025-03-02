using UnityEngine;

[CreateAssetMenu(fileName = "UnblockCellsWincondition", menuName = "Game/Levels/Wins/Unblock Cells Win Condition")]
public class UnblockCellsWincondition : BaseWinConditionData
{
    public int cellsToUnblock;
    
    [System.NonSerialized]
    private int calculatedCellsToUnblock = -1;
    
    public void CalculateDynamicCellCount(Vector2Int gridSize, Vector2Int unblockCenter)
    {
        if (cellsToUnblock <= 0)
        {
            calculatedCellsToUnblock = gridSize.x * gridSize.y - unblockCenter.x * unblockCenter.y;
            Debug.Log($"Calculated cells to unblock: {calculatedCellsToUnblock}");
        }
        else
        {
            calculatedCellsToUnblock = cellsToUnblock;
        }
    }
    
    public int GetEffectiveCellCount()
    {
        if (cellsToUnblock > 0)
            return cellsToUnblock;
            
        if (calculatedCellsToUnblock >= 0)
            return calculatedCellsToUnblock;
            
        return 0;
    }
    
    public override bool IsCompleted(WinProgressData progressData)
    {
        int requiredCells = GetEffectiveCellCount();
        return progressData.cellsUnblocked >= requiredCells;
    }
    
    public override void UpdateProgress(WinProgressData progressData)
    {
    }
    
    public override int GetCurrentProgress(WinProgressData progressData)
    {
        return progressData.cellsUnblocked;
    }
    
    public override int GetRequiredProgress()
    {
        return GetEffectiveCellCount();
    }
    
    public override string GetWinConditionText()
    {
        int effectiveCount = GetEffectiveCellCount();
        
        if (effectiveCount <= 0)
        {
            return "Unblock all cells to win!";
        }
        
        string cellText = effectiveCount == 1 ? "cell" : "cells";
        return $"Unblock {effectiveCount} {cellText} to win!";
    }
}