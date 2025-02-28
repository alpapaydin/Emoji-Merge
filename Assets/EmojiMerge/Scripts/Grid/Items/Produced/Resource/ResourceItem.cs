using UnityEngine;

public class ResourceItem : BaseProducedItem
{
    private ResourceItemProperties ResourceProperties => properties as ResourceItemProperties;
    public override void Initialize(BaseItemProperties props, int level = 1)
    {
        if (!(props is ResourceItemProperties))
        {
            Debug.LogError("Incorrect properties type provided to ResourceItem");
            return;
        }

        base.Initialize(props, level);
    }

    public override void OnTapped()
    {
        base.OnTapped();
        if (!CanPerformAction()) return;

        switch (properties.itemType)
        {
            case ItemType.Energy:
                GameManager.Instance.AddEnergy(ResourceProperties.resourceAmounts[currentLevel - 1]);
                break;
            case ItemType.Coin:
                GameManager.Instance.AddCoins(Mathf.RoundToInt(ResourceProperties.resourceAmounts[currentLevel - 1]));
                break;
        }

        isConsumed = true;
        ShowParticleEffect("consume");
        StartCoroutine(DestroyWithAnimation());
    }

    private System.Collections.IEnumerator DestroyWithAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            t = t * t * (3f - 2f * t);
            
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            yield return null;
        }

        Destroy(gameObject);
    }
}