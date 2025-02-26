using UnityEngine;
using System.Collections.Generic;

public class ProducerItem : GridItem
{
    private bool isReadyToProduce = false;
    private Dictionary<int, int> producedItemCounts = new Dictionary<int, int>();
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
        InitializeProducedItems();
        
        if (CurrentLevelData.canProduce)
        {
            StartRecharge();
        }
    }

    private void InitializeProducedItems()
    {
        producedItemCounts.Clear();
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            producedItemCounts[capacity.level] = 0;
        }
    }

    public override bool CanPerformAction()
    {
        if (!CurrentLevelData.canProduce) return false;
        // also check grid capacity for items
        // check if we have capacity for any item level
        bool hasCapacity = false;
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            if (producedItemCounts.TryGetValue(capacity.level, out int count))
            {
                if (count < capacity.count)
                {
                    hasCapacity = true;
                    break;
                }
            }
        }
        if (!hasCapacity) return false;
        
        // check if we have enough energy
        if (!GameManager.Instance.HasEnoughEnergy(ProducerProperties.energyCost)) return false;
        
        return isReadyToProduce && !isRecharging;
    }

    public override void OnTapped()
    {
        if (!CanPerformAction()) return;

        GameManager.Instance.ConsumeEnergy(ProducerProperties.energyCost);
        ProduceItem();
        
        StartRecharge();
    }

    private void ProduceItem()
    {
        List<ProducerItemCapacity> availableSlots = new List<ProducerItemCapacity>();
        foreach (var capacity in CurrentLevelData.itemCapacities)
        {
            if (producedItemCounts[capacity.level] < capacity.count)
            {
                availableSlots.Add(capacity);
            }
        }

        if (availableSlots.Count == 0) return;

        // randomly select one of the available slots
        var selectedCapacity = availableSlots[Random.Range(0, availableSlots.Count)];
        
        GridItem newItem = ItemManager.Instance.CreateProducedItem(gridPosition, selectedCapacity.level);
        
        if (newItem != null)
        {
            // grid capacity checking by trial and error
            producedItemCounts[selectedCapacity.level]++;
        }
    }

    private void StartRecharge()
    {
        isRecharging = true;
        rechargeProgress = 0f;
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
        isReadyToProduce = true;
        InitializeProducedItems();
        ShowParticleEffect("ready");
    }

    private void Update()
    {
        UpdateRecharge();
    }
}