using UnityEngine;

[CreateAssetMenu(fileName = "ProducedItemProperties", menuName = "Game/Items/Produced Item Properties")]
public class ProducedItemProperties : BaseItemProperties
{
    private void OnEnable()
    {
        itemType = ItemType.ProducedItem;
        maxLevel = 5;
    }
}