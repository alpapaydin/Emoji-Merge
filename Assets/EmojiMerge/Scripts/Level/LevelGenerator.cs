using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelGenerator
{
    public static Vector2Int GetRandomGridSize(RandomLevelData randomLevelData)
    {
        return new Vector2Int(
            Random.Range(randomLevelData.MinGridSize.x, randomLevelData.MaxGridSize.x),
            Random.Range(randomLevelData.MinGridSize.y, randomLevelData.MaxGridSize.y)
        );
    }

    public static void GenerateRandomLevel(RandomLevelData randomLevelData, Vector2Int gridSize, ItemManager itemManager)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                float totalProbability = randomLevelData.ProducerProbability + 
                                        randomLevelData.ProducedItemProbability + 
                                        randomLevelData.ChestProbability;
                
                if (totalProbability <= 0) continue;
                
                float normalizedProducerProb = randomLevelData.ProducerProbability / totalProbability;
                float normalizedProducedItemProb = randomLevelData.ProducedItemProbability / totalProbability;
                float normalizedChestProb = randomLevelData.ChestProbability / totalProbability;
                
                float randomValue = Random.value;
                
                if (randomValue < normalizedProducerProb)
                {
                    ProducerItemProperties producerItemProperties = randomLevelData.ProducerProperties[Random.Range(0, randomLevelData.ProducerProperties.Length)];
                    itemManager.CreateProducerItem(new Vector2Int(x, y), Random.Range(1, producerItemProperties.maxLevel - 1));
                }
                else if (randomValue < normalizedProducerProb + normalizedProducedItemProb)
                {
                    ProducedItemProperties producedItemProperties = randomLevelData.ProducedItemProperties[Random.Range(0, randomLevelData.ProducedItemProperties.Length)];
                    itemManager.CreateProducedItem(new Vector2Int(x, y), Random.Range(1, producedItemProperties.maxLevel - 1));
                }
                else
                {
                    ChestItemProperties chestItemProperties = randomLevelData.ChestProperties[Random.Range(0, randomLevelData.ChestProperties.Length)];
                    itemManager.CreateChestItem(new Vector2Int(x, y), Random.Range(1, chestItemProperties.maxLevel - 1));
                }
            }
        }
    }
}
