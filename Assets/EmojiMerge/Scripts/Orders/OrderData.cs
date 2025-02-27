using UnityEngine;

[CreateAssetMenu(fileName = "OrderData", menuName = "Game/Orders/Order Data")]
public class OrderData : ScriptableObject
{
    public string orderName;
    public string description;
    public ItemLevelCount[] requiredItems;
    public int goldReward;
    public float energyReward;
}