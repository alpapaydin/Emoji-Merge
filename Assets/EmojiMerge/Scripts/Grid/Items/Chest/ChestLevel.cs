using UnityEngine;
using System;

[Serializable]
public struct ChestResourceDrop
{
    public ItemType resourceType;
    public int level;
    public int count;
}

[Serializable]
public struct ChestLevel
{
    public ChestResourceDrop[] resourceDrops;
    public float unlockTime;

    public int TotalCapacity
    {
        get
        {
            int total = 0;
            if (resourceDrops != null)
            {
                foreach (var drop in resourceDrops)
                {
                    total += drop.count;
                }
            }
            return total;
        }
    }
}