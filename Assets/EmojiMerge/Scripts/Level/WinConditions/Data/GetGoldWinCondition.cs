using UnityEngine;

[CreateAssetMenu(fileName = "GetGoldWinCondition", menuName = "Game/Levels/Wins/Get Gold Win Condition")]
public class GetGoldWinCondition : BaseWinConditionData
{
    public int goldNeeded;
    
    public override bool IsCompleted(WinProgressData progressData)
    {
        return progressData.goldEarned >= goldNeeded;
    }
    
    public override void UpdateProgress(WinProgressData progressData)
    {
    }
    
    public override int GetCurrentProgress(WinProgressData progressData)
    {
        return progressData.goldEarned;
    }
    
    public override int GetRequiredProgress()
    {
        return goldNeeded;
    }
    
    public override string GetWinConditionText()
    {
        return $"Get {goldNeeded} gold to win!";
    }
}