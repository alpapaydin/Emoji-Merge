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

    public static void GenerateRandomLevel(RandomLevelData randomLevelData, ItemManager itemManager, GridManager gridManager)
    {
        Vector2Int gridSize = gridManager.GridSize;
        
        TryBlockCells(randomLevelData, gridManager);
        
        int producerCount = 0;
        int chestCount = 0;
        int producedItemCount = 0;
        
        List<Vector2Int> availablePositions = GetPrioritizedAvailablePositions(gridSize, gridManager);
        
        PlaceForcedItems(randomLevelData, availablePositions, itemManager, ref producerCount, ref chestCount, ref producedItemCount);
        
        PlaceMinimumRequiredProducers(randomLevelData, availablePositions, itemManager, ref producerCount);
        
        PlaceMinimumRequiredChests(randomLevelData, availablePositions, itemManager, ref chestCount);
        
        FillRemainingCellsRandomly(randomLevelData, availablePositions, itemManager, ref producerCount, ref chestCount, ref producedItemCount);
    }
    
    private static List<Vector2Int> GetPrioritizedAvailablePositions(Vector2Int gridSize, GridManager gridManager)
    {
        List<Vector2Int> unblockPositions = new List<Vector2Int>();
        List<Vector2Int> blockedPositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (gridManager.IsCellBlocked(pos))
                {
                    blockedPositions.Add(pos);
                }
                else
                {
                    unblockPositions.Add(pos);
                }
            }
        }
        
        ShuffleList(unblockPositions);
        ShuffleList(blockedPositions);
        
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        availablePositions.AddRange(unblockPositions);
        availablePositions.AddRange(blockedPositions);
        
        return availablePositions;
    }
    
    private static void PlaceForcedItems(RandomLevelData randomLevelData, List<Vector2Int> availablePositions, ItemManager itemManager, 
                                        ref int producerCount, ref int chestCount, ref int producedItemCount)
    {
        if (randomLevelData.forcedSpawnItems == null || randomLevelData.forcedSpawnItems.Length <= 0)
            return;
            
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
    
    private static void PlaceMinimumRequiredProducers(RandomLevelData randomLevelData, List<Vector2Int> availablePositions, ItemManager itemManager, ref int producerCount)
    {
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
    }
    
    private static void PlaceMinimumRequiredChests(RandomLevelData randomLevelData, List<Vector2Int> availablePositions, ItemManager itemManager, ref int chestCount)
    {
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
    }
    
    private static void FillRemainingCellsRandomly(RandomLevelData randomLevelData, List<Vector2Int> availablePositions, ItemManager itemManager, 
                                                ref int producerCount, ref int chestCount, ref int producedItemCount)
    {
        while (availablePositions.Count > 0)
        {
            Vector2Int pos = availablePositions[0];
            availablePositions.RemoveAt(0);
            
            float totalProbability = CalculateTotalProbability(randomLevelData, producerCount, chestCount);
            if (totalProbability <= 0) continue;
            
            ItemType itemTypeToSpawn = DetermineItemTypeToSpawn(randomLevelData, producerCount, chestCount, totalProbability);
            
            SpawnRandomItem(itemTypeToSpawn, randomLevelData, pos, itemManager, ref producerCount, ref chestCount, ref producedItemCount);
        }
    }
    
    private static float CalculateTotalProbability(RandomLevelData randomLevelData, int producerCount, int chestCount)
    {
        float producerProb = (producerCount >= randomLevelData.MaxProducerCount) ? 0 : randomLevelData.ProducerProbability;
        float chestProb = (chestCount >= randomLevelData.MaxChestCount) ? 0 : randomLevelData.ChestProbability;
        float producedItemProb = randomLevelData.ProducedItemProbability;
        
        return producerProb + chestProb + producedItemProb;
    }
    
    private enum ItemType
    {
        Producer,
        Chest,
        ProducedItem
    }
    
    private static ItemType DetermineItemTypeToSpawn(RandomLevelData randomLevelData, int producerCount, int chestCount, float totalProbability)
    {
        float producerProb = (producerCount >= randomLevelData.MaxProducerCount) ? 0 : randomLevelData.ProducerProbability;
        float chestProb = (chestCount >= randomLevelData.MaxChestCount) ? 0 : randomLevelData.ChestProbability;
        
        float normalizedProducerProb = producerProb / totalProbability;
        float normalizedChestProb = chestProb / totalProbability;
        
        float randomValue = Random.value;
        
        if (randomValue < normalizedProducerProb && producerCount < randomLevelData.MaxProducerCount)
        {
            return ItemType.Producer;
        }
        else if (randomValue < normalizedProducerProb + normalizedChestProb && chestCount < randomLevelData.MaxChestCount)
        {
            return ItemType.Chest;
        }
        else
        {
            return ItemType.ProducedItem;
        }
    }
    
    private static void SpawnRandomItem(ItemType itemType, RandomLevelData randomLevelData, Vector2Int position, ItemManager itemManager, 
                                      ref int producerCount, ref int chestCount, ref int producedItemCount)
    {
        switch (itemType)
        {
            case ItemType.Producer:
                ProducerItemProperties producerProps = randomLevelData.ProducerProperties[Random.Range(0, randomLevelData.ProducerProperties.Length)];
                int producerLevel = Random.Range(1, producerProps.maxLevel);
                itemManager.CreateProducerItem(position, producerLevel, producerProps);
                producerCount++;
                break;
                
            case ItemType.Chest:
                ChestItemProperties chestProps = randomLevelData.ChestProperties[Random.Range(0, randomLevelData.ChestProperties.Length)];
                int chestLevel = Random.Range(1, chestProps.maxLevel);
                itemManager.CreateChestItem(position, chestLevel, chestProps);
                chestCount++;
                break;
                
            case ItemType.ProducedItem:
                ProducedItemProperties producedProps = randomLevelData.ProducedItemProperties[Random.Range(0, randomLevelData.ProducedItemProperties.Length)];
                int producedLevel = Random.Range(1, producedProps.maxLevel);
                itemManager.CreateProducedItem(position, producedLevel, producedProps);
                producedItemCount++;
                break;
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

    private static void TryBlockCells(RandomLevelData randomLevelData, GridManager gridManager)
    {
        if (!randomLevelData.blockCells)
            return;
        
        int centerX = gridManager.GridSize.x / 2;
        int centerY = gridManager.GridSize.y / 2;
        
        int halfWidthX = randomLevelData.unblockCenter.x / 2;
        int halfHeightY = randomLevelData.unblockCenter.y / 2;
        
        int startX = centerX - halfWidthX;
        int endX = centerX + halfWidthX + (randomLevelData.unblockCenter.x % 2);
        int startY = centerY - halfHeightY;
        int endY = centerY + halfHeightY + (randomLevelData.unblockCenter.y % 2);
        
        for (int x = 0; x < gridManager.GridSize.x; x++)
        {
            for (int y = 0; y < gridManager.GridSize.y; y++)
            {
                if (randomLevelData.unblockCenter.x <= 0 || randomLevelData.unblockCenter.y <= 0 || 
                    x < startX || x >= endX || y < startY || y >= endY)
                {
                    gridManager.BlockCell(new Vector2Int(x, y));
                }
            }
        }
    }
}
