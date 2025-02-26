using UnityEngine;

[CreateAssetMenu(fileName = "ResourceItemProperties", menuName = "Game/Items/Resource Properties")]
public class ResourceItemProperties : BaseItemProperties
{
    public float[] resourceAmounts;
    
    private void OnEnable()
    {
        maxLevel = 3;
        if (resourceAmounts == null || resourceAmounts.Length != maxLevel)
        {
            resourceAmounts = new float[maxLevel];
            for (int i = 0; i < maxLevel; i++)
            {
                resourceAmounts[i] = (i + 1) * 10f;
            }
        }
    }
}