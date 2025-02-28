using UnityEngine;

public abstract class BaseProducedItem : GridItem
{
    protected bool isConsumed = false;
    
    public override bool CanPerformAction()
    {
        return !isConsumed && IsReadyToMerge;
    }

    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        base.Initialize(props, level);
        ShowParticleEffect("spawn");
    }
}