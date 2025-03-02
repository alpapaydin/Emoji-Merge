using UnityEngine;

public abstract class BaseWinConditionData : ScriptableObject
{
    public abstract bool IsCompleted(WinProgressData progressData);
    public abstract void UpdateProgress(WinProgressData progressData);
    public abstract int GetCurrentProgress(WinProgressData progressData);
    public abstract int GetRequiredProgress();
    
    public virtual string GetWinConditionText()
    {
        return $"Complete the objective to win!";
    }
}