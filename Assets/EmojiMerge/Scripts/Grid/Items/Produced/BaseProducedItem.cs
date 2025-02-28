using UnityEngine;

public abstract class BaseProducedItem : GridItem
{
    
    public override bool CanPerformAction()
    {
        return IsReadyToMerge;
    }

    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        base.Initialize(props, level);
        ShowParticleEffect("spawn");
    }
}