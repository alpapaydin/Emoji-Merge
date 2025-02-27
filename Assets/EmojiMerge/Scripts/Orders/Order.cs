using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public OrderData data;
    private Dictionary<(BaseItemProperties itemDef, int level), int> remainingItems;
    private List<GridItem> markedItems = new List<GridItem>();
    private bool canBeCompleted;
    private Sprite customerSprite;

    public bool CanBeCompleted => canBeCompleted;
    public Sprite CustomerSprite => customerSprite;

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
            remainingItems[(requirement.itemDefinition, requirement.level)] = requirement.count;
        }
    }

    public bool IsCompleted => remainingItems.Count == 0;

    public bool TrySubmitItem(GridItem item)
    {
        if (item == null) return false;
        
        var key = (item.properties, item.CurrentLevel);
        if (remainingItems.TryGetValue(key, out int remaining))
        {
            remaining--;
            if (remaining <= 0)
            {
                remainingItems.Remove(key);
            }
            else
            {
                remainingItems[key] = remaining;
            }
            return true;
        }
        return false;
    }

    public IReadOnlyDictionary<(BaseItemProperties itemDef, int level), int> GetRemainingItems()
    {
        return remainingItems;
    }

    public void ClearMarkedItems()
    {
        foreach (var item in markedItems)
        {
            if (item != null)
            {
                item.IsMarkedForDelivery = false;
            }
        }
        markedItems.Clear();
    }

    public void AddMarkedItem(GridItem item)
    {
        if (!markedItems.Contains(item))
        {
            markedItems.Add(item);
            item.IsMarkedForDelivery = true;
        }
    }

    private void OnItemDestroyed(GridItem item)
    {
        markedItems.Remove(item);
    }

    public IReadOnlyList<GridItem> GetMarkedItems()
    {
        return markedItems;
    }

    public void OnItemChanged(GridItem item)
    {
        if (item == null) return;

        var key = (item.properties, item.CurrentLevel);
        if (remainingItems.ContainsKey(key))
        {
            UpdateCompletionStatus();
        }
    }

    public void UpdateCompletionStatus()
    {
        ClearMarkedItems();

        canBeCompleted = CanFulfillRequirements();
    }

    private bool CanFulfillRequirements()
    {
        Dictionary<(BaseItemProperties, int), List<GridItem>> availableItems = new Dictionary<(BaseItemProperties, int), List<GridItem>>();

        foreach (var requirement in data.requiredItems)
        {
            var key = (requirement.itemDefinition, requirement.level);
            var items = GridManager.Instance.FindItemsWithLevel(requirement.itemDefinition.itemType, requirement.level)
                .Where(item => !item.IsMarkedForDelivery)
                .ToList();

            if (items.Count < requirement.count)
            {
                return false;
            }

            availableItems[key] = items;
        }

        foreach (var requirement in data.requiredItems)
        {
            var key = (requirement.itemDefinition, requirement.level);
            var items = availableItems[key];

            for (int i = 0; i < requirement.count; i++)
            {
                AddMarkedItem(items[i]);
            }
        }

        return true;
    }
}