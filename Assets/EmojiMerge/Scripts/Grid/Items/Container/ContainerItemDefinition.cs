using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct ContainerItemCapacity
{
    public BaseItemProperties itemDefinition;
    public int level;
    public int count;
}

[Serializable]
public struct ContainerItemLevel
{
    public float rechargeTime;
    public ContainerItemCapacity[] itemCapacities;
    public bool canSpawnItems;

    public int TotalCapacity
    {
        get
        {
            int total = 0;
            if (itemCapacities != null)
            {
                foreach (var capacity in itemCapacities)
                {
                    total += capacity.count;
                }
            }
            return total;
        }
    }
}

[CreateAssetMenu(fileName = "ContainerItemProperties", menuName = "Game/Items/Container Properties")]
public class ContainerItemDefinition : BaseItemProperties
{
    public float energyCost;
    public ContainerItemLevel[] levels;

    protected virtual void OnEnable()
    {
        if (levels == null || levels.Length != maxLevel)
        {
            levels = new ContainerItemLevel[maxLevel];
            InitializeDefaultLevels();
        }
    }

    protected virtual void OnValidate()
    {
        if (levels == null || levels.Length != maxLevel)
        {
            levels = new ContainerItemLevel[maxLevel];
            InitializeDefaultLevels();
        }
    }

    protected virtual void InitializeDefaultLevels()
    {
    }

    public ContainerItemLevel GetLevelData(int level)
    {
        return levels[Mathf.Clamp(level - 1, 0, levels.Length - 1)];
    }
}
