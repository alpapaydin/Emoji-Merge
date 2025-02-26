using UnityEngine;
using System.Collections.Generic;

public class ProducerItem : GridItem
{
    private bool isReadyToProduce = true;
    private Dictionary<int, int> inventoryItemCounts = new Dictionary<int, int>();
    private bool isRecharging = false;
    private float rechargeProgress = 0f;
    
    private ProducerItemProperties ProducerProperties => properties as ProducerItemProperties;
    private ProducerLevel CurrentLevelData => ProducerProperties.GetLevelData(currentLevel);
    
    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ProducerItemProperties))
        {
            Debug.LogError("Incorrect properties type provided to ProducerItem");
            return;
        }

        base.Initialize(props, level);
        InitializeInventory();
        
        isReadyToMerge = currentLevel < props.maxLevel;
        isReadyToProduce = true;

        if (CurrentLevelData.canProduce)
        {
            FillInventory();
        }
    }

    private void InitializeInventory()
    {
        inventoryItemCounts.Clear();
        if (CurrentLevelData.itemCapacities != null)
        {
            foreach (var capacity in CurrentLevelData.itemCapacities)
            {
                inventoryItemCounts[capacity.level] = 0;
            }
        }
    }

    public override bool CanPerformAction()
    {
        if (InputManager.IsDragging)
            return false;
            
        if (CurrentLevelData.canProduce)
        {
            bool hasItems = false;
            foreach (var count in inventoryItemCounts.Values)
            {
                if (count > 0)
                {
                    hasItems = true;
                    break;
                }
            }
            if (!hasItems) return false;
            
            if (!GameManager.Instance.HasEnoughEnergy(ProducerProperties.energyCost)) 
                return false;
                
            return isReadyToProduce;
        }
        
        return false;
    }

    public override void OnTapped()
    {
        if (!CanPerformAction()) return;

        GameManager.Instance.ConsumeEnergy(ProducerProperties.energyCost);
        SpawnOneItemFromInventory();
        
        if (HasEmptyInventorySlots())
        {
            StartRecharge();
        }
    }

    private bool HasEmptyInventorySlots()
    {
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            if (inventoryItemCounts[capacity.level] < capacity.count)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnOneItemFromInventory()
    {
        List<ProducerItemCapacity> availableSlots = new List<ProducerItemCapacity>();
        
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            if (inventoryItemCounts[capacity.level] > 0)
            {
                availableSlots.Add(capacity);
            }
        }

        if (availableSlots.Count > 0)
        {
            var selectedCapacity = availableSlots[Random.Range(0, availableSlots.Count)];
            
            GridItem newItem = ItemManager.Instance.CreateProducedItem(gridPosition, selectedCapacity.level);
            if (newItem != null)
            {
                inventoryItemCounts[selectedCapacity.level]--;
                
                if (!isRecharging && HasEmptyInventorySlots())
                {
                    StartRecharge();
                }
            }
        }
    }

    private void StartRecharge()
    {
        if (!isRecharging)
        {
            isRecharging = true;
            rechargeProgress = 0f;
            ShowParticleEffect("recharge_start");
        }
    }

    private void UpdateRecharge()
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

    private void CompleteRecharge()
    {
        isRecharging = false;
        rechargeProgress = 0f;
        FillInventory();
        ShowParticleEffect("ready");
    }

    private void FillInventory()
    {
        bool inventoryChanged = false;
        
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            int currentCount = inventoryItemCounts[capacity.level];
            if (currentCount < capacity.count)
            {
                inventoryItemCounts[capacity.level] = capacity.count;
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

    private void Update()
    {
        UpdateRecharge();
    }
}