using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContainerItemProperties", menuName = "Game/Items/Container Properties")]
public class ContainerItemDefinition : BaseItemProperties
{
    // Item property definition which chest and producer will inherit from. move inventory and recharge related properties here.
    // make another struct ContainerItemLevel, which will hold contained items and recharge times for each level of item.
}
