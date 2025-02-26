using UnityEngine;

public class ResourceItem : GridItem
{
    private bool isConsumed = false;
    private ResourceItemProperties ResourceProperties => properties as ResourceItemProperties;

    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ResourceItemProperties))
        {
            Debug.LogError("Incorrect properties type provided to ResourceItem");
            return;
        }

        base.Initialize(props, level);
        ShowParticleEffect("spawn");
    }

    public override bool CanPerformAction()
    {
        return !isConsumed;
    }

    public override void OnTapped()
    {
        if (!CanPerformAction()) return;

        switch (properties.itemType)
        {
            case ItemType.Energy:
                GameManager.Instance.AddEnergy(ResourceProperties.resourceAmounts[currentLevel - 1]);
                break;
            case ItemType.Coin:
                GameManager.Instance.AddCoins(ResourceProperties.resourceAmounts[currentLevel - 1]);
                break;
        }

        isConsumed = true;
        ShowParticleEffect("consume");
        Destroy(gameObject, 0.5f);
    }
}