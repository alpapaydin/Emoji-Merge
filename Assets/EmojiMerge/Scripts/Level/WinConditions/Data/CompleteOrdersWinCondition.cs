using UnityEngine;

[CreateAssetMenu(fileName = "CompleteOrdersWinCondition", menuName = "Game/Levels/Wins/Complete Orders Win Condition")]
public class CompleteOrdersWinCondition : BaseWinConditionData
{
    public int ordersToComplete;
    
    public override bool IsCompleted(WinProgressData progressData)
    {
        return progressData.ordersCompleted >= ordersToComplete;
    }
    
    public override void UpdateProgress(WinProgressData progressData)
    {
    }
    
    public override int GetCurrentProgress(WinProgressData progressData)
    {
        return progressData.ordersCompleted;
    }
    
    public override int GetRequiredProgress()
    {
        return ordersToComplete;
    }
    
    public override string GetWinConditionText()
    {
        string orderText = ordersToComplete == 1 ? "order" : "orders";
        return $"Complete {ordersToComplete} {orderText} to win!";
    }
}