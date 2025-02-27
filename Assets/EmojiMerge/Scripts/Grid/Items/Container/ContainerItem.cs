using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContainerItem : GridItem
{
    protected Dictionary<int, Dictionary<BaseItemProperties, int>> inventoryItemCounts = new Dictionary<int, Dictionary<BaseItemProperties, int>>();
    protected bool isRecharging = false;
    protected float rechargeProgress = 0f;
    
    protected ContainerItemDefinition ContainerProperties => properties as ContainerItemDefinition;
    protected ContainerItemLevel CurrentLevelData => ContainerProperties.GetLevelData(currentLevel);
    
    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ContainerItemDefinition))
        {
            Debug.LogError($"Incorrect properties type provided to {GetType().Name}");
            return;
        }

        base.Initialize(props, level);
        InitializeInventory();
        
        isReadyToMerge = currentLevel < props.maxLevel;

        if (CurrentLevelData.canSpawnItems)
        {
            FillInventory();
        }
    }

    protected virtual void InitializeInventory()
    {
        inventoryItemCounts.Clear();
        if (CurrentLevelData.itemCapacities != null)
        {
            foreach (var capacity in CurrentLevelData.itemCapacities)
            {
                if (!inventoryItemCounts.ContainsKey(capacity.level))
                {
                    inventoryItemCounts[capacity.level] = new Dictionary<BaseItemProperties, int>();
                }
                inventoryItemCounts[capacity.level][capacity.itemDefinition] = 0;
            }
        }
    }

    public override bool CanPerformAction()
    {
        if (InputManager.IsDragging)
            return false;
            
        if (CurrentLevelData.canSpawnItems)
        {
            bool hasItems = false;
            foreach (var levelItems in inventoryItemCounts.Values)
            {
                foreach (var count in levelItems.Values)
                {
                    if (count > 0)
                    {
                        hasItems = true;
                        break;
                    }
                }
                if (hasItems) break;
            }
            if (!hasItems) return false;
            
            if (!GameManager.Instance.HasEnoughEnergy(ContainerProperties.energyCost)) 
                return false;
                
            return true;
        }
        
        return false;
    }

    protected bool HasEmptyInventorySlots()
    {
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            if (inventoryItemCounts[capacity.level][capacity.itemDefinition] < capacity.count)
            {
                return true;
            }
        }
        return false;
    }

    protected virtual void FillInventory()
    {
        bool inventoryChanged = false;
        
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            int currentCount = inventoryItemCounts[capacity.level][capacity.itemDefinition];
            if (currentCount < capacity.count)
            {
                inventoryItemCounts[capacity.level][capacity.itemDefinition] = capacity.count;
                inventoryChanged = true;
            }
        }

        if (HasEmptyInventorySlots())
        {
            StartRecharge();
        }
        else if (inventoryChanged)
        {
            ShowParticleEffect("ready");
        }
    }

    protected virtual void StartRecharge()
    {
        if (!isRecharging)
        {
            isRecharging = true;
            rechargeProgress = 0f;
            ShowParticleEffect("recharge_start");
        }
    }

    protected virtual void UpdateRecharge()
    {
        if (isRecharging)
        {
            rechargeProgress += Time.deltaTime;
            if (rechargeProgress >= CurrentLevelData.rechargeTime)
            {
                CompleteRecharge();
            }
        }
    }

    protected virtual void CompleteRecharge()
    {
        isRecharging = false;
        rechargeProgress = 0f;
        FillInventory();
        ShowParticleEffect("ready");
    }

    protected virtual void Update()
    {
        UpdateRecharge();
    }

    protected virtual void SpawnSingleItem(ContainerItemCapacity capacity)
    {
        if (inventoryItemCounts[capacity.level][capacity.itemDefinition] > 0)
        {
            GridItem newItem = null;

            if (capacity.itemDefinition is ResourceItemProperties)
            {
                newItem = ItemManager.Instance.CreateResourceItem(gridPosition, capacity.itemDefinition, capacity.level);
            }
            else if (capacity.itemDefinition is ProducedItemProperties)
            {
                newItem = ItemManager.Instance.CreateProducedItemWithAnimation(gridPosition, capacity.level, transform.position);
            }

            if (newItem != null)
            {
                inventoryItemCounts[capacity.level][capacity.itemDefinition]--;
                OnItemSpawned();
            }
        }
    }

    protected virtual ContainerItemCapacity? SelectItemToSpawn()
    {
        List<ContainerItemCapacity> availableSlots = new List<ContainerItemCapacity>();
        
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            if (inventoryItemCounts[capacity.level][capacity.itemDefinition] > 0)
            {
                availableSlots.Add(capacity);
            }
        }

        if (availableSlots.Count > 0)
        {
            return availableSlots[Random.Range(0, availableSlots.Count)];
        }

        return null;
    }

    protected virtual void OnItemSpawned()
    {
        if (!isRecharging && HasEmptyInventorySlots())
        {
            StartRecharge();
        }
    }
}
