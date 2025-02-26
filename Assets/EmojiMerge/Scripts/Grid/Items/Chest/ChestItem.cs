using UnityEngine;
using System.Collections.Generic;

public class ChestItem : GridItem
{
    private bool isLocked = true;
    private float unlockProgress = 0f;
    private bool isUnlocking = false;
    
    private ChestItemProperties ChestProperties => properties as ChestItemProperties;
    private ChestLevel CurrentLevelData => ChestProperties.GetLevelData(currentLevel);

    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ChestItemProperties))
        {
            Debug.LogError("Incorrect properties type provided to ChestItem");
            return;
        }

        base.Initialize(props, level);
        StartUnlocking();
    }

    public override bool CanPerformAction()
    {
        return !isLocked;
    }

    public override void OnTapped()
    {
        if (isLocked) return;
        
        SpawnContainedItems();
    }

    private void SpawnContainedItems()
    {
        bool anyItemSpawned = false;
        foreach (var drop in CurrentLevelData.resourceDrops)
        {
            for (int i = 0; i < drop.count; i++)
            {
                GridItem newItem = ItemManager.Instance.CreateResourceItem(gridPosition, drop.resourceType, drop.level);
                if (newItem != null)
                {
                    anyItemSpawned = true;
                }
            }
        }

        if (anyItemSpawned)
        {
            // destroy chest after successfully spawning at least one item? or stays
            Destroy(gameObject);
        }
    }

    private void StartUnlocking()
    {
        isUnlocking = true;
        unlockProgress = 0f;
        ShowParticleEffect("unlock_start");
    }

    private void UpdateUnlock()
    {
        if (isUnlocking && isLocked)
        {
            unlockProgress += Time.deltaTime;
            if (unlockProgress >= CurrentLevelData.unlockTime)
            {
                CompleteUnlock();
            }
        }
    }

    private void CompleteUnlock()
    {
        isUnlocking = false;
        isLocked = false;
        ShowParticleEffect("ready");
    }

    private void Update()
    {
        UpdateUnlock();
    }
}