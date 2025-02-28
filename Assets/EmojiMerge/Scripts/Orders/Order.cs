using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public event System.Action<Order> OnItemsUpdated;
    public OrderData data;
    private Dictionary<(BaseItemProperties itemDef, int level), int> remainingItems;
    private List<GridItem> markedItems = new List<GridItem>();
    private bool canBeCompleted;
    private Sprite customerSprite;

    public bool CanBeCompleted => canBeCompleted;
    public Sprite CustomerSprite => customerSprite;
    public bool IsCompleted => remainingItems.Count == 0;

    public Order(OrderData orderData, Sprite sprite)
    {
        data = orderData;
        customerSprite = sprite;
        InitializeRemainingItems();
    }

    private void InitializeRemainingItems()
    {
        remainingItems = new Dictionary<(BaseItemProperties itemDef, int level), int>();
        foreach (var requirement in data.requiredItems)
        {
            if (requirement.itemDefinition != null)
            {
                remainingItems[(requirement.itemDefinition, requirement.level)] = requirement.count;
            }
        }
    }

    public void ClearMarkedItems()
    {
        foreach (var item in markedItems.ToList())
        {
            if (item != null)
            {
                item.RemoveOrderMark(this);
            }
        }
        markedItems.Clear();
        UpdateCompletionStatus();
    }

    public bool AddMarkedItem(GridItem item)
    {
        if (item == null) return false;

        var key = (item.properties, item.CurrentLevel);
        if (!remainingItems.ContainsKey(key)) return false;

        int currentCount = markedItems.Count(x => 
            x != null && 
            x.properties == key.Item1 && 
            x.CurrentLevel == key.Item2);

        if (currentCount >= remainingItems[key]) return false;

        if (item.AddOrderMark(this))
        {
            markedItems.Add(item);
            OnItemsUpdated?.Invoke(this);
            UpdateCompletionStatus();
            return true;
        }
        return false;
    }

    public void OnMarkedItemDestroyed(GridItem item)
    {
        if (markedItems.Remove(item))
        {
            canBeCompleted = false;
            UpdateCompletionStatus();
            
            TryMarkAvailableItems();
        }
    }

    public IReadOnlyDictionary<(BaseItemProperties itemDef, int level), int> GetRemainingItems()
    {
        return remainingItems;
    }

    public IReadOnlyList<GridItem> GetMarkedItems()
    {
        return markedItems.Where(x => x != null).ToList();
    }

    public bool TryMarkAvailableItems()
    {
        if (GridManager.Instance == null) return false;
        bool statusChanged = false;
        bool wasComplete = canBeCompleted;
        var unavailableItems = new HashSet<GridItem>();
        var currentlyMarkedItems = new HashSet<GridItem>(markedItems);

        foreach (var item in currentlyMarkedItems)
        {
            if (!IsItemNeededForCompletion(item))
            {
                item.RemoveOrderMark(this);
                statusChanged = true;
            }
        }

        foreach (var requirement in remainingItems)
        {
            var key = requirement.Key;
            int neededCount = requirement.Value;

            var currentMarkedForType = markedItems.Where(x => 
                x != null && 
                x.properties == key.Item1 && 
                x.CurrentLevel == key.Item2).ToList();
            
            int currentCount = currentMarkedForType.Count;
            
            if (currentCount >= neededCount) continue;

            var availableItems = GridManager.Instance.FindItemsWithLevel(key.Item1.itemType, key.Item2)
                .Where(item => item != null && !unavailableItems.Contains(item))
                .OrderBy(item => item.IsMarkedForDelivery)
                .ToList();

            foreach (var item in availableItems)
            {
                if (currentCount >= neededCount) break;

                if (currentMarkedForType.Contains(item)) continue;

                if (AddMarkedItemWithoutNotify(item))
                {
                    currentCount++;
                    statusChanged = true;
                }
                else
                {
                    unavailableItems.Add(item);
                }
            }
        }

        UpdateCompletionStatus();
        bool completionChanged = wasComplete != canBeCompleted;
        
        if (statusChanged || completionChanged)
        {
            OnItemsUpdated?.Invoke(this);
        }

        return statusChanged || completionChanged;
    }

    public void UpdateCompletionStatus()
    {
        bool wasComplete = canBeCompleted;
        canBeCompleted = CheckAllRequirementsmet();
        
        if (wasComplete != canBeCompleted || !canBeCompleted)
        {
            OnItemsUpdated?.Invoke(this);
        }
    }

    private bool CheckAllRequirementsmet()
    {
        foreach (var requirement in remainingItems)
        {
            var key = requirement.Key;
            int neededCount = requirement.Value;

            int markedCount = markedItems.Count(item => 
                item != null && 
                item.properties == key.Item1 && 
                item.CurrentLevel == key.Item2);

            if (markedCount < neededCount)
                return false;
        }
        return true;
    }

    public bool RequiresItem(BaseItemProperties itemDef, int level)
    {
        return remainingItems.ContainsKey((itemDef, level));
    }

    private bool AddMarkedItemWithoutNotify(GridItem item)
    {
        if (item == null) return false;

        var key = (item.properties, item.CurrentLevel);
        if (!remainingItems.ContainsKey(key)) return false;

        int currentCount = markedItems.Count(x => 
            x != null && 
            x.properties == key.Item1 && 
            x.CurrentLevel == key.Item2);

        if (currentCount >= remainingItems[key]) return false;

        if (item.AddOrderMark(this))
        {
            markedItems.Add(item);
            return true;
        }
        return false;
    }

    private bool IsItemNeededForCompletion(GridItem item)
    {
        if (item == null) return false;

        var key = (item.properties, item.CurrentLevel);
        if (!remainingItems.ContainsKey(key)) return false;

        int neededCount = remainingItems[key];
        int currentCount = markedItems.Count(x => 
            x != null && 
            x.properties == key.Item1 && 
            x.CurrentLevel == key.Item2);

        return currentCount <= neededCount;
    }
}