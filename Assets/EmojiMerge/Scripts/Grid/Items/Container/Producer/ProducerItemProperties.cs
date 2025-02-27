using UnityEngine;

[CreateAssetMenu(fileName = "ProducerItemProperties", menuName = "Game/Items/Producer Properties")]
public class ProducerItemProperties : ContainerItemDefinition
{
    protected override void OnEnable()
    {
        itemType = ItemType.Producer;
        maxLevel = 5;
        base.OnEnable();
    }
}