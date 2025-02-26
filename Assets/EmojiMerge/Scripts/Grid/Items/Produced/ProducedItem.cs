using UnityEngine;

public class ProducedItem : GridItem
{
    private ProducedItemProperties ProducedProperties => properties as ProducedItemProperties;

    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ProducedItemProperties))
        {
            Debug.LogError("Incorrect properties type provided to ProducedItem");
            return;
        }

        base.Initialize(props, level);
        ShowParticleEffect("spawn");
    }

    public override bool CanPerformAction()
    {
        return IsReadyToMerge;
    }
}