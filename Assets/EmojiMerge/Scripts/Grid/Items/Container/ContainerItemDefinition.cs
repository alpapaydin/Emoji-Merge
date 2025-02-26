using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContainerItemProperties", menuName = "Game/Items/Container Properties")]
public class ContainerItemDefinition : BaseItemProperties
{
    // Item property definition which chest and producer will inherit from. move inventory and recharge related properties here.
    // make another struct ContainerItemLevel, which will hold contained item type (base item definition), item level, item count and recharge times for each level of item.
    // also refactor resource items to inherit from produced items
}   
