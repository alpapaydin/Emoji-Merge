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
        int totalCells = gridSize.x * gridSize.y;
        
        int producerCount = 0;
        int chestCount = 0;
        int producedItemCount = 0;
        
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                availablePositions.Add(new Vector2Int(x, y));
            }
        }
        
        ShuffleList(availablePositions);
        
        if (randomLevelData.forcedSpawnItems != null && randomLevelData.forcedSpawnItems.Length > 0)
        {
            foreach (ItemLevelCount forcedItem in randomLevelData.forcedSpawnItems)
            {
                if (forcedItem.itemDefinition == null || forcedItem.count <= 0 || availablePositions.Count <= 0)
                    continue;

                for (int i = 0; i < forcedItem.count && availablePositions.Count > 0; i++)
                {
                    Vector2Int pos = availablePositions[0];
                    availablePositions.RemoveAt(0);
                    
                    if (forcedItem.itemDefinition is ProducerItemProperties producerProperties)
                    {
                        itemManager.CreateProducerItem(pos, forcedItem.level, producerProperties);
                        producerCount++;
                    }
                    else if (forcedItem.itemDefinition is ChestItemProperties chestProperties)
                    {
                        itemManager.CreateChestItem(pos, forcedItem.level, chestProperties);
                        chestCount++;
                    }
                    else if (forcedItem.itemDefinition is ProducedItemProperties producedProperties)
                    {
                        itemManager.CreateProducedItem(pos, forcedItem.level, producedProperties);
                        producedItemCount++;
                    }
                }
            }
        }
        
        for (int i = 0; i < randomLevelData.MinProducerCount && availablePositions.Count > 0; i++)
        {
            if (producerCount >= randomLevelData.MinProducerCount)
                break;
                
            Vector2Int pos = availablePositions[0];
            availablePositions.RemoveAt(0);
            
            ProducerItemProperties properties = randomLevelData.ProducerProperties[Random.Range(0, randomLevelData.ProducerProperties.Length)];
            int level = Random.Range(1, properties.maxLevel);
            itemManager.CreateProducerItem(pos, level, properties);
            producerCount++;
        }
        
        for (int i = 0; i < randomLevelData.MinChestCount && availablePositions.Count > 0; i++)
        {
            if (chestCount >= randomLevelData.MinChestCount)
                break;
                
            Vector2Int pos = availablePositions[0];
            availablePositions.RemoveAt(0);
            
            ChestItemProperties properties = randomLevelData.ChestProperties[Random.Range(0, randomLevelData.ChestProperties.Length)];
            int level = Random.Range(1, properties.maxLevel);
            itemManager.CreateChestItem(pos, level, properties);
            chestCount++;
        }
        
        while (availablePositions.Count > 0)
        {
            Vector2Int pos = availablePositions[0];
            availablePositions.RemoveAt(0);
            
            float totalProbability = randomLevelData.ProducerProbability + 
                                    randomLevelData.ProducedItemProbability + 
                                    randomLevelData.ChestProbability;
            
            if (totalProbability <= 0) continue;
            
            float producerProb = (producerCount >= randomLevelData.MaxProducerCount) ? 0 : randomLevelData.ProducerProbability;
            float chestProb = (chestCount >= randomLevelData.MaxChestCount) ? 0 : randomLevelData.ChestProbability;
            float producedItemProb = randomLevelData.ProducedItemProbability;
            
            totalProbability = producerProb + chestProb + producedItemProb;
            
            if (totalProbability <= 0) continue;
            
            float normalizedProducerProb = producerProb / totalProbability;
            float normalizedChestProb = chestProb / totalProbability;
            
            float randomValue = Random.value;
            
            if (randomValue < normalizedProducerProb && producerCount < randomLevelData.MaxProducerCount)
            {
                ProducerItemProperties properties = randomLevelData.ProducerProperties[Random.Range(0, randomLevelData.ProducerProperties.Length)];
                int level = Random.Range(1, properties.maxLevel);
                itemManager.CreateProducerItem(pos, level, properties);
                producerCount++;
            }
            else if (randomValue < normalizedProducerProb + normalizedChestProb && chestCount < randomLevelData.MaxChestCount)
            {
                ChestItemProperties properties = randomLevelData.ChestProperties[Random.Range(0, randomLevelData.ChestProperties.Length)];
                int level = Random.Range(1, properties.maxLevel);
                itemManager.CreateChestItem(pos, level, properties);
                chestCount++;
            }
            else
            {
                ProducedItemProperties properties = randomLevelData.ProducedItemProperties[Random.Range(0, randomLevelData.ProducedItemProperties.Length)];
                int level = Random.Range(1, properties.maxLevel);
                itemManager.CreateProducedItem(pos, level, properties);
                producedItemCount++;
            }
        }
    }
    
    private static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
}
