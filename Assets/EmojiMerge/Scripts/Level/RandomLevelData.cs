using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomLevelData", menuName = "Game/Levels/Random Level Data")]
public class RandomLevelData : ScriptableObject
{
    [Header("Win Condition")]
    public BaseWinConditionData winCondition;
    
    [Header("Grid Data")]
    public Vector2Int MinGridSize;
    public Vector2Int MaxGridSize;

    [Header("Items Config")]
    public ProducerItemProperties[] ProducerProperties;
    public float ProducerProbability;
    public ProducedItemProperties[] ProducedItemProperties;
    public float ProducedItemProbability;
    public ChestItemProperties[] ChestProperties;
    public float ChestProbability;

    [Header("Orders Config")]
    public OrderData[] possibleOrders;
    public int maxOrders = 3;
    public float orderCooldown = 5f;
}
