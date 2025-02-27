using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct Order
{
    public ItemLevelCount[] requiredItems;
    public int RewardGold => 1;

    public Order(ItemLevelCount[] requiredItems)
    {
        this.requiredItems = requiredItems;
    }
}