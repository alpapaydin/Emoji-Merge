using UnityEngine;

[CreateAssetMenu(fileName = "ChestItemProperties", menuName = "Game/Items/Chest Properties")]
public class ChestItemProperties : ContainerItemDefinition
{    
    protected override void OnEnable()
    {
        itemType = ItemType.Chest;
        maxLevel = 3;
        base.OnEnable();
    }
}