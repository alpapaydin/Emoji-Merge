using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomLevelData", menuName = "Game/Levels/Random Level Data")]
public class RandomLevelData : ScriptableObject
{
    [Header("Level Data")]
    public Vector2Int MinGridSize;
    public Vector2Int MaxGridSize;
    public ProducerItemProperties[] ProducerProperties;
    public float ProducerProbability;
    public ProducedItemProperties[] ProducedItemProperties;
    public float ProducedItemProbability;
    public ChestItemProperties[] ChestProperties;
    public float ChestProbability;
}
