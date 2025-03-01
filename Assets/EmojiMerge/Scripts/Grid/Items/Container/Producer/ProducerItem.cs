using UnityEngine;
using System.Collections.Generic;

public class ProducerItem : ContainerItem
{
    private bool isReadyToProduce = true;
    
    private ProducerItemProperties ProducerProperties => properties as ProducerItemProperties;
    
    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ProducerItemProperties))
        {
            Debug.LogError("Incorrect properties type provided to ProducerItem");
            return;
        }

        base.Initialize(props, level);
        isReadyToProduce = true;
    }

    public override bool CanPerformAction()
    {
        if (!base.CanPerformAction())
            return false;
                
        return isReadyToProduce;
    }

    public override void OnTapped()
    {
        base.OnTapped();
        if (!CanPerformAction()) return;

        var selectedItem = SelectItemToSpawn();
        if (selectedItem.HasValue)
        {
            bool isSpawned = SpawnSingleItem(selectedItem.Value);
            if (isSpawned)
            {
                GameManager.Instance.ConsumeEnergy(ContainerProperties.energyCost);
            }
        }
    }
    
}