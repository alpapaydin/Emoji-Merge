using UnityEngine;
using System.Collections.Generic;

public class ChestItem : ContainerItem
{
    private bool isLocked = true;
    private float unlockProgress = 0f;
    private bool isUnlocking = false;
    
    public bool IsLocked => isLocked;
    public float UnlockProgress => unlockProgress / CurrentLevelData.rechargeTime;
    public bool IsUnlocking => isUnlocking;
    
    private ChestItemProperties ChestProperties => properties as ChestItemProperties;
    
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
        if (!base.CanPerformAction())
            return false;

        return !isLocked;
    }

    public override void OnTapped()
    {
        base.OnTapped();
        if (!CanPerformAction()) return;

        GameManager.Instance.ConsumeEnergy(ContainerProperties.energyCost);
        var selectedItem = SelectItemToSpawn();
        if (selectedItem.HasValue)
        {
            SpawnSingleItem(selectedItem.Value);
            if (!HasItemsLeft())
            {
                Destroy(gameObject);
            }
        }
    }

    private bool HasItemsLeft()
    {
        foreach (var levelInventory in inventoryItemCounts.Values)
        {
            foreach (var count in levelInventory.Values)
            {
                if (count > 0) return true;
            }
        }
        return false;
    }

    private void StartUnlocking()
    {
        if (!isUnlocking)
        {
            isUnlocking = true;
            unlockProgress = 0f;
            ShowParticleEffect("unlock_start");
            NotifyStateChanged();
        }
    }

    private void UpdateUnlock()
    {
        if (isUnlocking && isLocked)
        {
            float oldProgress = unlockProgress;
            unlockProgress += Time.deltaTime;
            
            if (Mathf.FloorToInt(oldProgress) != Mathf.FloorToInt(unlockProgress))
            {
                NotifyStateChanged();
            }
            
            if (unlockProgress >= CurrentLevelData.rechargeTime)
            {
                CompleteUnlock();
            }
        }
    }

    private void CompleteUnlock()
    {
        isUnlocking = false;
        isLocked = false;
        unlockProgress = CurrentLevelData.rechargeTime;
        ShowParticleEffect("ready");
        NotifyStateChanged();
    }

    protected override void Update()
    {
        UpdateUnlock();
        base.Update();
    }
}