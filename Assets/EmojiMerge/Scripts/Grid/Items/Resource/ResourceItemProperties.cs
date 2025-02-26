using UnityEngine;

[CreateAssetMenu(fileName = "ResourceItemProperties", menuName = "Game/Items/Resource Properties")]
public class ResourceItemProperties : BaseItemProperties
{
    public int[] resourceAmounts; 

    private void OnEnable()
    {
        maxLevel = 3;
    }

    private void OnValidate()
    {
        if (resourceAmounts == null || resourceAmounts.Length != maxLevel)
        {
            resourceAmounts = new int[maxLevel];
        }
    }
}